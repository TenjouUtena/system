using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Models;

public class BuildingDto
{
    public int Id { get; set; }
    public int GridSquareId { get; set; }
    public BuildingType Type { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int ConstructionProgress { get; set; }
    public bool IsComplete { get; set; }
    public DateTime? ConstructionStartTime { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}

