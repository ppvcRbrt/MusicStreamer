using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MusicStreamerBackend.Models.DTOs;

namespace MusicStreamerBackend.Data.EFModels.Music;

public class TrackEF
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string FilePath { get; set; }
    public string Format { get; set; }
    public int TrackNumber { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Genre { get; set; }
    public string? ImageUrl { get; set; }
    public int AlbumId { get; set; }
    public int ArtistId { get; set; }
    
    [ForeignKey(nameof(AlbumId))]
    public AlbumEF? Album { get; set; }
    [ForeignKey(nameof(ArtistId))]
    public ArtistEF? Artist { get; set; }
    public ICollection<TrackAltFormatsEF>? AltFormats { get; set; }

    public TrackDto ToDto(bool includeArtist = true, bool includeAlbum = true)
    {
        var altFormats = AltFormats?.Select(a => a.FilePath).ToList();
        List<string> filePaths = [FilePath];
        if (altFormats != null)
        {
            filePaths.AddRange(altFormats);
        }
        var dto = new TrackDto()
        {
            Id = Id,
            TrackNumber =  TrackNumber,
            Title = Title,
            FilePaths = filePaths,
            Duration = Duration ?? TimeSpan.Zero,
        };
        if (includeAlbum)
        {
            dto.Album = Album?.ToDto();
        }
        if (includeArtist)
        {
            dto.Artist = Artist?.ToDto();
        }
        return dto;
    }
}