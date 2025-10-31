namespace SystemGame.Api.Data.Entities;

public class Wormhole
{
    public int Id { get; set; }
    public int SystemAId { get; set; }
    public int SystemBId { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual StarSystem SystemA { get; set; } = null!;
    public virtual StarSystem SystemB { get; set; } = null!;
}

