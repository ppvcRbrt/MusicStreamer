using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MusicStreamerBackend.Controllers;
using MusicStreamerBackend.Data;
using MusicStreamerBackend.Data.EFModels.Music;
using MusicStreamerBackend.Helpers;
using MusicStreamerBackend.Models.MusicBrainz;

namespace MusicStreamerBackend.Services;

public interface IExternalMetadataService
{
    Task<Dictionary<int, MusicBrainzSearchResult?>> FindArtistsMissingExternalMetadata(int maxResults = 5);
    Task<Dictionary<int, MusicBrainzReleaseGroup>?> FindAlbumsMissingReleaseGroups(int dbArtistId);
    Task<int> StoreMissingCoverArt();
}

public class ExternalMetadataService: IExternalMetadataService
{
    private readonly MusicStreamerDbContext _dbContext;
    private readonly IMusicBrainzService _musicBrainzService;
    private readonly IFileService _fileService;
    private readonly ILogger<ExternalMetadataController> _logger;
    private readonly IDbStorageService _dbStorageService;
    private readonly string _coverArtRootPath;
    
    public ExternalMetadataService(
        MusicStreamerDbContext dbContext, 
        IMusicBrainzService musicBrainzService, 
        ILogger<ExternalMetadataController> logger,
        IFileService fileService,
        IDbStorageService dbStorageService,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _musicBrainzService = musicBrainzService;
        _logger = logger;
        _fileService = fileService;
        _dbStorageService = dbStorageService;
        _coverArtRootPath = configuration["CoverArtRootPath"] ?? "/covers";
    }
    
    public async Task<Dictionary<int, MusicBrainzSearchResult?>> FindArtistsMissingExternalMetadata(int maxResults = 5)
    {
        var artistsWithoutExtMetadata = await _dbContext.Artists
            .Include(a => a.ExtIds) 
            .Where(a => a.ExtIds == null || !a.ExtIds.Any())
            .ToListAsync();
        
        _logger.LogInformation("Found {Count} artists without external metadata", artistsWithoutExtMetadata.Count);
        
        var result = new Dictionary<int, MusicBrainzSearchResult?>();
        foreach (var artist in artistsWithoutExtMetadata)
        {
            var searchResult = await _musicBrainzService.SearchArtists(RemoveArticlesAndCommas(artist.Name), maxResults);
            if(searchResult != null && searchResult.Artists != null)
            {
                _logger.LogInformation("Found {Count} search results for artist '{ArtistName}'", searchResult.Artists.Count, artist.Name);
                result[artist.Id] = searchResult;
            }
            else
            {
                _logger.LogInformation("No search results found for artist '{ArtistName}'", artist.Name);
            }
        }
        _logger.LogInformation("Returning search results for {Count} artists with a limit of {Limit} per artist", result.Count, maxResults);
        return result;
    }

    public async Task<Dictionary<int, MusicBrainzReleaseGroup>?> FindAlbumsMissingReleaseGroups(int dbArtistId)  
    {
        var dbArtist = await _dbContext.Artists
            .Include(a => a.ExtIds)
            .FirstOrDefaultAsync(a => a.Id == dbArtistId);
        
        if(dbArtist == null)        
        {
            _logger.LogError("Artist with ID {ArtistId} not found in database", dbArtistId);
            throw new Exception($"Artist with ID {dbArtistId} not found in database");
        }
        
        var albumsMissingReleaseGroups = await _dbContext.Albums
            .Include(a => a.ExtIds)
            .Where(a => a.ArtistId == dbArtistId && (a.ExtIds == null || !a.ExtIds.Any()))
            .ToListAsync();

        if (!albumsMissingReleaseGroups.Any())
            return null;

        if (dbArtist.ExtIds == null || !dbArtist.ExtIds.Any() || dbArtist.ExtIds.All(e => e.ServiceName != "MusicBrainz"))
        {
            _logger.LogError("Artist with ID {ArtistId} found in database, however no MusicBrainz metadata was available", dbArtistId);
            throw new Exception($"Artist with ID {dbArtistId} found in database, however no MusicBrainz metadata was available");
        }
        var mdbid = dbArtist.ExtIds!.FirstOrDefault(e => e.ServiceName == "MusicBrainz")!.ExtId;
        var releaseGroups = await _musicBrainzService.GetArtistReleaseGroups(mdbid);
        if(releaseGroups == null || releaseGroups.ReleaseGroups == null || !releaseGroups.ReleaseGroups.Any())
        {
            _logger.LogError("No release groups found for artist with MusicBrainz ID {Mdbid}", mdbid);
            throw new Exception($"No release groups found for artist with MusicBrainz ID {mdbid}");
        }
        return MatchDbAlbumsWithReleaseGroups(albumsMissingReleaseGroups, releaseGroups);
    }

