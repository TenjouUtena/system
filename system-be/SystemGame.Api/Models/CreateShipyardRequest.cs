using System.ComponentModel.DataAnnotations;

namespace SystemGame.Api.Models;

public class CreateShipyardRequest
{
    [Required]
    public int GameId { get; set; }
    
    [Required]
    public int SpaceStationId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public int MaxConcurrentBuilds { get; set; } = 1;
}
