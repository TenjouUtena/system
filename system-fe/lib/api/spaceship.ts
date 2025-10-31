import apiClient from './client';
import {
  Spaceship,
  Shipyard,
  CreateSpaceshipRequest,
  CreateShipyardRequest,
  MoveSpaceshipRequest,
  ColonizePlanetRequest
} from '../types/spaceship';

// Shipyard API
export async function createShipyard(request: CreateShipyardRequest): Promise<Shipyard> {
  const response = await apiClient.post<Shipyard>('/api/spaceships/shipyards', request);
  return response.data;
}

export async function getGameShipyards(gameId: number): Promise<Shipyard[]> {
  const response = await apiClient.get<Shipyard[]>(`/api/spaceships/shipyards/game/${gameId}`);
  return response.data;
}

// Spaceship API
export async function createSpaceship(request: CreateSpaceshipRequest): Promise<Spaceship> {
  const response = await apiClient.post<Spaceship>('/api/spaceships', request);
  return response.data;
}

export async function getGameSpaceships(gameId: number): Promise<Spaceship[]> {
  const response = await apiClient.get<Spaceship[]>(`/api/spaceships/game/${gameId}`);
  return response.data;
}

export async function getSpaceship(id: number): Promise<Spaceship> {
  const response = await apiClient.get<Spaceship>(`/api/spaceships/${id}`);
  return response.data;
}

export async function getSystemSpaceships(systemId: number): Promise<Spaceship[]> {
  const response = await apiClient.get<Spaceship[]>(`/api/spaceships/system/${systemId}`);
  return response.data;
}

// Ship movement
export async function moveSpaceship(request: MoveSpaceshipRequest): Promise<Spaceship> {
  const response = await apiClient.post<Spaceship>(
    `/api/spaceships/${request.spaceshipId}/move`,
    request
  );
  return response.data;
}

// Colony operations
export async function colonizePlanet(request: ColonizePlanetRequest): Promise<{ success: boolean; message: string; planetId: number }> {
  const response = await apiClient.post<{ success: boolean; message: string; planetId: number }>(
    `/api/spaceships/${request.spaceshipId}/colonize`,
    request
  );
  return response.data;
}
