namespace SystemGame.Api.Models;

public class BuilderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int? AssignedBuildingId { get; set; }
    public DateTime CreatedAt { get; set; }
}

