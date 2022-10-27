using Orleans;

namespace Grains;

public interface IUserGrain : IGrainWithGuidKey
{
    Task SetName(string name);
    Task<string> GetName();
    Task EnterRoom(Guid roomId);
    Task ExitRoom(Guid roomId);
    Task SendMessage(Guid roomId, string message);
    Task ReceiveMessage(Guid roomId, Guid userId, string message);
    Task ReceiveNotification(Guid roomId, string message);
    Task<UserInfo> GetUserInfo();
}


public class UserGrain : Grain, IUserGrain
{
    private readonly UserInfo _userInfo;

    public UserGrain()
    {
        _userInfo = new(this.GetPrimaryKey());
    }

    public Task SetName(string name)
    {
        _userInfo.Name = name;
        return Task.CompletedTask;
    }

    public Task<string> GetName() => Task.FromResult(_userInfo.Name);

    public Task EnterRoom(Guid roomId)
    {
        var room = GrainFactory.GetGrain<IRoomGrain>(roomId);
        room.Enter(_userInfo.Id);
        return Task.CompletedTask;
    }

    public Task ExitRoom(Guid roomId)
    {
        var room = GrainFactory.GetGrain<IRoomGrain>(roomId);
        room.Exit(_userInfo.Id);
        return Task.CompletedTask;
    }

    public Task SendMessage(Guid roomId, string message)
    {
        var room = GrainFactory.GetGrain<IRoomGrain>(roomId);
        room.SendMessage(_userInfo.Id, message);
        return Task.CompletedTask;
    }

    public async Task ReceiveMessage(Guid roomid, Guid userId, string message)
    {
        var senderName = await GetUserName(userId);
        var sendingRoomName = await GetRoomGrain(roomid).GetName();
        Console.WriteLine($"[{sendingRoomName}] {senderName}: {message}");
    }

    public async Task ReceiveNotification(Guid roomId, string message)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        var sendingRoomName = await GetRoomGrain(roomId).GetName();
        Console.WriteLine($"[{sendingRoomName}] {message}");
        Console.ResetColor();
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

    private IRoomGrain GetRoomGrain(Guid roomId)
    {
        return GrainFactory.GetGrain<IRoomGrain>(roomId);
    }
}
