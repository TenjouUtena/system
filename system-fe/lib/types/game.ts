export interface CreateGameRequest {
  name: string;
  description?: string;
  systemCount: number;
  maxPlayers: number;
}

export interface Game {
  id: number;
  name: string;
  description?: string;
  playerCount: number;
  maxPlayers: number;
  systemCount: number;
  isActive: boolean;
  createdAt: string;
  isJoined: boolean;
  isCreator: boolean;
}

export interface GalaxyMap {
  id: number;
  name: string;
  systems: SystemMap[];
  wormholes: WormholeMap[];
}

export interface SystemMap {
  id: number;
  name: string;
  x: number;
  y: number;
  planetCount: number;
}

export interface WormholeMap {
  id: number;
  systemAId: number;
  systemBId: number;
  isActive: boolean;
}

export interface SystemDetail {
  id: number;
  name: string;
  x: number;
  y: number;
  planets: Planet[];
}

export interface Planet {
  id: number;
  name: string;
  size: number;
  type: string;
  gridWidth: number;
  gridHeight: number;
}

