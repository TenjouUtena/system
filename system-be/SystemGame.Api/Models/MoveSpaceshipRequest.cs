using System.ComponentModel.DataAnnotations;

namespace SystemGame.Api.Models;

public class MoveSpaceshipRequest
{
    [Required]
    public int SpaceshipId { get; set; }
    
    // Either move within system or to another system via wormhole
    public int? DestinationSystemId { get; set; }  // If traveling to another system
    
    [Required]
    public double DestinationX { get; set; }
    
    [Required]
    public double DestinationY { get; set; }
}
