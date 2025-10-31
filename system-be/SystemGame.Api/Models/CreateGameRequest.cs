using System.ComponentModel.DataAnnotations;

namespace SystemGame.Api.Models;

public class CreateGameRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(10, 100)]
    public int SystemCount { get; set; } = 20;

    [Range(2, 100)]
    public int MaxPlayers { get; set; } = 50;
}

