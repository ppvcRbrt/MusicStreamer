using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using MusicStreamerBackend.Data;
using MusicStreamerBackend.Data.EFModels.Music;
using MusicStreamerBackend.Helpers;

namespace MusicStreamerBackend.Services;

public interface ITranscodingService
{
    Task TriggerSync(CancellationToken ct = default);
    TranscodingStatus GetStatus();
}

public record TranscodingStatus(string State, int TracksProcessed, int TracksSkipped, int TracksFailed, string? LastError);

public class TranscodingService : BackgroundService, ITranscodingService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TranscodingService> _logger;
    private readonly Channel<bool> _triggerChannel = Channel.CreateBounded<bool>(1);

    private string _state = "Idle";
    private int _tracksProcessed;
    private int _tracksSkipped;
    private int _tracksFailed;
    private string? _lastError;

    public TranscodingService(IServiceScopeFactory scopeFactory, ILogger<TranscodingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var _ in _triggerChannel.Reader.ReadAllAsync(stoppingToken))
        {
            _state = "Running";
            _tracksProcessed = 0;
            _tracksSkipped = 0;
            _tracksFailed = 0;
            _lastError = null;

            _logger.LogInformation("Transcoding sync triggered, starting...");

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MusicStreamerDbContext>();
            var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();
            var dbStorageService = scope.ServiceProvider.GetRequiredService<IDbStorageService>();
            
            var tracks = await dbContext.Tracks
                .Where(t => t.FilePath.EndsWith(".flac"))
                .ToListAsync(stoppingToken);

            _logger.LogInformation("Found {Count} FLAC tracks to check", tracks.Count);

            foreach (var track in tracks)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                var opusPath = Path.ChangeExtension(track.FilePath, ".opus");
                var altFormat = new TrackAltFormatsEF()
                {
                    TrackId = track.Id,
                    Format = StringHelpers.ExtensionToFormat(Path.GetExtension(opusPath)),
                    FilePath = opusPath
                };
                if (File.Exists(opusPath))
                {
                    await dbStorageService.AddNewAltTrackFormat(altFormat); 
                    _tracksSkipped++;
                    continue;
                }

                try
                {
                    await fileService.TranscodeToOpus(track.FilePath, opusPath, stoppingToken);
                    await dbStorageService.AddNewAltTrackFormat(altFormat);
                    
                    _tracksProcessed++;
                    _logger.LogDebug("Transcoded: {Path}", track.FilePath);
                }
                catch (Exception ex)
                {
                    _tracksFailed++;
                    _lastError = $"{track.FilePath}: {ex.Message}";
                    _logger.LogError(ex, "Failed to transcode {Path}", track.FilePath);
                }
            }

            _state = "Idle";
            _logger.LogInformation(
                "Transcoding sync completed. Processed: {Processed}, Skipped: {Skipped}, Failed: {Failed}",
                _tracksProcessed, _tracksSkipped, _tracksFailed);
        }
    }

    public Task TriggerSync(CancellationToken ct = default)
    {
        if (!_triggerChannel.Writer.TryWrite(true))
            throw new InvalidOperationException("Transcoding sync is already queued or running");

        return Task.CompletedTask;
    }

    public TranscodingStatus GetStatus() =>
        new(_state, _tracksProcessed, _tracksSkipped, _tracksFailed, _lastError);
}