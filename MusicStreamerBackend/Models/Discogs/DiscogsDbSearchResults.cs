using System.Text.Json.Serialization;

namespace MusicStreamerBackend.Models.Discogs;

public enum DbSearchResultType
{
    Artist = 1,
    Album = 2,
    Release = 3
}

public class DiscogsDbSearchResult
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("master_id")]
    public int? MasterId { get; set; }
    [JsonPropertyName("master_url")]
    public string? MasterUrl { get; set; }
    [JsonPropertyName("uri")]
    public string Uri { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("thumb")]
    public string? Thumb { get; set; }
    [JsonPropertyName("cover_image")]
    public string? CoverImage { get; set; }
    [JsonPropertyName("resource_url")]
    public string? ResourceUrl { get; set; }
    
    public DbSearchResultType ResultType => Type switch
    {
        "artist" => DbSearchResultType.Artist,
        "album" => DbSearchResultType.Album,
        "release" => DbSearchResultType.Release,
        _ => throw new ArgumentOutOfRangeException(nameof(Type), $"Unknown result type: {Type}")
    };
}

public class DiscogsDbSearchResults
{
    public DiscogsPagination Pagination { get; set; }
    public IEnumerable<DiscogsDbSearchResult> Results { get; set; }
}
public class DiscogsPagination
{
    public int Page { get; set; }
    public int Pages { get; set; }
    public int PerPage { get; set; }
    public int Items { get; set; }
}