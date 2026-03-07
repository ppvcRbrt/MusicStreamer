using System.Text.Json.Serialization;

namespace MusicStreamerBackend.Models.MusicBrainz;

public class MusicBrainzReleaseGroup
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("score")]
    public int Score { get; set; }
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("first-release-date")]
    public string ReleaseYear { get; set; }
}