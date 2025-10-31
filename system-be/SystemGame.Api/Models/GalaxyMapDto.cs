namespace SystemGame.Api.Models;

public class GalaxyMapDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<SystemMapDto> Systems { get; set; } = new();
    public List<WormholeMapDto> Wormholes { get; set; } = new();
}

public class SystemMapDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int PlanetCount { get; set; }
}

public class WormholeMapDto
{
    public int Id { get; set; }
    public int SystemAId { get; set; }
    public int SystemBId { get; set; }
    public bool IsActive { get; set; }
}

