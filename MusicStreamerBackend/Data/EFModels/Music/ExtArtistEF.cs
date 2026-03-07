using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicStreamerBackend.Data.EFModels.Music;

public class ExtArtistEF
{
    [Key]
    public string ExtId { get; set; }
    public int ArtistId { get; set; }
    public string ServiceName { get; set; }
    
    [ForeignKey(nameof(ArtistId))]
    public ArtistEF Artist { get; set; }
}