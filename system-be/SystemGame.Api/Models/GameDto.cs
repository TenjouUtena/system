namespace SystemGame.Api.Models;

public class GameDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PlayerCount { get; set; }
    public int MaxPlayers { get; set; }
    public int SystemCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsJoined { get; set; }
    public bool IsCreator { get; set; }
}

