using System.Text.Json.Serialization;

namespace MusicStreamerBackend.Models.Discogs;

public class DiscogsArtistReleases
{
    public DiscogsPagination Pagination { get; set; }
    public IEnumerable<DiscogsReleaseInfo> Releases { get; set; }
}

public class DiscogsReleaseInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("main_release")]
    public int MainReleaseId { get; set; }
    [JsonPropertyName("artist")]
    public string Artist { get; set; }
    [JsonPropertyName("resource_url")]
    public string ResourceUrl { get; set; }
    [JsonPropertyName(("role"))]
    public string Role { get; set; }
    [JsonPropertyName("thumb")]
    public string Thumb { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("year")]
    public int Year { get; set; }
}