namespace SystemGame.Api.Models;

public class SystemDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public List<PlanetDto> Planets { get; set; } = new();
}

public class PlanetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Size { get; set; }
    public string Type { get; set; } = string.Empty;
    public int GridWidth => Size * 20;
    public int GridHeight => Size * 20;
}

