using Microsoft.AspNetCore.Mvc;
using MusicStreamerBackend.Models.DTOs.User;
using MusicStreamerBackend.Services;

namespace MusicStreamerBackend.Controllers;

[ApiController]
[Route( "[controller]" )]
public class UserController : Controller
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost("listeningEvent")]
    public async Task<IActionResult> ListeningEvent([FromBody] UserListeningEventDto eventDtoData)
    {
        var stored = await _userService.StoreUserListeningEvent(eventDtoData);
        if (!stored)
        {
            return BadRequest();
        }
        return Ok();
    }

    [HttpPost("createPlaylist")]
    public async Task<IActionResult> CreatePlaylist([FromBody] PlaylistDto playlist)
    {
        var storedPlaylist = await _userService.StoreUserPlaylist(playlist);
        return Ok(storedPlaylist);
    }
    
    [HttpGet("myPlaylists")] 
    public async Task<IActionResult> GetMyPlaylists()
    {
        var playlists = await _userService.GetUserPlaylists();
        return Ok(playlists);
    }
    
    [HttpPost("addToPlaylist")]
    public async Task<IActionResult> AddToPlaylist([FromBody] AddRemoveFromPlaylistRequestDto request)
    {
        var success = await _userService.AddTrackToPlaylist(request);
        if (!success)
        {
            return BadRequest();
        }
        return Ok();
    }
    
    [HttpPost("removeFromPlaylist")]
    public async Task<IActionResult> RemoveFromPlaylist([FromBody] AddRemoveFromPlaylistRequestDto request)
    {
        var success = await _userService.RemoveTrackFromPlaylist(request);
        if (!success)
        {
            return BadRequest();
        }
        return Ok();
    }
    
    [HttpPost("reorderPlaylist")]
    public async Task<IActionResult> ReorderPlaylist([FromBody] PlaylistDto playlist)
    {
        var success = await _userService.ReorderPlaylist(playlist);
        if (!success)
        {
            return BadRequest();
        }
        return Ok();
    }

    [HttpDelete("deletePlaylist/{playlistId}")]
    public async Task<IActionResult> DeletePlaylist(int playlistId)
    {
        var success = await _userService.DeletePlaylist(playlistId);
        if (!success)
        {
            return BadRequest();
        }
        return Ok();
    }
}