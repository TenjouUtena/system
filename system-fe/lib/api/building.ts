import { apiClient } from './client';
import type {
  Building,
  PlaceBuildingRequest,
  Builder,
  CreateBuilderRequest,
  SpaceStation,
} from '@/lib/types/building';

export const buildingApi = {
  placeBuilding: async (request: PlaceBuildingRequest): Promise<Building> => {
    const response = await apiClient.post<Building>('/api/buildings/place', request);
    return response.data;
  },

  getBuildingsForPlanet: async (planetId: number): Promise<Building[]> => {
    const response = await apiClient.get<Building[]>(`/api/buildings/planet/${planetId}`);
    return response.data;
  },

  createBuilder: async (request: CreateBuilderRequest): Promise<Builder> => {
    const response = await apiClient.post<Builder>('/api/builders/create', request);
    return response.data;
  },

  getBuildersForPlanet: async (planetId: number): Promise<Builder[]> => {
    const response = await apiClient.get<Builder[]>(`/api/builders/planet/${planetId}`);
    return response.data;
  },

  getSpaceStationsForSystem: async (systemId: number): Promise<SpaceStation[]> => {
    const response = await apiClient.get<SpaceStation[]>(`/api/space-stations/system/${systemId}`);
    return response.data;
  },

  getSpaceStation: async (id: number): Promise<SpaceStation> => {
    const response = await apiClient.get<SpaceStation>(`/api/space-stations/${id}`);
    return response.data;
  },
};

