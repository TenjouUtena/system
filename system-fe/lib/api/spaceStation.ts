import apiClient from './client';

export interface SpaceStation {
  id: number;
  name: string;
  systemId: number;
  systemName: string;
  playerId: string;
  playerName: string;
  ironAmount: number;
  copperAmount: number;
  fuelAmount: number;
  soilAmount: number;
  createdAt: string;
}

export async function getPlayerSpaceStations(gameId: number): Promise<SpaceStation[]> {
  const response = await apiClient.get<SpaceStation[]>(`/api/space-stations/game/${gameId}`);
  return response.data;
}

export async function getSystemSpaceStations(systemId: number): Promise<SpaceStation[]> {
  const response = await apiClient.get<SpaceStation[]>(`/api/space-stations/system/${systemId}`);
  return response.data;
}

export async function getSpaceStation(id: number): Promise<SpaceStation> {
  const response = await apiClient.get<SpaceStation>(`/api/space-stations/${id}`);
  return response.data;
}
