using MusicStreamerBackend.Data.EFModels.Music;
using MusicStreamerBackend.Models.Scanning;

namespace MusicStreamerBackend.Services;

public interface IFileService
{
    IEnumerable<TrackFile>? ScanForTracks(string rootFolderPath);
    (IEnumerable<TrackEF> tracks, IEnumerable<AlbumEF> albums, IEnumerable<ArtistEF> artists) TracksToDbModels(IEnumerable<TrackFile> trackFiles);
}
public class FileService: IFileService
{
    private readonly List<string> _fileTypes = new List<string> { ".mp3", ".wav", ".ogg", ".m4a", ".flac", ".aac" };
    private readonly ILogger<FileService> _logger;
    
    public FileService(ILogger<FileService> logger)
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
                Metadata =  GetTrackMetadata(f)
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
            Artist = tagFile.Tag.FirstPerformer,
            Album = tagFile.Tag.Album,
            Duration = tagFile.Properties.Duration
        };
    }
    
    public (IEnumerable<TrackEF> tracks, IEnumerable<AlbumEF> albums, IEnumerable<ArtistEF> artists) TracksToDbModels(IEnumerable<TrackFile> trackFiles)
    {
        var tracks = new List<TrackEF>();
        var albums = new List<AlbumEF>();
        var artists = new List<ArtistEF>();

        foreach (var trackFile in trackFiles)
        {
            string? artistName = trackFile.Metadata.Artist;
            if(!string.IsNullOrEmpty(artistName) && !artists.Any(a => a.Name == artistName))
            {
                artists.Add(new ArtistEF()
                {
                    Name = artistName,
                });
            }
        }
        return (tracks, albums, artists);
    }
}