using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MusicStreamerBackend.Data;

public class AppDbContextFactory
{
    public class MusicStreamerDbContextFactory : IDesignTimeDbContextFactory<MusicStreamerDbContext>
    {
        public MusicStreamerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MusicStreamerDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost:5442; Database=musicStreamerDb; Username=admin; Password=adminpassword");

            return new MusicStreamerDbContext(optionsBuilder.Options);
        }
    }
}