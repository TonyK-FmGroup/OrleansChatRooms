using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Utilities;
using System;

namespace Grains;

public interface IRoomGrain : IGrainWithGuidKey
{
    Task SetName(IUserGrain userGrain, string name);
    Task<string> GetName();
    Task SendMessage(UserInfo user, string message);
    Task<bool> Enter(IUserGrain userGrain, UserInfo userInfo);
    Task Exit(UserInfo user);
}

public class RoomGrain : Grain, IRoomGrain
{
    private RoomInfo _roomInfo = null!;
    private readonly ObserverManager<IUserGrain> _subsManager;

    public RoomGrain(ILogger<IUserGrain> logger)
    {
        _subsManager =
      new ObserverManager<IUserGrain>(TimeSpan.FromMinutes(5), logger);
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _roomInfo = new(this.GetPrimaryKey());
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<bool> Enter(IUserGrain userGrain, UserInfo userInfo)
    {
        if (_roomInfo.Participants.Contains(userInfo.Id))
        {
            return false;
        }

        _roomInfo.Participants.Add(userInfo.Id);
        await Subscribe(userGrain);

        await NotifyAll($"{userInfo.Name} has entered the room.");

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
        /* foreach (var userInfo in _roomInfo.Participants)
         {
             var userGrain = GrainFactory.GetGrain<IUserGrain>(user.Id);
             userGrain.ReceiveMessage(_roomInfo, user, message);
         }*/

        _subsManager.Notify(s => s.ReceiveMessage(_roomInfo, user, message));
        return Task.CompletedTask;
    }

    public async Task SetName(IUserGrain userGrain, string name)
    {
        _roomInfo = _roomInfo with { Name = name };

        var userInfo = await userGrain.GetUserInfo();

        await NotifyAll($"{userInfo.Name} changed the name of the room.");
    }

    public Task<string> GetName() => Task.FromResult(_roomInfo.Name);

    public Task Subscribe(IUserGrain observer)
    {
        _subsManager.Subscribe(observer, observer);
        return Task.CompletedTask;
    }

    /* public Task UnSubscribe(IUserGrain observer)
     {
         _subsManager.Unsubscribe(observer, observer);

         return Task.CompletedTask;
     }*/

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