namespace MusicStreamerBackend.Models.DTOs;

public class AlbumDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Image { get; set; }
    public ArtistDto Artist { get; set; }
    public List<TrackDto> Tracks { get; set; }
    public string Type { get; set; } = "album";
}