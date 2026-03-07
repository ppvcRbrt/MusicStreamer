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
    private readonly IMusicService _musicService;
    public MusicController(ILogger<MusicController> logger, IMusicService musicService)
    {
        _logger = logger;
        _musicService = musicService;
    }

    [HttpGet("stream")]
    public IActionResult Stream([FromQuery] string filePath)
    {
        var file = _musicService.GetTrackFileStream(filePath);
        if(file == null)
        {
            _logger.LogError("Track not found on the filesystem: {TrackName}", filePath);
            return NotFound();
        }
        var contentType = _musicService.GetContentType(filePath);
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
        return Ok(_musicService.GetArtists());
    }

    [HttpGet("albums")]
    public IActionResult GetAlbums()
    {
        return Ok(_musicService.GetAlbums());
    }

    [HttpGet("artist/{artistId}")]
    public IActionResult GetArtistDetails(int artistId)
    {
        return Ok(_musicService.GetArtistDetails(artistId));
    }
    
    [HttpGet("artists/{artistId}/albums")]
    public IActionResult GetAlbums(int artistId)
    {
        return Ok(_musicService.GetAlbums(artistId));
    }
}