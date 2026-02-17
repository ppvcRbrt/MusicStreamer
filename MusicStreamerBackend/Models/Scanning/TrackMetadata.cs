namespace MusicStreamerBackend.Models.Scanning;

public class TrackMetadata
{
    public string Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public TimeSpan Duration { get; set; }
}