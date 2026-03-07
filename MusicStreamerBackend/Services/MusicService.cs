using Microsoft.EntityFrameworkCore;
using MusicStreamerBackend.Data;
using MusicStreamerBackend.Data.EFModels.Music;
using MusicStreamerBackend.Models.DTOs;

namespace MusicStreamerBackend.Services;

public interface IMusicService
{
    FileStream? GetTrackFileStream(string filePath);
    string? GetContentType(string filePath);
    List<ArtistDto> GetArtists();
    List<AlbumDto> GetAlbums();
    List<AlbumDto> GetAlbums(int artistId);
    ArtistDto? GetArtistDetails(int artistId);
}
public class MusicService : IMusicService
{
    private readonly MusicStreamerDbContext _dbContext;
    public MusicService(MusicStreamerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<AlbumDto> GetAlbums(int artistId)
    {
        return _dbContext.Albums
            .Where(a => a.ArtistId == artistId)
            .Include(a => a.Artist)
            .Include(a => a.Tracks)
            .Select(a => a.ToDto()).ToList();
    }
    public List<AlbumDto> GetAlbums()
    {
        return _dbContext.Albums
            .Include(a => a.Artist)
            .Include(a => a.Tracks)
            .Select(a => a.ToDto()).ToList();
    }
    public List<ArtistDto> GetArtists()
    {
        return _dbContext.Artists
            .Select(a => a.ToDto())
            .ToList();
    }

    public ArtistDto? GetArtistDetails(int artistId)
    {
        return _dbContext.Artists
            .Find(artistId)?
            .ToDto();
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