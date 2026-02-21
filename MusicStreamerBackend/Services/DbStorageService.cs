using MusicStreamerBackend.Data;
using MusicStreamerBackend.Data.EFModels.Music;
using MusicStreamerBackend.Helpers;
using MusicStreamerBackend.Models.Discogs;
using MusicStreamerBackend.Models.Scanning;

namespace MusicStreamerBackend.Services;

public interface IDbStorageService
{
   Task<bool> StoreTracks(List<TrackFile> trackFiles);
}
public class DbStorageService : IDbStorageService
{
    private readonly ILogger<FileService> _logger;
    private readonly MusicStreamerDbContext _dbContext;
    public DbStorageService(ILogger<FileService> logger, MusicStreamerDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<bool> StoreTracks(List<TrackFile> trackFiles)
    {
        var artists = BuildArtists(trackFiles);
        _dbContext.Artists.AddRange(artists);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Stored tracks for {Artists} artists", artists.Count());
        
        var albums = new List<AlbumEF>();
        foreach (var artist in artists)
        {
            var artistAlbums = BuildAlbums(trackFiles, artist);
            albums.AddRange(artistAlbums);
        }
        _dbContext.Albums.AddRange(albums);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Stored albums for {Artists} artists", albums.Count());
        
        var tracks = new List<TrackEF>();
        foreach (var album in albums)        
        {
            var albumTracks = BuildTracks(trackFiles, album);
            tracks.AddRange(albumTracks);
        }   
        _dbContext.Tracks.AddRange(tracks);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Stored {Tracks} tracks", tracks.Count());
        
        return true;
    }
    
    private IEnumerable<TrackEF> BuildTracks(List<TrackFile> trackFiles, AlbumEF album)
    {
        var albumTracks = trackFiles.Where(t => t.Metadata.Album == album.Title);
        var tracks = new List<TrackEF>();
        foreach (var trackFile in albumTracks)
        {
            var track = new TrackEF()
            {
                FilePath = trackFile.Location,
                ArtistId = album.ArtistId,
                AlbumId = album.Id,
                Title = trackFile.Metadata.Title,
                Genre = trackFile.Metadata.Genre,
                Duration = trackFile.Metadata.Duration,
            };
            tracks.Add(track);
        }
        return tracks;
    }
    private IEnumerable<AlbumEF> BuildAlbums(List<TrackFile> tracksFiles, ArtistEF artist)
    {
        var artistFiles = tracksFiles.Where(t => t.Metadata.Artist == artist.Name);
        var albumNames = artistFiles.Select(t => t.Metadata.Album).Where(a => !string.IsNullOrEmpty(a)).Distinct();
        var albums = new List<AlbumEF>();
        foreach (var albumName in albumNames)
        {
            var trackMetadata = artistFiles.FirstOrDefault(t => t.Metadata.Album == albumName);
            var album = new AlbumEF()
            {
                ArtistId = artist.Id,
                Title = albumName,
                Genre = trackMetadata?.Metadata.Genre,
                Year = trackMetadata?.Metadata.Year,
            };
             albums.Add(album);
        }
        return albums;
    }
    private IEnumerable<ArtistEF> BuildArtists(List<TrackFile> trackFiles)
    {
        var artistNames = trackFiles.Select(t => t.Metadata.Artist).Where(a => !string.IsNullOrEmpty(a)).Distinct();
        var artists = new List<ArtistEF>();
        foreach (var artistName in artistNames)
        {
            var artist = new ArtistEF()
            {
                Name = artistName,
            };    
            artists.Add(artist);
        }
        return artists;
    }

}