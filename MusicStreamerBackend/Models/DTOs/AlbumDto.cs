namespace MusicStreamerBackend.Models.DTOs;

public class AlbumDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Image { get; set; }
    public string? ImageSmall => Image != null ? CreateImagePath(Image, 150) : null;
    public string? ImageLarge => Image != null ? CreateImagePath(Image, 300): null;
    public ArtistDto Artist { get; set; }
    public List<TrackDto> Tracks { get; set; }
    public string Type { get; set; } = "album";
    
    private string? CreateImagePath(string? path, int size)
    {
        if (path == null) return null;
        return $"{Path.ChangeExtension(path, null)}_{size}.jpg";
    }
}