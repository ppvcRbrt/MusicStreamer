using Microsoft.EntityFrameworkCore;
using MusicStreamerBackend.Data;
using MusicStreamerBackend.Data.EFModels.Music;
using MusicStreamerBackend.Helpers;
using MusicStreamerBackend.Models.Discogs;
using MusicStreamerBackend.Models.Scanning;

namespace MusicStreamerBackend.Services;

public interface IDbStorageService
{
    Task<TrackStoreResult> StoreTracks(List<TrackFile> trackFiles);
}
public class DbStorageService : IDbStorageService
{
    private readonly ILogger<FileService> _logger;
    private readonly MusicStreamerDbContext _dbContext;
    public DbStorageService(ILogger<FileService> logger, MusicStreamerDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<TrackStoreResult> StoreTracks(List<TrackFile> trackFiles)
    {
        var newTracks = await GetTrackFilesNotStored(trackFiles);
        var artists = BuildArtists(newTracks);
        var newArtists = await GetArtistsNotStored(artists);
        _dbContext.Artists.AddRange(newArtists);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Stored Artist info for {ArtistsCount} artists", newArtists.Count());
        
        var albums = new List<AlbumEF>();
        foreach (var artist in artists)
        {
            var artistAlbums = BuildAlbums(newTracks, artist);
            albums.AddRange(artistAlbums);
        }
        var newAlbums = await GetAlbumsNotStored(albums);
        _dbContext.Albums.AddRange(newAlbums);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Stored albums for {Artists} artists", albums.Count());
        
        var tracks = new List<TrackEF>();
        foreach (var album in newAlbums)
        {
            var albumTracks = BuildTracks(newTracks, album);
            tracks.AddRange(albumTracks);
        }
        _dbContext.Tracks.AddRange(tracks);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Stored {Tracks} tracks", tracks.Count());
        return new TrackStoreResult()
        {
            Message = "Successfully stored new tracks",
            ArtistsAdded = newArtists.Count,
            AlbumsAdded = newAlbums.Count,
            TracksAdded = tracks.Count,
        };
    }

    private IEnumerable<TrackEF> BuildTracks(List<TrackFile> trackFiles, AlbumEF album)
    {
        var albumTracks = trackFiles.Where(t => t.Metadata.Album == album.Title);
        var tracks = new List<TrackEF>();
        foreach (var trackFile in albumTracks)
        {
            var track = new TrackEF()
            {
                TrackNumber = trackFile.Metadata.TrackNumber,
                FilePath = trackFile.Location,
                ArtistId = album.ArtistId,
                AlbumId = album.Id,
                Title = trackFile.Metadata.Title,
                Genre = trackFile.Metadata.Genre,
                Duration = trackFile.Metadata.Duration,
            };
            tracks.Add(track);
        }
        return tracks;
    }
    private IEnumerable<AlbumEF> BuildAlbums(List<TrackFile> tracksFiles, ArtistEF artist)
    {
        var artistFiles = tracksFiles.Where(t => t.Metadata.Artist == artist.Name);
        var albumNames = artistFiles.Select(t => t.Metadata.Album).Where(a => !string.IsNullOrEmpty(a)).Distinct();
        var albums = new List<AlbumEF>();
        foreach (var albumName in albumNames)
        {
            var trackMetadata = artistFiles.FirstOrDefault(t => t.Metadata.Album == albumName);
            var album = new AlbumEF()
            {
                ArtistId = artist.Id,
                Title = albumName,
                Genre = trackMetadata?.Metadata.Genre,
                Year = trackMetadata?.Metadata.Year,
            };
             albums.Add(album);
        }
        return albums;
    }
    private IEnumerable<ArtistEF> BuildArtists(List<TrackFile> trackFiles)
    {
        var artistNames = trackFiles.Select(t => t.Metadata.Artist).Where(a => !string.IsNullOrEmpty(a)).Distinct();
        var artists = new List<ArtistEF>();
        foreach (var artistName in artistNames)
        {
            var artist = new ArtistEF()
            {
                Name = artistName,
            };
            artists.Add(artist);
        }
        return artists;
    }

    private async Task<List<AlbumEF>> GetAlbumsNotStored(List<AlbumEF> albums)
    {
        var storedAlbums = await _dbContext.Albums.Select(a => a.Title).ToListAsync();
        var newAlbums = albums.Where(a => !storedAlbums.Contains(a.Title));
        return newAlbums.ToList();
    }
    
    private async Task<List<ArtistEF>> GetArtistsNotStored(IEnumerable<ArtistEF> artists)
    {
        var storedArtists = await _dbContext.Artists.Select(a  => a.Name.ToLower()).ToListAsync();
        var newArtists = artists.Where(a => !storedArtists.Contains(a.Name.ToLower()));
        return newArtists.ToList();
    }
    
    private async Task<List<TrackFile>> GetTrackFilesNotStored(List<TrackFile> trackFiles)
    {
        var storedTracksFileNames = await _dbContext.Tracks.Select(t=>Path.GetFileName(t.FilePath)).ToListAsync();
        var newTracks = trackFiles.Where(t => !storedTracksFileNames.Contains(Path.GetFileName(t.Location)));
        return newTracks.ToList();  
    }
}