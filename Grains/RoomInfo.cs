namespace Grains;

public record class RoomInfo
{
    public Guid Id { get; private set; }
    public string Name { get; set; } = "New Room";
    public List<UserInfo> Users { get; } = new();

    public RoomInfo(Guid id)
    {
        Id = id;
    }

}
