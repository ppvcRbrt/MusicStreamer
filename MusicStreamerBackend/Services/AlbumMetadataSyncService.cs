using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using MusicStreamerBackend.Data;
using MusicStreamerBackend.Data.EFModels.Music;

namespace MusicStreamerBackend.Services;

public interface IAlbumMetadataSyncService
{
    Task TriggerSync(CancellationToken ct = default);
    AlbumSyncStatus GetStatus();
}
public record AlbumSyncStatus(string State, int ArtistsProcessed, int AlbumsMatched, int CoversFetched, string? LastError);

public class AlbumMetadataSyncService : BackgroundService, IAlbumMetadataSyncService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlbumMetadataSyncService> _logger;
    private readonly Channel<bool> _triggerChannel = Channel.CreateBounded<bool>(1);

    private string _state = "Idle";
    private int _artistsProcessed;
    private int _albumsMatched;
    private int _coversfetched;
    private string? _lastError;

    public AlbumMetadataSyncService(IServiceScopeFactory scopeFactory, ILogger<AlbumMetadataSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var _ in _triggerChannel.Reader.ReadAllAsync(stoppingToken))
        {
            _state = "Running";
            _artistsProcessed = 0;
            _albumsMatched = 0;
            _coversfetched = 0;
            _lastError = null;
            
            _logger.LogInformation("Album metadata sync triggered, starting...");
            using var scope = _scopeFactory.CreateScope();
            var metadataService = scope.ServiceProvider.GetRequiredService<IExternalMetadataService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<MusicStreamerDbContext>();
            var dbStorageService = scope.ServiceProvider.GetRequiredService<IDbStorageService>();
            
            var artistsWithExternalMetadata = await dbContext.Artists
                .Include(a => a.ExtIds)
                .Where(a => a.ExtIds != null && a.ExtIds.Any())
                .ToListAsync(cancellationToken: stoppingToken);
            _artistsProcessed = artistsWithExternalMetadata.Count;
            _logger.LogInformation("Found {Count} artists with external metadata", _artistsProcessed);

            foreach (var artist in artistsWithExternalMetadata)
            {
                var albumMetedata = await metadataService.FindAlbumsMissingReleaseGroups(artist.Id);
                if (albumMetedata != null)
                {
                    _albumsMatched += albumMetedata.Count;
                    _logger.LogInformation("Found {Count} albums for artist '{ArtistName}'", albumMetedata.Count, artist.Name);
                    
                    int totalStored = await dbStorageService.StoreExternalAlbumReferences(albumMetedata);
                    _logger.LogInformation("Stored {Count} album references for artist '{ArtistName}'", totalStored, artist.Name);
                }
            }
        
            int total = await metadataService.StoreMissingCoverArt();
            
            _state = "Idle";
            _logger.LogInformation("Album metadata sync completed");
        }
    }
    
    public Task TriggerSync(CancellationToken ct = default)
    {
        if (!_triggerChannel.Writer.TryWrite(true))
        {
            throw new InvalidOperationException("Album metadata sync is already queued or running");
        }

        return Task.CompletedTask;
    }

    public AlbumSyncStatus GetStatus() =>
        new(_state, _artistsProcessed, _albumsMatched, _coversfetched, _lastError);
}