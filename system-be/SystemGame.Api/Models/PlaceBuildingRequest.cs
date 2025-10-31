using System.ComponentModel.DataAnnotations;

namespace SystemGame.Api.Models;

public class PlaceBuildingRequest
{
    [Required]
    public int GridSquareId { get; set; }
    
    [Required]
    public BuildingTypeDto BuildingType { get; set; }
}

public enum BuildingTypeDto
{
    IronMiner = 1,
    CopperMiner = 2,
    FuelMiner = 3,
    Farm = 4
}

