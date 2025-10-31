namespace SystemGame.Api.Models;

public class SpaceStationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public double IronAmount { get; set; }
    public double CopperAmount { get; set; }
    public double FuelAmount { get; set; }
    public double SoilAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

