// Spaceship types

export interface Spaceship {
  id: number;
  playerId: string;
  gameId: number;
  name: string;
  type: string;
  state: string;
  shipyardId?: number;
  constructionProgress: number;
  constructionTimeSeconds: number;
  constructionStartTime?: string;
  constructionCompletedTime?: string;
  currentSystemId: number;
  currentSystemName?: string;
  positionX: number;
  positionY: number;
  speed: number;
  destinationSystemId?: number;
  destinationSystemName?: string;
  destinationX?: number;
  destinationY?: number;
  movementStartTime?: string;
  estimatedArrivalTime?: string;
  cargoIron?: number;
  cargoCopper?: number;
  cargoFuel?: number;
  cargoSoil?: number;
  cargoCapacity: number;
  health: number;
  maxHealth: number;
  attack: number;
  defense: number;
  createdAt: string;
  lastUpdatedAt: string;
}

export interface Shipyard {
  id: number;
  playerId: string;
  gameId: number;
  spaceStationId: number;
  spaceStationName: string;
  name: string;
  maxConcurrentBuilds: number;
  isActive: boolean;
  createdAt: string;
  currentBuildsCount: number;
  currentBuilds?: Spaceship[];
}

export interface CreateSpaceshipRequest {
  gameId: number;
  shipyardId: number;
  name: string;
  type: string;
}

export interface CreateShipyardRequest {
  gameId: number;
  spaceStationId: number;
  name: string;
  maxConcurrentBuilds?: number;
}

export interface MoveSpaceshipRequest {
  spaceshipId: number;
  destinationSystemId?: number;
  destinationX: number;
  destinationY: number;
}

export interface ColonizePlanetRequest {
  spaceshipId: number;
  planetId: number;
}

// Ship types
export const ShipTypes = {
  Scout: 'Scout',
  Colony: 'Colony',
  Freighter: 'Freighter',
  Destroyer: 'Destroyer',
  Cruiser: 'Cruiser',
  Carrier: 'Carrier',
  Capital: 'Capital'
} as const;

export type ShipType = typeof ShipTypes[keyof typeof ShipTypes];

// Ship states
export const ShipStates = {
  UnderConstruction: 'UnderConstruction',
  Idle: 'Idle',
  Moving: 'Moving',
  Colonizing: 'Colonizing',
  InCombat: 'InCombat',
  Destroyed: 'Destroyed'
} as const;

export type ShipState = typeof ShipStates[keyof typeof ShipStates];

// Helper functions
export function getShipTypeColor(type: string): string {
  switch (type) {
    case 'Scout':
      return 'text-blue-500';
    case 'Colony':
      return 'text-green-500';
    case 'Freighter':
      return 'text-yellow-500';
    case 'Destroyer':
      return 'text-red-500';
    case 'Cruiser':
      return 'text-purple-500';
    case 'Carrier':
      return 'text-indigo-500';
    case 'Capital':
      return 'text-orange-500';
    default:
      return 'text-gray-500';
  }
}

export function getShipStateColor(state: string): string {
  switch (state) {
    case 'UnderConstruction':
      return 'bg-yellow-100 text-yellow-800';
    case 'Idle':
      return 'bg-gray-100 text-gray-800';
    case 'Moving':
      return 'bg-blue-100 text-blue-800';
    case 'Colonizing':
      return 'bg-green-100 text-green-800';
    case 'InCombat':
      return 'bg-red-100 text-red-800';
    case 'Destroyed':
      return 'bg-black text-white';
    default:
      return 'bg-gray-100 text-gray-800';
  }
}

export function getShipTypeDescription(type: string): string {
  switch (type) {
    case 'Scout':
      return 'Fast exploration vessel';
    case 'Colony':
      return 'Colonizes new planets';
    case 'Freighter':
      return 'Large cargo transport';
    case 'Destroyer':
      return 'Medium combat ship';
    case 'Cruiser':
      return 'Heavy combat ship';
    case 'Carrier':
      return 'Command vessel';
    case 'Capital':
      return 'Flagship battleship';
    default:
      return 'Unknown ship type';
  }
}
