using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using MusicStreamerBackend.Models.MusicBrainz;
using MusicStreamerBackend.Services;

namespace MusicStreamerBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ExternalMetadataController : Controller
{
    private readonly IExternalMetadataService _externalMetadataService;
    private readonly IDbStorageService _dbStorageService;
    
    public ExternalMetadataController(IExternalMetadataService externalMetadataService, IDbStorageService dbStorageService)
    {
        _externalMetadataService = externalMetadataService;
        _dbStorageService = dbStorageService;
    }
    
    [HttpGet("dbArtists")]
    [RequestTimeout("LongRunning")]
    public async Task<IActionResult> ArtistSearch([FromQuery] int? limit)
    {
        var results = await _externalMetadataService.FindArtistsMissingExternalMetadata(limit ?? 5);
        return Ok(results);
    }

    [HttpPost("disambiguateArtists")]
    public async Task<IActionResult> StoreArtistReferences([FromBody] Dictionary<int, MusicBrainzArtist?> artists)
    {
        int totalStored = await _dbStorageService.StoreExternalArtistReferences(artists);
        
        return Ok($"Stored {totalStored} artist references");
    }

    [HttpGet("triggerAlbumSync")]
    public async Task<IActionResult> TriggerAlbumSync(IAlbumMetadataSyncService albumMetadataSyncService)
    {
        await albumMetadataSyncService.TriggerSync(CancellationToken.None);
        return Ok();   
    }

    [HttpGet("sync/status")]
    public IActionResult GetSyncStatus(IAlbumMetadataSyncService albumMetadataSyncService, ITranscodingService transcodingService)
    {
        return Ok(new
        {
            metadata = albumMetadataSyncService.GetStatus(),
            transcoding = transcodingService.GetStatus()
        });
    }

}