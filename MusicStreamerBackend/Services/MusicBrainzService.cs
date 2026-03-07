using System.Net;
using MusicStreamerBackend.Helpers;
using MusicStreamerBackend.Models.MusicBrainz;

namespace MusicStreamerBackend.Services;

public interface IMusicBrainzService
{
    Task<MusicBrainzSearchResult?> SearchArtists(string query, int maxResults = 5);
    Task<MusicBrainzSearchResult?> GetArtistReleaseGroups(string artistId, int maxResults = 50);
    Task<(byte[] ImageBytes, string Extension)?> GetReleaseGroupCoverArt(string releaseGroupId);
}

public class MusicBrainzService : IMusicBrainzService
{
    private readonly HttpClient _musicBrainzHttpClient;
    private readonly HttpClient _coverArtArchiveHttpClient;
    private readonly ILogger<MusicBrainzService> _logger;

    public MusicBrainzService(IHttpClientFactory httpClientFactory, ILogger<MusicBrainzService> logger)
    {
        _logger = logger;
        _musicBrainzHttpClient = httpClientFactory.CreateClient("MusicBrainz");
        _coverArtArchiveHttpClient = httpClientFactory.CreateClient("CoverArtArchive");
    }

    public async Task<MusicBrainzSearchResult?> SearchArtists(string query, int maxResults = 5)
    {
        HttpResponseMessage response;
        try
        {
            response = await MusicBrainzApiHelper.SendWithRateLimitHandlingAsync(
                _musicBrainzHttpClient,
                new HttpRequestMessage(HttpMethod.Get, 
                    $"artist/?query=artist:{Uri.EscapeDataString(query)}&limit={maxResults}&fmt=json"),
                ensureSuccess: true
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("MusicBrainz artist search request failed: {Message}", ex.Message);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var searchResult = System.Text.Json.JsonSerializer.Deserialize<MusicBrainzSearchResult>(content);

        if (searchResult == null)
            _logger.LogWarning("MusicBrainz search returned null for query: {Query}", query);

        return searchResult;
    }

    public async Task<MusicBrainzSearchResult?> GetArtistReleaseGroups(string artistId, int maxResults = 50)
    {
        var requestUri = $"release-group?artist={artistId}&limit={maxResults}&offset=0&fmt=json";

        HttpResponseMessage response;
        try
        {
            response = await MusicBrainzApiHelper.SendWithRateLimitHandlingAsync(
                _musicBrainzHttpClient,
                new HttpRequestMessage(HttpMethod.Get, requestUri),
                ensureSuccess: true
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("MusicBrainz release group request failed: {Message}", ex.Message);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var searchResult = System.Text.Json.JsonSerializer.Deserialize<MusicBrainzSearchResult>(content);

        if (searchResult == null)
            _logger.LogWarning("MusicBrainz release groups returned null for artist: {ArtistId}", artistId);

        return searchResult;
    }

    public async Task<(byte[] ImageBytes, string Extension)?> GetReleaseGroupCoverArt(string releaseGroupId)
    {
        HttpResponseMessage response;
        try
        {
            response = await MusicBrainzApiHelper.SendWithRateLimitHandlingAsync(
                _coverArtArchiveHttpClient,
                new HttpRequestMessage(HttpMethod.Get, $"release-group/{releaseGroupId}/front"),
                secondsBetweenRequests: 5
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Cover art request failed for {Id}: {Message}", releaseGroupId, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Cover art request failed for {Id} after retries: {Message}", releaseGroupId, ex.Message);
            return null;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug("No cover art found for release group {Id}", releaseGroupId);
            return null;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning("Rate limited fetching cover art for {Id}, backing off", releaseGroupId);
            await Task.Delay(TimeSpan.FromMinutes(5));
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Cover art request failed for {Id}: {Status}", releaseGroupId, response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsByteArrayAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
        var extension = contentType switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
        return (content, extension);
    }
}