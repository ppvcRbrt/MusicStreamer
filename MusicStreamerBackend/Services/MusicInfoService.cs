using MusicStreamerBackend.Models.Discogs;

namespace MusicStreamerBackend.Services;

/// <summary>
/// Should be mainly using the discogs music database API to get information about artists, albums and tracks.
/// This will be used to enrich the metadata of the tracks in the media folder, and to provide additional information to the client applications.
/// TODO: Feature a "This Application uses Discogs API but is not in any way affiliated with Discogs" disclaimer in the about section of the client applications, and the api docs.
/// </summary>
public interface IMusicInfoService
{
    string GetAlbumImageUrl(string artistName, string albumName);
    Task<DiscogsArtistInfo?> GetArtistInfo(string artistName);
}

public class MusicInfoService: IMusicInfoService
{
    private readonly HttpClient _discogsHttpClient;
    private readonly ILogger<MusicInfoService> _logger;
    
    public MusicInfoService(IHttpClientFactory httpClientFactory, ILogger<MusicInfoService> logger)
    {
        _logger = logger;
        _discogsHttpClient = httpClientFactory.CreateClient("Discogs");
    }
    
    public async Task<DiscogsArtistInfo?> GetArtistInfo(string artistName)
    {
        var discogsDbResult = await SearchDiscogsDb(artistName, DbSearchResultType.Artist);
        if (discogsDbResult == null)
        {
            throw new Exception($"No results found for artist '{artistName}' in Discogs database");
        }

        try
        {
            var result = await _discogsHttpClient.GetAsync(discogsDbResult.ResourceUrl);
            var artistInfo = await result.Content.ReadFromJsonAsync<DiscogsArtistInfo>();
            return artistInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
    private async Task<DiscogsDbSearchResult?> SearchDiscogsDb(string searchTerm, DbSearchResultType searchType)
    {
        var result= await _discogsHttpClient.GetAsync($"database/search?q={searchTerm}&type={searchType.ToString().ToLower()}");
        try
        {
            var dbSearchResult = await result.Content.ReadFromJsonAsync<DiscogsDbSearchResults>();
            if (dbSearchResult != null && dbSearchResult.Results.Any())
            {
                //Maybe add a checker to see if the result is a good match for the search term, as sometimes the search can return results that are not relevant
                return dbSearchResult.Results.First();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        return null;
    }
    
    public string GetAlbumImageUrl(string artistName, string albumName)
    {
        throw new NotImplementedException();
    }
}