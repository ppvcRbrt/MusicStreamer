using System.ComponentModel.DataAnnotations;
using MusicStreamerBackend.Data.EFModels.Music;
using MusicStreamerBackend.Models.DTOs.User;

namespace MusicStreamerBackend.Data.EFModels.User;

public class PlaylistEF
{
    [Key]
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Title { get; set; }
    public int[] TrackIds { get; set; }
    
    public PlaylistDto ToDto()
    {
        return new PlaylistDto
        {
            Id = Id,
            Title = Title,
            TrackIds = TrackIds
        };
    }
}