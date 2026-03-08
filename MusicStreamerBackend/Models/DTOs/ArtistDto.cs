namespace MusicStreamerBackend.Models.DTOs;

public class ArtistDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Image { get; set; }
    public string? ImageSmall => Image != null ? CreateImagePath(Image, 150) : null;
    public string? ImageLarge => Image != null ? CreateImagePath(Image, 600): null;

    
    private string? CreateImagePath(string? path, int size)
    {
        if (path == null) return null;
        return $"{Path.ChangeExtension(path, null)}_{size}.jpg";
    }

}