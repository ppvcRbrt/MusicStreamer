using Microsoft.AspNetCore.Mvc;
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
}