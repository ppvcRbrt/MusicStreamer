using TagLib;

namespace MusicStreamerBackend.Models.Scanning;

public class TrackMetadata
{
    public int TrackNumber { get; set; }
    public string Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Genre { get; set; }
    public int? Year { get; set; }
    public TimeSpan Duration { get; set; }
}