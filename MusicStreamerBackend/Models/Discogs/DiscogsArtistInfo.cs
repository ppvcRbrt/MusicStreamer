using System.Text.Json.Serialization;

namespace MusicStreamerBackend.Models.Discogs;

public class DiscogsArtistInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("resource_url")]
    public string ResourceUrl { get; set; }
    [JsonPropertyName("uri")]
    public string Uri { get; set; }
    [JsonPropertyName("releases_url")]
    public string ReleasesUrl { get; set; }
    [JsonPropertyName("images")]
    public List<DiscogsArtistImage> Images { get; set; }
    [JsonPropertyName("profile")]
    public string Profile { get; set; }
    [JsonPropertyName("urls")]
    public List<string> Urls { get; set; }
    [JsonPropertyName("data_quality")]
    public string DataQuality { get; set; }
}

public class DiscogsArtistImage
{
    public string Type { get; set; }
    public string Uri { get; set; }
    public string ResourceUrl { get; set; }
    public string Uri150 { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}