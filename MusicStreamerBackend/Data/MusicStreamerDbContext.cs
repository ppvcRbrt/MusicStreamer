using Microsoft.EntityFrameworkCore;

namespace MusicStreamerBackend.Data;

public class MusicStreamerDbContext : DbContext
{
    public MusicStreamerDbContext(DbContextOptions options) : base(options) { }
}