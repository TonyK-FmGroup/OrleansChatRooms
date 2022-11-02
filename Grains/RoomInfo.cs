using Orleans;

namespace Grains;

[Immutable]
[GenerateSerializer]
public record class RoomInfo
{
    public Guid Id { get; private set; }
    public string Name { get; set; } = "New Room";
    public List<Guid> Participants { get; } = new();

    public RoomInfo(Guid id)
    {
        Id = id;
    }

}
