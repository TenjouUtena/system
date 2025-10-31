import { apiClient } from './client';
import type {
  CreateGameRequest,
  Game,
  GalaxyMap,
  SystemDetail,
} from '@/lib/types/game';

export const gameApi = {
  getGames: async (): Promise<Game[]> => {
    const response = await apiClient.get<Game[]>('/api/games');
    return response.data;
  },

  getGame: async (id: number): Promise<Game> => {
    const response = await apiClient.get<Game>(`/api/games/${id}`);
    return response.data;
  },

  createGame: async (data: CreateGameRequest): Promise<Game> => {
    const response = await apiClient.post<Game>('/api/games', data);
    return response.data;
  },

  joinGame: async (id: number): Promise<void> => {
    await apiClient.post(`/api/games/${id}/join`);
  },

  deleteGame: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/games/${id}`);
  },

  getGalaxyMap: async (gameId: number): Promise<GalaxyMap> => {
    const response = await apiClient.get<GalaxyMap>(`/api/games/${gameId}/galaxy`);
    return response.data;
  },

  getSystemDetail: async (gameId: number, systemId: number): Promise<SystemDetail> => {
    const response = await apiClient.get<SystemDetail>(`/api/games/${gameId}/systems/${systemId}`);
    return response.data;
  },
};

