namespace SystemGame.Api.Data.Entities;

public class Builder
{
    public int Id { get; set; }
    public int PlanetId { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public int? AssignedBuildingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Planet Planet { get; set; } = null!;
    public virtual AppUser Player { get; set; } = null!;
    public virtual Building? AssignedBuilding { get; set; }
}