    public async Task<int> StoreMissingCoverArt()
    {
        var existingAlbumIds = _fileService.GetStoredAlbumCoverIds();
        var missingAlbums = await _dbContext.Albums
            .Include(a => a.ExtIds)
            .Where(a => a.ExtIds != null && a.ExtIds.Any(e => e.ServiceName == "MusicBrainz"))
            .Where(a => !existingAlbumIds.Contains(a.Id))
            .ToListAsync();
        int totalStored = 0;
        
        _logger.LogInformation("Found {Count} albums without cover art", missingAlbums.Count);
        foreach (var album in missingAlbums)
        {
            var albumExtData = album.ExtIds!.FirstOrDefault(e => e.ServiceName == "MusicBrainz");
            if (albumExtData == null)
            {
                _logger.LogError("Album with ID {AlbumId} has no MusicBrainz metadata. This is most likely a bug.", album.Id);
                continue;
            }
            _logger.LogInformation("Downloading cover art for album '{AlbumName}'", album.Title);
            var coverArt = await _musicBrainzService.GetReleaseGroupCoverArt(albumExtData.ExtId);
            if (coverArt == null)
            {
                _logger.LogError("Failed to download cover art for album '{AlbumName}'", album.Title);
                continue;
            }
            _logger.LogInformation("Downloaded cover art for album '{AlbumName}'", album.Title);
            
            _logger.LogInformation("Storing cover art for album '{AlbumName}'", album.Title);
            bool stored = _fileService.StoreAlbumCover(album.Id, coverArt.Value.ImageBytes, coverArt.Value.Extension);
            if (stored)
            {
                totalStored++;
                await _dbStorageService.UpdateAlbumCoverImageUrl(album, $"{_coverArtRootPath}/{album.Id}{coverArt.Value.Extension}");
                _logger.LogInformation("Stored cover art for album '{AlbumName}'", album.Title);
            }
            else
            {
                _logger.LogError("Failed to store cover art for album '{AlbumName}'", album.Title);
            }
        }
        _logger.LogInformation("Stored cover art for {Count} albums", totalStored);
        
        _logger.LogInformation("Creating cover art variants");
        _fileService.CreateAlbumCoverVariants(150, 300);
        _logger.LogInformation("Created cover art variants");
        return totalStored;
    }
    
    private Dictionary<int, MusicBrainzReleaseGroup> MatchDbAlbumsWithReleaseGroups(
        List<AlbumEF> dbAlbums, MusicBrainzSearchResult releaseGroups)
    {
        var matchedReleaseGroups = new Dictionary<int, MusicBrainzReleaseGroup>();

        foreach (var dbAlbum in dbAlbums)
        {
            var normalisedAlbum = NormaliseAlbumTitle(dbAlbum.Title);
            MusicBrainzReleaseGroup? bestMatch = null;
            double bestSimilarity = 0;

            foreach (var rg in releaseGroups.ReleaseGroups)
            {
                var normalisedRg = NormaliseAlbumTitle(rg.Title);
                var similarity = Math.Max(
                    StringHelpers.Similarity(normalisedAlbum, normalisedRg),
                    StringHelpers.Similarity(
                        NormaliseAlbumTitle(StripParentheses(dbAlbum.Title)),
                        NormaliseAlbumTitle(StripParentheses(rg.Title)))
                );

                if (similarity > bestSimilarity)
                {
                    bestSimilarity = similarity;
                    bestMatch = rg;
                }
            }
            // 0.85 threshold - should be tweaked if we find too many false positives/negatives.
            if (bestMatch != null && bestSimilarity >= 0.85)
            {
                matchedReleaseGroups[dbAlbum.Id] = bestMatch;
            }
        }
        return matchedReleaseGroups;
    }
    
    private static string StripParentheses(string title)
    {
        return Regex.Replace(title, @"\s*[\(\[].*?[\)\]]", "").Trim();
    }
    
    private string NormaliseAlbumTitle(string title)
    {
        // strip edition/remaster suffixes
        title = Regex.Replace(title, @"\s*[\(\[].*?(remaster|edition|bonus|expanded|anniversary).*?[\)\]]",
            "", RegexOptions.IgnoreCase);

        // remove non-alphanumeric (keep spaces)
        title = Regex.Replace(title, @"[^\w\s]", "");

        return title.ToLowerInvariant().Trim();
    }

    private string RemoveArticlesAndCommas(string input)
    {
        input = Regex.Replace(input, @"\bthe\b", "", RegexOptions.IgnoreCase);
        input = Regex.Replace(input, @"\ban\b", "", RegexOptions.IgnoreCase);
        input = Regex.Replace(input, @"\ba\b", "", RegexOptions.IgnoreCase);
        input = input.Replace(",", "");
        return Regex.Replace(input.Trim(), @"\s+", " ");    
    }
}