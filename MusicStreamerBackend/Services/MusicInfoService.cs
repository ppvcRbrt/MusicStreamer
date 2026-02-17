namespace MusicStreamerBackend.Services;

/// <summary>
/// Should be mainly using the discogs music database API to get information about artists, albums and tracks.
/// This will be used to enrich the metadata of the tracks in the media folder, and to provide additional information to the client applications.
/// </summary>
public interface IMusicInfoService
{
    Task<string> GetArtistImageUrl(string artistName);
    string GetAlbumImageUrl(string artistName, string albumName);
}
public class MusicInfoService: IMusicInfoService
{
    private readonly HttpClient _httpClient;
    
    public MusicInfoService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Discogs");
    }
    
    public async Task<string> GetArtistImageUrl(string artistName)
    {
        var result= await _httpClient.GetAsync($"database/search?q={artistName}&type=artist");
        var resultTxt = await result.Content.ReadAsStringAsync();
        return "";
    }

    public string GetAlbumImageUrl(string artistName, string albumName)
    {
        throw new NotImplementedException();
    }
}