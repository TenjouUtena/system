using System.ComponentModel.DataAnnotations;

namespace SystemGame.Api.Models;

public class CreateBuilderRequest
{
    [Required]
    public int PlanetId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

