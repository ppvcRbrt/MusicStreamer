using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MusicStreamerBackend.Models.DTOs;

namespace MusicStreamerBackend.Data.EFModels.Music;

public class AlbumEF
{
    [Key]
    public int Id { get; set; }
    public int ArtistId { get; set; }
    [Required]
    public string Title { get; set; }
    public string? Genre { get; set; }
    public string? ImageUrl { get; set; }
    public int? Year { get; set; }
    public int? DiscogsDbId { get; set; }
    
    [ForeignKey(nameof(ArtistId))]
    public ArtistEF Artist { get; set; }
    public ICollection<TrackEF>? Tracks { get; set; }

    public AlbumDto ToDto(bool includeTrackAlbums = false, bool includeTrackArtists = false)
    {
        return new AlbumDto()
        {
            Id = Id,
            Title = Title,
            Image = ImageUrl,
            Artist = Artist.ToDto(),
            Tracks = Tracks?.Select(t => t.ToDto(includeTrackAlbums, includeTrackArtists)).ToList() ?? new List<TrackDto>()
        };
    }
}