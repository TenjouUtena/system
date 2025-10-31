namespace SystemGame.Api.Data.Entities;

/// <summary>
/// Current state of a spaceship
/// </summary>
public enum ShipState
{
    UnderConstruction = 0,  // Being built at shipyard
    Idle = 1,               // Parked and waiting
    Moving = 2,             // Traveling to destination
    Colonizing = 3,         // Colony ship deploying to planet
    InCombat = 4,           // Engaged in battle
    Destroyed = 5           // Ship destroyed
}
