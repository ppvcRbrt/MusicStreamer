using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicStreamerBackend.Data.EFModels.Music;

public class ExtAlbumEF
{
    [Key]
    public string ExtId { get; set; }
    public int AlbumId { get; set; }
    public string ServiceName { get; set; }
    
    [ForeignKey(nameof(AlbumId))]
     public AlbumEF? Album { get; set; }
}