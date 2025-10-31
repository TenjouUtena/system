namespace SystemGame.Api.Data.Entities;

public class Building
{
    public int Id { get; set; }
    public int GridSquareId { get; set; }
    public BuildingType Type { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public int ConstructionProgress { get; set; } = 0; // 0-100
    public DateTime? ConstructionStartTime { get; set; }
    public DateTime? ConstructionCompleteTime { get; set; }
    public bool IsComplete { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual GridSquare GridSquare { get; set; } = null!;
    public virtual AppUser Player { get; set; } = null!;
}

