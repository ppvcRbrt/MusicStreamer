using Microsoft.AspNetCore.Mvc;
using MusicStreamerBackend.Data.EFModels.Music;
using MusicStreamerBackend.Models.Scanning;
using MusicStreamerBackend.Services;

namespace MusicStreamerBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : Controller
{
    private readonly IFileService _fileService;
    private readonly IMusicInfoService _musicInfoService;
    private readonly IDbStorageService _dbStorageService;
    
    private readonly string _rootMediaFolder;
    private readonly ILogger<FileController> _logger;
    
    public FileController(IFileService fileService, IMusicInfoService musicInfoService, IConfiguration configuration, ILogger<FileController> logger, IDbStorageService dbStorageService)
    {
        _logger = logger;
        _fileService = fileService;
        _musicInfoService = musicInfoService;
        _dbStorageService = dbStorageService;
        _rootMediaFolder = configuration["MediaFolder"];
        if (_rootMediaFolder == null)
        {
            _logger.LogError("Media folder not configured");
            throw new ArgumentNullException(nameof(_rootMediaFolder), "Required configuration 'MediaFolder' is not set");
        }
    }
    
    [HttpGet("loadLocalTracks")]
    public async Task<IActionResult> ScanMediaFolder()
    {
        try
        {
            var trackFiles = _fileService.ScanForTracks(_rootMediaFolder);
            var stored = await _dbStorageService.StoreTracks(trackFiles.ToList());
            return Ok(stored);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new TrackStoreResult() {Message = $"Error scanning media folder: {ex.Message}"});
        }
    }
    
    [HttpGet("artistImage")]
    public async Task<IActionResult> GetArtistImage(string artistName)
    {
        try
        {
            var imageUrl = await _musicInfoService.GetArtistInfo(artistName);
            return Ok(imageUrl);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting artist image");
        }
    }
}