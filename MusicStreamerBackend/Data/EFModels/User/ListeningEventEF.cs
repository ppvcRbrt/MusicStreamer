using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MusicStreamerBackend.Data.EFModels.Music;

namespace MusicStreamerBackend.Data.EFModels.User;

public enum ListeningEventType
{
    Play,
    Skip,
    Pause,
    Resume, 
    SeekForward, 
    SeekBackward,
    QueueAdd,
    QueueRemove,
    Complete
}

public enum ContextType
{
    Search,
    Queue,
    Album,
    Artist,
    Playlist,
    Recommendation,
    Player
}

public class ListeningEventEF
{
    [Key]
    public int EventId { get; set; }
    public int TrackId { get; set; }
    public int UserId { get; set; } = 1;
    public ListeningEventType EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public float PositionMs { get; set; }
    public float DurationMs { get; set; }
    public ContextType Context { get; set; }   
    
    [ForeignKey(nameof(TrackId))]
    public TrackEF Track { get; set; }
}