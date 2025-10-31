using System.ComponentModel.DataAnnotations;

namespace SystemGame.Api.Models;

public class ColonizePlanetRequest
{
    [Required]
    public int SpaceshipId { get; set; }  // Must be a Colony ship
    
    [Required]
    public int PlanetId { get; set; }
}
