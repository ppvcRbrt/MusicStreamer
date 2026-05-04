using Microsoft.EntityFrameworkCore;
using MusicStreamerBackend.Data;
using MusicStreamerBackend.Models.DTOs.User;

namespace MusicStreamerBackend.Services;

public interface IUserService
{
    Task<bool> StoreUserListeningEvent(UserListeningEventDto eventDtoData);
    Task<PlaylistDto?> StoreUserPlaylist(PlaylistDto playlistDto);
    Task<bool> ReorderPlaylist(PlaylistDto playlistDto);
    Task<List<PlaylistDto>> GetUserPlaylists();
    Task<bool> AddTrackToPlaylist(AddRemoveFromPlaylistRequestDto request);
    Task<bool> RemoveTrackFromPlaylist(AddRemoveFromPlaylistRequestDto request);
    Task<bool> DeletePlaylist(int playlistId);
}

public class UserService : IUserService
{
    private readonly MusicStreamerDbContext _dbContext;

    public UserService(MusicStreamerDbContext dbContext)
    {
        _dbContext = dbContext; 
    }
    
    public async Task<bool> StoreUserListeningEvent(UserListeningEventDto eventDtoData)
    {
        var trackExists = await _dbContext.Tracks.AnyAsync(t => t.Id == eventDtoData.TrackId);
        if (!trackExists)
        {
            return false;
        }
        _dbContext.ListeningEvents.Add(eventDtoData.ToEF());
        return (await _dbContext.SaveChangesAsync()) == 1;
    }

    public async Task<PlaylistDto?> StoreUserPlaylist(PlaylistDto playlistDto)
    {
        var playlistExists = await _dbContext.UserPlaylists.AnyAsync(p => p.Title == playlistDto.Title);
        if (playlistExists)
        {
            return null;
        }
        var efPlaylist = playlistDto.ToEF();
        _dbContext.UserPlaylists.Add(efPlaylist);
        if ((await _dbContext.SaveChangesAsync()) == 1)
        {
            return await GetPlaylist(efPlaylist.Id);            
        }
        return null;
    }
    
    public async Task<bool> DeletePlaylist(int playlistId)
    {
        var playlist = await _dbContext.UserPlaylists.FindAsync(playlistId);
        if (playlist != null)
        {
            _dbContext.UserPlaylists.Remove(playlist);
            return (await _dbContext.SaveChangesAsync()) == 1;
        }
        return false;
    }
    
    public async Task<bool> ReorderPlaylist(PlaylistDto playlistDto)
    {
        var playlist = await _dbContext.UserPlaylists.FindAsync(playlistDto.Id);
        if (playlist != null)
        {
            playlist.TrackIds = playlistDto.TrackIds;
            return (await _dbContext.SaveChangesAsync()) == 1;
        }
        return false;
    }
    
    public async Task<List<PlaylistDto>> GetUserPlaylists()
    {
        List<PlaylistDto> playlists = await _dbContext.UserPlaylists.Select(p => p.ToDto()).ToListAsync();
        foreach (var playlist in playlists)
        {
            var tracks = await _dbContext.Tracks
                .Where(t => playlist.TrackIds.Contains(t.Id))
                .Include(t => t.Album)
                .Include(t => t.Artist)
                .Select(t => t.ToDto())
                .ToListAsync();
            playlist.Tracks = tracks;
        }
        return playlists;
    }
    public async Task<PlaylistDto> GetPlaylist(int playlistId)
    {
        var playlist = await _dbContext.UserPlaylists.FindAsync(playlistId);
        if (playlist != null)
        {
            var playlistDto = playlist.ToDto();
            var tracks = await _dbContext.Tracks
                .Where(t => playlistDto.TrackIds.Contains(t.Id))
                .Select(t => t.ToDto())
                .ToListAsync();
            playlistDto.Tracks = tracks;
            return playlistDto;
        }
        return new PlaylistDto();
    }

    public async Task<bool> AddTrackToPlaylist(AddRemoveFromPlaylistRequestDto request)
    {
        var playlist = await _dbContext.UserPlaylists.FindAsync(request.PlaylistId);
        var track = await _dbContext.Tracks.FindAsync(request.TrackId);
        
        if (playlist != null && track != null)
        {
            var trackIds = playlist.TrackIds.ToList();
            if (!trackIds.Contains(request.TrackId))
            {
                trackIds.Add(request.TrackId);
                playlist.TrackIds = trackIds.ToArray();
                return (await _dbContext.SaveChangesAsync()) == 1;
            }
        }
        return false;
    }
    
    public async Task<bool> RemoveTrackFromPlaylist(AddRemoveFromPlaylistRequestDto request)
    {
        var playlist = await _dbContext.UserPlaylists.FindAsync(request.PlaylistId);
        var track = await _dbContext.Tracks.FindAsync(request.TrackId);
        
        if (playlist != null && track != null)
        {
            var trackIds = playlist.TrackIds.ToList();
            if (trackIds.Contains(request.TrackId))
            {
                trackIds.Remove(request.TrackId);
                playlist.TrackIds = trackIds.ToArray();
                return (await _dbContext.SaveChangesAsync()) == 1;
            }
        }
        return false;
    }
}