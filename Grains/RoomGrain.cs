using Orleans;

namespace Grains;

public interface IRoomGrain : IGrainWithGuidKey
{
    Task SetName(IUserGrain userGrain, string name);
    Task<string> GetName();
    Task SendMessage(UserInfo user, string message);
    Task<bool> Enter(UserInfo user);
    Task Exit(UserInfo user);
}

public class RoomGrain : Grain, IRoomGrain
{
    private RoomInfo _roomInfo = null!;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _roomInfo = new(this.GetPrimaryKey());
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<bool> Enter(UserInfo user)
    {
        if (_roomInfo.Participants.Contains(user.Id))
        {
            return false;
        }

        _roomInfo.Participants.Add(user.Id);

        await NotifyAll($"{user.Name} has entered the room.");

        return true;
    }

    public async Task Exit(UserInfo user)
    {
        if (_roomInfo.Participants.Contains(user.Id))
        {
            _roomInfo.Participants.Remove(user.Id);
        }

        await NotifyAll($"{user.Name} has left the room.");
    }

    public Task SendMessage(UserInfo user, string message)
    {
        foreach (var userInfo in _roomInfo.Participants)
        {
            var userGrain = GrainFactory.GetGrain<IUserGrain>(user.Id);
            userGrain.ReceiveMessage(_roomInfo, user, message);
        }
        return Task.CompletedTask;
    }

    public async Task SetName(IUserGrain userGrain, string name)
    {
        _roomInfo = _roomInfo with { Name = name };

        var userInfo = await userGrain.GetUserInfo();

        await NotifyAll($"{userInfo.Name} changed the name of the room.");
    }

    public Task<string> GetName() => Task.FromResult(_roomInfo.Name);

    private Task NotifyAll(string message)
    {
        foreach (var userId in _roomInfo.Participants)
        {
            var userGrain = GrainFactory.GetGrain<IUserGrain>(userId);
            userGrain.ReceiveNotification(_roomInfo, message);
        }
        return Task.CompletedTask;
    }

}