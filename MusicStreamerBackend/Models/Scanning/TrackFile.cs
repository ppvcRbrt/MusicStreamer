namespace MusicStreamerBackend.Models.Scanning;

public class TrackFile
{
    public string Name { get; set; }
    public string Location { get; set; }
    public string Extensions { get; set; }
    public TrackMetadata Metadata { get; set; }
}