using Orleans;

namespace Grains;

public interface IUserGrain : IGrainWithGuidKey
{
    Task SetName(string name);
    Task EnterRoom(Guid roomId);
    Task ExitRoom(Guid roomId);
    Task SendMessage(Guid roomId, string message);
    Task ReceiveMessage(RoomInfo room, UserInfo user, string message);
    Task ReceiveNotification(RoomInfo room, string message);
}


public class UserGrain : Grain, IUserGrain
{
    private readonly UserInfo _userInfo;

    public UserGrain()
    {
        _userInfo = new(this.GetPrimaryKey());

        // Enter the lobby when we first create the user
        // Later we will change this to rehydrate and put the user back into the rooms they were in when we last saw them
        EnterRoom(Guid.Empty);
    }


    public Task SetName(string name)
    {
        _userInfo.Name = name;
        return Task.CompletedTask;
    }

    public Task EnterRoom(Guid roomId)
    {
        var room = GrainFactory.GetGrain<RoomGrain>(roomId);
        room.Enter(_userInfo);
        return Task.CompletedTask;
    }

    public Task ExitRoom(Guid roomId)
    {
        var room = GrainFactory.GetGrain<RoomGrain>(roomId);
        room.Exit(_userInfo);
        return Task.CompletedTask;
    }

    public Task SendMessage(Guid roomId, string message)
    {
        var room = GrainFactory.GetGrain<RoomGrain>(roomId);
        room.SendMessage(_userInfo, message);
        return Task.CompletedTask;
    }

    public Task ReceiveMessage(RoomInfo room, UserInfo user, string message)
    {
        Console.WriteLine($"[{room.Name}] {user.Name}: {message}");
        return Task.CompletedTask;
    }

    public Task ReceiveNotification(RoomInfo room, string message)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"[{room.Name}] {message}");
        Console.ResetColor();
        return Task.CompletedTask;
    }
}
