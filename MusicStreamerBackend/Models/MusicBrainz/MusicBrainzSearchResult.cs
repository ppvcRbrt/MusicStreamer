using System.Text.Json.Serialization;

namespace MusicStreamerBackend.Models.MusicBrainz;

public class MusicBrainzSearchResult
{
    [JsonPropertyName("created")]
    public DateTime Created { get; set; }
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("offset")]
    public int Offset { get; set; }
    [JsonPropertyName("artists")]
    public List<MusicBrainzArtist>? Artists { get; set; }
    [JsonPropertyName("release-groups")]
    public List<MusicBrainzReleaseGroup>? ReleaseGroups { get; set; }
}