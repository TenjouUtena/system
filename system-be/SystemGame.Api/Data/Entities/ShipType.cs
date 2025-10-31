namespace SystemGame.Api.Data.Entities;

/// <summary>
/// Types of spaceships available in the game
/// </summary>
public enum ShipType
{
    Scout = 0,          // Fast, low cost, exploration
    Colony = 1,         // Colonizes new planets
    Freighter = 2,      // Large cargo capacity for resource transport
    Destroyer = 3,      // Combat ship, medium firepower
    Cruiser = 4,        // Combat ship, heavy firepower
    Carrier = 5,        // Carries smaller ships, command vessel
    Capital = 6         // Flagship, most powerful
}
