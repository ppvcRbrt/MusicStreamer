namespace MusicStreamerBackend.Models.Scanning;

public class TrackStoreResult
{
    public string Message { get; set; }
    public int TracksAdded { get; set; }
    public int AlbumsAdded { get; set; }
    public int ArtistsAdded { get; set; }
}