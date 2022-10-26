using Orleans;

namespace Grains;

public interface IRoomGrain : IGrainWithGuidKey
{
    Task SetName(UserInfo user, string name);
    Task SendMessage(UserInfo user, string message);
    Task<bool> Enter(UserInfo user);
    Task Exit(UserInfo user);
}

public class RoomGrain : Grain, IRoomGrain
{
    private readonly RoomInfo _roomInfo;

    public RoomGrain()
    {
        _roomInfo = new(this.GetPrimaryKey());
    }

    public Task<bool> Enter(UserInfo user)
    {
        if (_roomInfo.Users.Contains(user) == false)
        {
            _roomInfo.Users.Add(user);
        }
        NotifyAll($"{user.Name} has entered the room.");

        return Task.FromResult(true);
    }

    public Task Exit(UserInfo user)
    {
        if (_roomInfo.Users.Contains(user))
        {
            _roomInfo.Users.Remove(user);
        }
        NotifyAll($"{user.Name} has left the room.");
        return Task.FromResult(true);
    }

    public Task SendMessage(UserInfo user, string message)
    {
        foreach (var userInfo in _roomInfo.Users)
        {
            var userGrain = GrainFactory.GetGrain<UserGrain>(userInfo.Id);
            userGrain.ReceiveMessage(_roomInfo, user, message);
        }
        return Task.CompletedTask;
    }

    public Task SetName(UserInfo user, string name)
    {
        _roomInfo.Name = name;
        return Task.CompletedTask;
    }

    private Task NotifyAll(string message)
    {
        foreach (var userInfo in _roomInfo.Users)
        {
            var userGrain = GrainFactory.GetGrain<UserGrain>(userInfo.Id);
            userGrain.ReceiveNotification(_roomInfo, message);
        }
        return Task.CompletedTask;
    }

}