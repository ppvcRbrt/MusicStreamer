using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicStreamerBackend.Data.EFModels.Music;

public class TrackEF
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Genre { get; set; }
    public string? ImageUrl { get; set; }
    public int AlbumId { get; set; }
    public int ArtistId { get; set; }
    
    [ForeignKey(nameof(ArtistId))]
    public AlbumEF Album { get; set; }
    [ForeignKey(nameof(AlbumId))]
    public ArtistEF Artist { get; set; }
}