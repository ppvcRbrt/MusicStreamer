using System.ComponentModel.DataAnnotations;

namespace MusicStreamerBackend.Data.EFModels.Music;

public class ArtistEF
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? DiscogsDbId { get; set; }
    
    public ICollection<AlbumEF>? Albums { get; set; }
    public ICollection<TrackEF>? Tracks { get; set; }
}