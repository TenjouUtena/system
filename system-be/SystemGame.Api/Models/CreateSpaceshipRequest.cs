using System.ComponentModel.DataAnnotations;

namespace SystemGame.Api.Models;

public class CreateSpaceshipRequest
{
    [Required]
    public int GameId { get; set; }
    
    [Required]
    public int ShipyardId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Type { get; set; } = string.Empty;  // ShipType enum as string
}
