using System.Text.Json.Serialization;

namespace MusicStreamerBackend.Models.MusicBrainz;

public class MusicBrainzArtist
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("score")]
    public int Score { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("disambiguation")]
    public string? Disambiguation { get; set; }
    
    public string MusicBrainzUrl => $"https://musicbrainz.org/artist/{Id}";
}