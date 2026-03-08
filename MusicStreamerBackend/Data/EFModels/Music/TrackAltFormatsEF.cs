using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicStreamerBackend.Data.EFModels.Music;

public class TrackAltFormatsEF
{
    [Key]
    public int Id { get; set; }
    public int TrackId { get; set; }
    public string Format { get; set; }
    public string FilePath { get; set; }
    
    [ForeignKey(nameof(TrackId))]
    public TrackEF Track { get; set; }
}