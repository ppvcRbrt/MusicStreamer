using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using MusicStreamerBackend.Data;
using MusicStreamerBackend.Data.EFModels.Music;
using MusicStreamerBackend.Helpers;
using MusicStreamerBackend.Models.Discogs;
using MusicStreamerBackend.Models.Scanning;
using SkiaSharp;
using TagLib.Riff;
using File = System.IO.File;

namespace MusicStreamerBackend.Services;

public interface IFileService
{
    IEnumerable<TrackFile>? ScanForTracks(string rootFolderPath);
    bool StoreAlbumCover(int albumId, byte[] coverData, string coverExtension);
    List<int> GetStoredAlbumCoverIds();
    void CreateAlbumCoverVariants(params int[] sizes);
    Task<string> TranscodeToOpus(string inputPath, string outputPath, CancellationToken ct);
}

public class FileService: IFileService
{
    private readonly List<string> _fileTypes = [".mp3", ".wav", ".ogg", ".m4a", ".flac", ".aac"];
    private readonly ILogger<FileService> _logger;
    private readonly string _rootCoverArtFolder;

    public FileService(ILogger<FileService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _rootCoverArtFolder = configuration["CoverArtFolder"] ?? $"{configuration["MediaFolder"]}/CoverArt";
    }
    
    
    public async Task<string> TranscodeToOpus(string inputPath, string outputPath, CancellationToken ct)
    {
        var tempPath = $"{outputPath}.tmp";
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    ArgumentList =
                    {
                        "-i", inputPath,
                        "-c:a", "libopus",
                        "-b:a", "128k",
                        "-vn",
                        "-f", "ogg",
                        "-y",
                        tempPath
                    },
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var stderr = await process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            if (process.ExitCode != 0)
                throw new Exception($"ffmpeg exited with code {process.ExitCode}: {stderr}");

            File.Move(tempPath, outputPath);
            return outputPath;
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
    
    public List<int> GetStoredAlbumCoverIds()
    {
        List<int> coverArtIds = new List<int>();
        var coverArtFiles = Directory.GetFiles(_rootCoverArtFolder).Where(f => !Path.GetFileName(f).Contains('_'));
        foreach (var coverArtFile in coverArtFiles)
        {
            var albumId = Path.GetFileNameWithoutExtension(coverArtFile);
            coverArtIds.Add(int.Parse(albumId));
        }
        return coverArtIds;
    }

    public void CreateAlbumCoverVariants(params int[] sizes)
    {
        var coverFiles = Directory.GetFiles(_rootCoverArtFolder)    
            .Where(f => !Path.GetFileName(f).Contains('_'))
            .ToArray();

        foreach (var coverFile in coverFiles)
        {
            using var original = SKBitmap.Decode(coverFile);
            if (original == null)
            {
                _logger.LogWarning("Failed to decode image: {CoverFile}", coverFile);
                continue;
            }

            foreach (var size in sizes)
            {
                string fileName = $"{Path.GetFileNameWithoutExtension(coverFile)}_{size}.jpg";
                if (File.Exists(Path.Combine(_rootCoverArtFolder, fileName)))
                {
                    _logger.LogInformation("Skipping {Size}px variant for {CoverFile} - already exists", size, coverFile);
                    continue;
                }
                if (original.Width <= size && original.Height <= size)
                {
                    _logger.LogInformation("Skipping {Size}px variant for {CoverFile} - original is smaller", size, coverFile);
                    continue;
                }

                try
                {
                    using var resized = original.Resize(
                        new SKImageInfo(size, size), 
                        new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

                    if (resized == null)
                    {
                        _logger.LogWarning("Failed to resize {CoverFile} to {Size}px", coverFile, size);
                        continue;
                    }

                    using var image = SKImage.FromBitmap(resized);
                    using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);
                    var path = Path.Combine(_rootCoverArtFolder, fileName);
                    using var stream = File.OpenWrite(path);
                    data.SaveTo(stream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create {Size}px variant for {CoverFile}", size, coverFile);
                }
            }
        }
    }    
    public bool StoreAlbumCover(int albumId, byte[] coverData, string coverExtension)
    {
        var coverPath = Path.Combine(_rootCoverArtFolder, $"{albumId}{coverExtension}");
        if (coverData.Length == 0)
        {
            _logger.LogWarning("No cover data found for album: {AlbumName}", Path.GetFileName(coverPath));
            return false;
        }
        if (!Directory.Exists(Path.GetDirectoryName(coverPath)))
        {
            _logger.LogWarning("No root directory found for cover art. Creating one at: {RootDirectory}", Path.GetDirectoryName(coverPath));
            if (String.IsNullOrEmpty(Path.GetDirectoryName(coverPath)))
            {
                _logger.LogError("Invalid cover path: {CoverPath}.\nCover path should be absolute.", coverPath);
                return false;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(coverPath)!);
        }
        if (File.Exists(coverPath))
        {
            _logger.LogWarning("Cover art already exists for album: {AlbumName}", Path.GetFileName(coverPath));
            return false;
        }
        _logger.LogInformation("Storing cover art for album: {AlbumName}", Path.GetFileName(coverPath));
        File.WriteAllBytes(coverPath, coverData);
        return true;
    }
    
    public IEnumerable<TrackFile>? ScanForTracks(string rootFolderPath)
    {
        try
        {
            var files = Directory.GetFiles(rootFolderPath, "*.*", SearchOption.AllDirectories)
                .Where(file => _fileTypes.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .Where(f => !Path.GetFileName(f).StartsWith("._"));
            _logger.LogInformation("Found {Count} files found", files.Count());
            return files.Select(f => new TrackFile()
            {
                Name =  Path.GetFileNameWithoutExtension(f),
                Location = f,
                Extensions = Path.GetExtension(f).ToLowerInvariant(),
                Metadata =  GetTrackMetadata(f),
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error scanning for tracks: {Message}", ex.Message);
            throw;
        }
    }

    private TrackMetadata? GetTrackMetadata(string filePath)
    {
        try
        {
            var tagFile = TagLib.File.Create(filePath);
            _logger.LogInformation("Reading tag file: {TagFile}", tagFile.Tag.Title);
            return new TrackMetadata()
            {
                TrackNumber = tagFile.Tag.Track > 0 ? (int)tagFile.Tag.Track : 0,
                Title = tagFile.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath),
                Artist = tagFile.Tag.AlbumArtists.FirstOrDefault() ?? NormalizeArtistName(tagFile.Tag.FirstPerformer),
                Album = tagFile.Tag.Album,
                Year = tagFile.Tag.Year > 0 ? (int?)tagFile.Tag.Year : null,
                Genre = tagFile.Tag.JoinedGenres,
                Duration = tagFile.Properties.Duration,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Skipping file {FilePath}, reason: {Reason}", filePath, ex.Message);
            return null;
        }
    }
    
    private static string NormalizeArtistName(string artistName)
    {
        if (string.IsNullOrEmpty(artistName))
            return artistName;

        // Common featuring separators
        string[] featuringSeparators = { " ft. ", " ft ", " feat. ", " feat ", " featuring ", " ft.", " feat." };
    
        var normalized = artistName;
        foreach (var separator in featuringSeparators)
        {
            int index = normalized.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                normalized = normalized.Substring(0, index).Trim();
                break;
            }
        }
    
        return normalized;
    }
}