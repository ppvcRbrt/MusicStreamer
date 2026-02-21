using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicStreamerBackend.Data.EFModels.Music;

public class AlbumEF
{
    [Key]
    public int Id { get; set; }
    public int ArtistId { get; set; }
    [Required]
    public string Title { get; set; }
    public string? Genre { get; set; }
    public string? ImageUrl { get; set; }
    public int? Year { get; set; }
    public int? DiscogsDbId { get; set; }
    
    [ForeignKey(nameof(ArtistId))]
    public ArtistEF Artist { get; set; }
    public ICollection<TrackEF>? Tracks { get; set; }
}