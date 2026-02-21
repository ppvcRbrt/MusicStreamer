using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStreamerBackend.Services;

namespace MusicStreamerBackend.Controllers;

[AllowAnonymous]
[ApiController]
[Route("[controller]")]
public class MusicController : Controller
{
    private readonly ILogger<MusicController> _logger;
    private readonly IStreamingService _streamService;
    public MusicController(ILogger<MusicController> logger, IStreamingService streamService)
    {
        _logger = logger;
        _streamService = streamService;
    }

    [HttpGet("stream")]
    public IActionResult Stream([FromQuery] string filePath)
    {
        var file = _streamService.GetTrackFileStream(filePath);
        if(file == null)
        {
            _logger.LogError("Track not found on the filesystem: {TrackName}", filePath);
            return NotFound();
        }
        var contentType = _streamService.GetContentType(filePath);
        if(contentType == null)
        {
            _logger.LogError("Unsupported file type for track: {TrackName}", filePath);
            return BadRequest("Unsupported file type");
        }
        return File(file, contentType, Path.GetFileName(filePath), enableRangeProcessing: true);
    }
    
    [HttpGet("artists")]
    public IActionResult GetArtists()
    {
        
        return Ok();
    }
}