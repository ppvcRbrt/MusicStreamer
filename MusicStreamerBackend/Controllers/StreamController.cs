using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MusicStreamerBackend.Controllers;

[AllowAnonymous]
[ApiController]
[Route("[controller]")]
public class StreamController : Controller
{
    private readonly ILogger<StreamController> _logger;

    public StreamController(ILogger<StreamController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{filename}")]
    public IActionResult Stream(string filename, IConfiguration config)
    {
        var rootFolder = config.GetValue<string>("MediaLocation") ?? "~/Music";
        var file = GetFile(filename, rootFolder);
        if(file == null)
        {
            _logger.LogError("File not found: {Filename}", filename);
            return NotFound();
        }
        var contentType = GetContentType(filename);
        if(contentType == null)
        {
            _logger.LogError("Content type not found: {Filename}", filename);
            return BadRequest("Unsupported file type");
        }
        return File(file, contentType, filename, enableRangeProcessing: true);
    }


    private FileStream? GetFile(string filename, string rootFolder)
    {
        var filePath = Directory.GetFiles(rootFolder, filename, SearchOption.AllDirectories).FirstOrDefault();
        return filePath == null ? null : new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    private static string? GetContentType(string filename) => Path.GetExtension(filename).ToLowerInvariant() switch
    {
        ".mp3" => "audio/mpeg",
        ".wav" => "audio/wav",
        ".ogg" => "audio/ogg",
        ".m4a" => "audio/mp4",
        ".flac" => "audio/flac",
        ".aac" => "audio/aac",
        _ => null
    };
}