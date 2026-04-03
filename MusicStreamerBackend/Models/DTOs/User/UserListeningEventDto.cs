using MusicStreamerBackend.Data.EFModels.User;
public class UserListeningEventDto
{
    public int TrackId { get; set; }
    public ListeningEventType EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public float PositionMs { get; set; }
    public float DurationMs { get; set; }
    public ContextType Context { get; set; }

    public ListeningEventEF ToEF()
    {
        return new ListeningEventEF
        {
            TrackId = TrackId,
            EventType = EventType,
            Timestamp = Timestamp,
            PositionMs = PositionMs,
            DurationMs = DurationMs,
            Context = Context
        };
    }
}
