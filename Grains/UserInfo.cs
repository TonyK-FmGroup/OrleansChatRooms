using Orleans;

namespace Grains;

[GenerateSerializer]
public record class UserInfo
{
    public Guid Id { get; private set; }
    public string Name { get; set; } = string.Empty;

    public List<RoomInfo> Rooms { get; } = new();

    public UserInfo(Guid id)
    {
        Id = id;
    }
}
