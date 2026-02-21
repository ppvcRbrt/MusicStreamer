using MusicStreamerBackend.Data.EFModels.Music;
using MusicStreamerBackend.Helpers;
using MusicStreamerBackend.Models.Discogs;
using MusicStreamerBackend.Models.Scanning;

namespace MusicStreamerBackend.Services;

public interface IFileService
{
    IEnumerable<TrackFile>? ScanForTracks(string rootFolderPath);
}
public class FileService: IFileService
{
    private readonly List<string> _fileTypes = new List<string> { ".mp3", ".wav", ".ogg", ".m4a", ".flac", ".aac" };
    private readonly ILogger<FileService> _logger;
    
    public FileService(ILogger<FileService> logger, IMusicInfoService musicInfoService)
    {
        _logger = logger;
    }
    
    public IEnumerable<TrackFile>? ScanForTracks(string rootFolderPath)
    {
        try
        {
            var files = Directory.GetFiles(rootFolderPath, "*.*", SearchOption.AllDirectories)
                .Where(file => _fileTypes.Contains(Path.GetExtension(file).ToLowerInvariant()));

            return files.Select(f => new TrackFile()
            {
                Name =  Path.GetFileNameWithoutExtension(f),
                Location = f,
                Extensions = Path.GetExtension(f).ToLowerInvariant(),
                Metadata =  GetTrackMetadata(f),
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error scanning for tracks: {Message}", ex.Message);
            throw;
        }
    }

    private TrackMetadata GetTrackMetadata(string filePath)
    {
        var tagFile = TagLib.File.Create(filePath);
        return new TrackMetadata()
        {
            Title = tagFile.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath),
            Artist = tagFile.Tag.AlbumArtists.FirstOrDefault() ?? NormalizeArtistName(tagFile.Tag.FirstPerformer),
            Album = tagFile.Tag.Album,
            Year = tagFile.Tag.Year > 0 ? (int?)tagFile.Tag.Year : null,
            Genre = tagFile.Tag.JoinedGenres,
            Duration = tagFile.Properties.Duration,
        };
    }
    
    private static string NormalizeArtistName(string artistName)
    {
        if (string.IsNullOrEmpty(artistName))
            return artistName;

        // Common featuring separators
        string[] featuringSeparators = { " ft. ", " ft ", " feat. ", " feat ", " featuring ", " ft.", " feat." };
    
        var normalized = artistName;
        foreach (var separator in featuringSeparators)
        {
            int index = normalized.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                normalized = normalized.Substring(0, index).Trim();
                break;
            }
        }
    
        return normalized;
    }
}