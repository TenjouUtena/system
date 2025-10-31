namespace SystemGame.Api.Data.Entities;

/// <summary>
/// Current state of a spaceship
/// </summary>
public enum ShipState
{
    UnderConstruction = 0,  // Being built at shipyard
    Idle = 1,               // Parked and waiting
    Active = 2,             // Active and operational
    Moving = 3,             // Traveling to destination
    Colonizing = 4,         // Colony ship deploying to planet
    InCombat = 5,           // Engaged in battle
    Fleeing = 6,            // Fleeing from battle
    Destroyed = 7           // Ship destroyed
}
