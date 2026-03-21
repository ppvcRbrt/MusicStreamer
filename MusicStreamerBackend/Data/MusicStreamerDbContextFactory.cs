using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MusicStreamerBackend.Data;

public class MusicStreamerDbContextFactory : IDesignTimeDbContextFactory<MusicStreamerDbContext>
{
    public MusicStreamerDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<MusicStreamerDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("MusicStreamerDb"));

        return new MusicStreamerDbContext(optionsBuilder.Options);
    }
}
