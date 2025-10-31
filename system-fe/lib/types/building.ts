export enum BuildingType {
  IronMiner = 1,
  CopperMiner = 2,
  FuelMiner = 3,
  Farm = 4,
}

export interface Building {
  id: number;
  gridSquareId: number;
  type: BuildingType;
  playerId: string;
  playerName: string;
  constructionProgress: number;
  isComplete: boolean;
  constructionStartTime?: string;
  x: number;
  y: number;
}

export interface PlaceBuildingRequest {
  gridSquareId: number;
  buildingType: BuildingType;
}

export interface Builder {
  id: number;
  name: string;
  isAvailable: boolean;
  assignedBuildingId?: number;
  createdAt: string;
}

export interface CreateBuilderRequest {
  planetId: number;
  name: string;
}

export interface SpaceStation {
  id: number;
  name: string;
  playerId: string;
  playerName: string;
  ironAmount: number;
  copperAmount: number;
  fuelAmount: number;
  soilAmount: number;
  createdAt: string;
}

