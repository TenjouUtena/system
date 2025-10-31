import { apiClient } from './client';
import type { PlanetGrid, PlanetGridSummary } from '@/lib/types/planet';

export const planetApi = {
  getPlanetGrid: async (planetId: number): Promise<PlanetGrid> => {
    const response = await apiClient.get<PlanetGrid>(`/api/planets/${planetId}/grid`);
    return response.data;
  },

  getPlanetGridSummary: async (planetId: number): Promise<PlanetGridSummary> => {
    const response = await apiClient.get<PlanetGridSummary>(`/api/planets/${planetId}/grid/summary`);
    return response.data;
  },
};

