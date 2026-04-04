using System.ComponentModel.DataAnnotations;

namespace MusicStreamerBackend.Data.EFModels.User;

public class PlaylistEF
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PlaylistName { get; set; }
    public int[] TrackIds { get; set; }
}