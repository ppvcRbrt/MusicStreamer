namespace MusicStreamerBackend.Models.DTOs.User;

public class AddRemoveFromPlaylistRequestDto
{
    public int PlaylistId { get; set; }
    public int TrackId { get; set; }
}