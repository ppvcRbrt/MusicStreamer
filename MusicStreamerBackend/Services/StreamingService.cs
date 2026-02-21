using MusicStreamerBackend.Data;
using MusicStreamerBackend.Data.EFModels.Music;

namespace MusicStreamerBackend.Services;

public interface IStreamingService
{
    FileStream? GetTrackFileStream(string filePath);
    string? GetContentType(string filePath);
}
public class StreamingService : IStreamingService
{
    private readonly MusicStreamerDbContext _dbContext;
    public StreamingService(MusicStreamerDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public FileStream? GetTrackFileStream(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }
        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public string? GetContentType(string filePath)
    {
        switch (filePath.ToLowerInvariant().Substring(filePath.LastIndexOf('.')))
        {
            case ".mp3":
                return "audio/mpeg";
            case ".wav":
                return "audio/wav";
            case ".ogg":
                return "audio/ogg";
            case ".m4a":
                return "audio/mp4";
            case ".flac":    
                return "audio/flac";
            case ".aac":
                return "audio/aac";
            default:
                return null;
        }
    }
    

}