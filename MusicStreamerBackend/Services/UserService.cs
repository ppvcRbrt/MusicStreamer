using Microsoft.EntityFrameworkCore;
using MusicStreamerBackend.Data;

namespace MusicStreamerBackend.Services;

public interface IUserService
{
    Task<bool> StoreUserListeningEvent(UserListeningEventDto eventDtoData);
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
}