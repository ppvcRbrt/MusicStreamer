namespace MusicStreamerBackend.Models.DTOs;

public class TrackDto
{
    public int Id { get; set; }
    public int TrackNumber { get; set; }
    public string Title { get; set; }
    public TimeSpan Duration { get; set; }
    public string FilePath { get; set; }
    public ArtistDto? Artist { get; set; }
    public AlbumDto? Album { get; set; }
}