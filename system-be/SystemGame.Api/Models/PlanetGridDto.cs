namespace SystemGame.Api.Models;

public class PlanetGridDto
{
    public int Id { get; set; }
    public int PlanetId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public List<GridSquareDto> Squares { get; set; } = new();
}

public class GridSquareDto
{
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public double? IronAmount { get; set; }
    public double? CopperAmount { get; set; }
    public double? FuelAmount { get; set; }
    public double? SoilAmount { get; set; }
}

