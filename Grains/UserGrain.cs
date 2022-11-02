using Orleans;
namespace Grains;

public interface IUserGrain : IGrainWithGuidKey, IGrainObserver
{
    Task SetName(string name);
    Task<string> GetName();
    Task EnterRoom(Guid roomId);
    Task ExitRoom(Guid roomId);
    Task SendMessage(Guid roomId, string message);
    void ReceiveMessage(RoomInfo room, UserInfo user, string message);
    Task ReceiveNotification(RoomInfo room, string message);
    Task<UserInfo> GetUserInfo();
}

public class UserGrain : Grain, IUserGrain
{
    private UserInfo _userInfo = null!;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _userInfo = new(this.GetPrimaryKey());
        return base.OnActivateAsync(cancellationToken);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        foreach (var roomInfo in _userInfo.Rooms)
        {
            ExitRoom(roomInfo.Id);
        }
        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    public Task SetName(string name)
    {
        _userInfo = _userInfo with { Name = name };
        return Task.CompletedTask;
    }

    public Task<string> GetName() => Task.FromResult(_userInfo.Name);

    public async Task EnterRoom(Guid roomId)
    {
        var room = GrainFactory.GetGrain<IRoomGrain>(roomId);
        var entered = await room.Enter(this.AsReference<IUserGrain>(), _userInfo);
    }

    public Task ExitRoom(Guid roomId)
    {
        var room = GrainFactory.GetGrain<IRoomGrain>(roomId);
        room.Exit(_userInfo);
        
        return Task.CompletedTask;
    }

    public Task SendMessage(Guid roomId, string message)
    {
        var room = GrainFactory.GetGrain<IRoomGrain>(roomId);
        room.SendMessage(_userInfo, message);
        return Task.CompletedTask;
    }

    public void ReceiveMessage(RoomInfo room, UserInfo user, string message)
    {
        Console.WriteLine($"[{room.Name}] {user.Name}: {message}");
    }

    public Task ReceiveNotification(RoomInfo room, string message)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"[{room.Name}] {message}");
        Console.ResetColor();
        return Task.CompletedTask;
    }

    public Task<UserInfo> GetUserInfo()
    {
        return Task.FromResult(_userInfo);
    }

    private IUserGrain GetUserGrain(Guid userId)
    {
        return GrainFactory.GetGrain<IUserGrain>(userId);
    }

    private async Task<string> GetUserName(Guid userId)
    {
        var userGrain = GetUserGrain(userId);
        var userInfo = await userGrain.GetUserInfo();
        return userInfo.Name;
    }
}
