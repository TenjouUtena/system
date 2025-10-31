namespace SystemGame.Api.Models;

public class ShipyardDto
{
    public int Id { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public int GameId { get; set; }
    public int SpaceStationId { get; set; }
    public string SpaceStationName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MaxConcurrentBuilds { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Current construction status
    public int CurrentBuildsCount { get; set; }
    public List<SpaceshipDto>? CurrentBuilds { get; set; }
}
