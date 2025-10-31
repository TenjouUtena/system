System is a multiplayer, real-time, long-term 4x style game.

The game runs in realtime, as you construct buildings and mine minerals, it happens if the game is running or not, realtime, in the background.    Games take days or weeks, and orders can be given or planned at any time.

The galazy is made up of systems.    Each system has one or more planets.    Each planet has a grid depending on the size.  THey have X and Y or 20 times the size, so a size 5 planet is 100x100.    Systems contain wormholes to other systems.   Each system should have between 1 and 4 wormhole connections to other systems.

Each square ont he grid has resources.    For now, let's make the resources Iron, Copper, Fuel, Soil but we want to easily add resources later.      For now we will use simple Iron, Copper, Fuel, and Soil.   When generating a planet grid, it should 'make sense' as in areas aof the grid should have good soil or good iron.

Buildings will be built on top of the grid squares.    Miners for mining iron, copper, or fuel, farms for using soil.

When creating the galaxy, we should ensire all systems are connected.




Frontend:   Next.js Typescript, Tailwind
Backend:  C#, Entity Framework.
Caching and session management, pub/sub: Redis
Long term storage: Postgresql
Communication: Signal R
