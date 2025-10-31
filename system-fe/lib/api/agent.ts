import { apiClient } from './client';
import { Agent, CreateAgentRequest, UpdateAgentRequest, AgentLog, BehaviorInfo } from '../types/agent';

export const agentApi = {
  /**
   * Get all agents for a specific game
   */
  getGameAgents: async (gameId: number): Promise<Agent[]> => {
    const response = await apiClient.get<Agent[]>(`/api/agents/game/${gameId}`);
    return response.data;
  },

  /**
   * Get a specific agent by ID
   */
  getAgent: async (id: number): Promise<Agent> => {
    const response = await apiClient.get<Agent>(`/api/agents/${id}`);
    return response.data;
  },

  /**
   * Create a new agent
   */
  createAgent: async (request: CreateAgentRequest): Promise<Agent> => {
    const response = await apiClient.post<Agent>('/api/agents', request);
    return response.data;
  },

  /**
   * Update an existing agent
   */
  updateAgent: async (id: number, request: UpdateAgentRequest): Promise<Agent> => {
    const response = await apiClient.put<Agent>(`/api/agents/${id}`, request);
    return response.data;
  },

  /**
   * Pause an agent
   */
  pauseAgent: async (id: number): Promise<void> => {
    await apiClient.post(`/api/agents/${id}/pause`);
  },

  /**
   * Resume an agent
   */
  resumeAgent: async (id: number): Promise<void> => {
    await apiClient.post(`/api/agents/${id}/resume`);
  },

  /**
   * Delete an agent
   */
  deleteAgent: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/agents/${id}`);
  },

  /**
   * Get agent logs
   */
  getAgentLogs: async (id: number, limit: number = 100): Promise<AgentLog[]> => {
    const response = await apiClient.get<AgentLog[]>(`/api/agents/${id}/logs?limit=${limit}`);
    return response.data;
  },

  /**
   * Get available behaviors
   */
  getAvailableBehaviors: async (): Promise<BehaviorInfo[]> => {
    const response = await apiClient.get<BehaviorInfo[]>('/api/agents/behaviors');
    return response.data;
  },

  /**
   * Test an agent's behavior
   */
  testBehavior: async (id: number): Promise<any> => {
    const response = await apiClient.post(`/api/agents/${id}/test-behavior`);
    return response.data;
  },
};
