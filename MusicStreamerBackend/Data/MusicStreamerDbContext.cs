using Microsoft.EntityFrameworkCore;
using MusicStreamerBackend.Data.EFModels.Music;

namespace MusicStreamerBackend.Data;

public class MusicStreamerDbContext : DbContext
{
    public MusicStreamerDbContext(DbContextOptions options) : base(options) { }
    
    public DbSet<ArtistEF> Artists { get; set; }
    public DbSet<AlbumEF> Albums { get; set; }
    public DbSet<TrackEF> Tracks { get; set; }
    public DbSet<ExtArtistEF> ExtArtists { get; set; }
    public DbSet<ExtAlbumEF> ExtAlbums { get; set; }
    public DbSet<TrackAltFormatsEF> TrackAltFormats { get; set; }
}