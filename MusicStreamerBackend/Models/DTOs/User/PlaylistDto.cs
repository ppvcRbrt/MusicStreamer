using MusicStreamerBackend.Data.EFModels.User;

namespace MusicStreamerBackend.Models.DTOs.User;

public class PlaylistDto
{
    public int? Id { get; set; }
    public string Title { get; set; }
    public int[] TrackIds { get; set; }
    public List<TrackDto>? Tracks { get; set; } = new List<TrackDto>();
    
    public PlaylistEF ToEF()
    {
        return new PlaylistEF
        {
            Title = Title,
            TrackIds = TrackIds
        };
    }
}