using Microsoft.AspNetCore.Mvc;
using MusicStreamerBackend.Data;
using MusicStreamerBackend.Services;

namespace MusicStreamerBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : Controller
{
    private readonly IExternalMetadataService _externalMetadataService;
    
    public TestController(IExternalMetadataService externalMetadataService, MusicStreamerDbContext dbContext)
    {
        _externalMetadataService = externalMetadataService;
    }

    [HttpGet("artistSearch")]
    public async Task<IActionResult> ArtistSearch([FromQuery] string artistName)
    {
        var result = await _externalMetadataService.FindArtistsMissingExternalMetadata(5);
        return Ok(result);   
    }

    [HttpGet("albumSearch")]
    public async Task<IActionResult> AlbumSearch([FromQuery] int artistId)
    {
        var result = await _externalMetadataService.FindAlbumsMissingReleaseGroups(artistId);
        return Ok(result);
    }

    [HttpGet("triggerAlbumSync")]
    public async Task<IActionResult> TriggerAlbumSync(IAlbumMetadataSyncService albumMetadataSyncService)
    {
        await albumMetadataSyncService.TriggerSync(CancellationToken.None);
        return Ok();
    }
    
    [HttpGet("coverArt")]
    public async Task<IActionResult> GetCoverArt(IFileService fileService)
    {
        fileService.CreateAlbumCoverVariants(150, 300);
        return Ok();
    }
}