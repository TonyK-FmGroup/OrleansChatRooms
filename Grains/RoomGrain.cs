using Orleans;

namespace Grains;

public interface IRoomGrain : IGrainWithGuidKey
{
    Task SetName(Guid userId, string name);
    Task<string> GetName();
    Task SendMessage(Guid userId, string message);
    Task<bool> Enter(Guid userId);
    Task Exit(Guid userId);
}

public class RoomGrain : Grain, IRoomGrain
{
    private readonly RoomInfo _roomInfo;

    public RoomGrain()
    {
        _roomInfo = new(this.GetPrimaryKey());
    }

    public async Task<bool> Enter(Guid userId)
    {
        if (_roomInfo.Participants.Contains(userId) == false)
        {
            _roomInfo.Participants.Add(userId);
        }

        var username = await GetUserName(userId);

        await NotifyAll($"{username} has entered the room.");

        return true;
    }

    public async Task Exit(Guid userId)
    {
        if (_roomInfo.Participants.Contains(userId))
        {
            _roomInfo.Participants.Remove(userId);
        }

        var username = await GetUserName(userId);
        await NotifyAll($"{username} has left the room.");
    }

    public Task SendMessage(Guid userId, string message)
    {
        foreach (var userInfo in _roomInfo.Participants)
        {
            var userGrain = GrainFactory.GetGrain<IUserGrain>(userId);
            userGrain.ReceiveMessage(_roomInfo.Id, userId, message);
        }
        return Task.CompletedTask;
    }

    public Task SetName(Guid userId, string name)
    {
        _roomInfo.Name = name;
        return Task.CompletedTask;
    }

    public Task<string> GetName() => Task.FromResult(_roomInfo.Name);

    private async Task NotifyAll(string message)
    {
        foreach (var userId in _roomInfo.Participants)
        {
            var userGrain = GrainFactory.GetGrain<IUserGrain>(userId);
            await userGrain.ReceiveNotification(_roomInfo.Id, message);
        }
    }

    private IUserGrain GetUserGrain(Guid userId)
    {
        return GrainFactory.GetGrain<IUserGrain>(userId);

    }

    private async Task<string> GetUserName(Guid userId)
    {
        var userGrain = GetUserGrain(userId);
        return await userGrain.GetName();
    }

}