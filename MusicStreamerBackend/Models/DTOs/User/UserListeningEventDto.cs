using System.Text.Json.Serialization;
using MusicStreamerBackend.Data.EFModels.User;
public class UserListeningEventDto
{
    [JsonPropertyName( "trackId")]
    public int TrackId { get; set; }
    [JsonPropertyName("eventType")]
    public ListeningEventType EventType { get; set; }
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    [JsonPropertyName("positionMs")]
    public float PositionMs { get; set; }
    [JsonPropertyName("durationMs")]
    public float DurationMs { get; set; }
    [JsonPropertyName("context")]
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
