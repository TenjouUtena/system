'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { agentApi } from '@/lib/api/agent';
import { Agent, AgentStates, AgentTypes } from '@/lib/types/agent';
import Link from 'next/link';

export default function AgentsPage() {
  const params = useParams();
  const router = useRouter();
  const gameId = parseInt(params.id as string);
  
  const [agents, setAgents] = useState<Agent[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filterState, setFilterState] = useState<string>('all');
  const [filterType, setFilterType] = useState<string>('all');

  useEffect(() => {
    loadAgents();
  }, [gameId]);

  const loadAgents = async () => {
    try {
      setLoading(true);
      const data = await agentApi.getGameAgents(gameId);
      setAgents(data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load agents');
    } finally {
      setLoading(false);
    }
  };

  const handlePause = async (agentId: number) => {
    try {
      await agentApi.pauseAgent(agentId);
      await loadAgents();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Failed to pause agent');
    }
  };

  const handleResume = async (agentId: number) => {
    try {
      await agentApi.resumeAgent(agentId);
      await loadAgents();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Failed to resume agent');
    }
  };

  const handleDelete = async (agentId: number) => {
    if (!confirm('Are you sure you want to delete this agent?')) {
      return;
    }

    try {
      await agentApi.deleteAgent(agentId);
      await loadAgents();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Failed to delete agent');
    }
  };

  const getStateColor = (state: string) => {
    switch (state) {
      case AgentStates.ACTIVE:
        return 'bg-green-500';
      case AgentStates.IDLE:
        return 'bg-blue-500';
      case AgentStates.PAUSED:
        return 'bg-yellow-500';
      case AgentStates.ERROR:
        return 'bg-red-500';
      case AgentStates.COMPLETED:
        return 'bg-gray-500';
      default:
        return 'bg-gray-400';
    }
  };

  const filteredAgents = agents.filter(agent => {
    if (filterState !== 'all' && agent.state !== filterState) return false;
    if (filterType !== 'all' && agent.type !== filterType) return false;
    return true;
  });

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 p-8">
        <div className="max-w-7xl mx-auto">
          <div className="text-center">Loading agents...</div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Agents</h1>
            <p className="text-gray-600 mt-1">Manage your autonomous agents</p>
          </div>
          <div className="flex gap-4">
            <Link
              href={`/games/${gameId}`}
              className="px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700"
            >
              Back to Game
            </Link>
            <button
              onClick={() => router.push(`/games/${gameId}/agents/create`)}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
            >
              Create Agent
            </button>
          </div>
        </div>

        {error && (
          <div className="mb-4 p-4 bg-red-100 border border-red-400 text-red-700 rounded">
            {error}
          </div>
        )}

        {/* Filters */}
        <div className="bg-white p-4 rounded-lg shadow-md mb-6 flex gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              State
            </label>
            <select
              value={filterState}
              onChange={(e) => setFilterState(e.target.value)}
              className="border border-gray-300 rounded-md px-3 py-2"
            >
              <option value="all">All States</option>
              {Object.values(AgentStates).map((state) => (
                <option key={state} value={state}>
                  {state}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Type
            </label>
            <select
              value={filterType}
              onChange={(e) => setFilterType(e.target.value)}
              className="border border-gray-300 rounded-md px-3 py-2"
            >
              <option value="all">All Types</option>
              {Object.values(AgentTypes).map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Agents List */}
        {filteredAgents.length === 0 ? (
          <div className="bg-white p-8 rounded-lg shadow-md text-center">
            <p className="text-gray-500">No agents found. Create your first agent to get started!</p>
          </div>
        ) : (
          <div className="grid gap-4">
            {filteredAgents.map((agent) => (
              <div
                key={agent.id}
                className="bg-white p-6 rounded-lg shadow-md hover:shadow-lg transition-shadow"
              >
                <div className="flex justify-between items-start">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <h3 className="text-xl font-semibold text-gray-900">
                        {agent.name}
                      </h3>
                      <span className={`px-2 py-1 rounded-full text-xs text-white ${getStateColor(agent.state)}`}>
                        {agent.state}
                      </span>
                      <span className="px-2 py-1 rounded-full text-xs bg-gray-200 text-gray-700">
                        {agent.type}
                      </span>
                    </div>
                    <div className="grid grid-cols-2 gap-4 text-sm text-gray-600">
                      {agent.currentBehaviorName && (
                        <div>
                          <span className="font-medium">Behavior:</span> {agent.currentBehaviorName}
                        </div>
                      )}
                      {agent.builderName && (
                        <div>
                          <span className="font-medium">Builder:</span> {agent.builderName}
                        </div>
                      )}
                      {agent.currentSystemName && (
                        <div>
                          <span className="font-medium">System:</span> {agent.currentSystemName}
                        </div>
                      )}
                      <div>
                        <span className="font-medium">Last Run:</span>{' '}
                        {new Date(agent.lastExecutionTime).toLocaleString()}
                      </div>
                    </div>
                  </div>
                  <div className="flex gap-2">
                    <Link
                      href={`/games/${gameId}/agents/${agent.id}`}
                      className="px-3 py-1 bg-blue-600 text-white rounded hover:bg-blue-700 text-sm"
                    >
                      View
                    </Link>
                    {agent.state === AgentStates.ACTIVE && (
                      <button
                        onClick={() => handlePause(agent.id)}
                        className="px-3 py-1 bg-yellow-600 text-white rounded hover:bg-yellow-700 text-sm"
                      >
                        Pause
                      </button>
                    )}
                    {agent.state === AgentStates.PAUSED && (
                      <button
                        onClick={() => handleResume(agent.id)}
                        className="px-3 py-1 bg-green-600 text-white rounded hover:bg-green-700 text-sm"
                      >
                        Resume
                      </button>
                    )}
                    <button
                      onClick={() => handleDelete(agent.id)}
                      className="px-3 py-1 bg-red-600 text-white rounded hover:bg-red-700 text-sm"
                    >
                      Delete
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
