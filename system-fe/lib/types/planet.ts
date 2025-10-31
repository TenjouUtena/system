export interface PlanetGrid {
  id: number;
  planetId: number;
  width: number;
  height: number;
  squares: GridSquare[];
}

export interface GridSquare {
  id: number;
  x: number;
  y: number;
  ironAmount?: number;
  copperAmount?: number;
  fuelAmount?: number;
  soilAmount?: number;
}

export interface PlanetGridSummary {
  planetId: number;
  planetName: string;
  gridSize: number;
  totalIron: number;
  totalCopper: number;
  totalFuel: number;
  totalSoil: number;
  ironSquares: number;
  copperSquares: number;
  fuelSquares: number;
  soilSquares: number;
}

