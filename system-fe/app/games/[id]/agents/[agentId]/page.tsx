'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { agentApi } from '@/lib/api/agent';
import { Agent, AgentLog, AgentStates } from '@/lib/types/agent';
import Link from 'next/link';

export default function AgentDetailPage() {
  const params = useParams();
  const router = useRouter();
  const gameId = parseInt(params.id as string);
  const agentId = parseInt(params.agentId as string);
  
  const [agent, setAgent] = useState<Agent | null>(null);
  const [logs, setLogs] = useState<AgentLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [logsLoading, setLogsLoading] = useState(false);
  const [editMode, setEditMode] = useState(false);
  const [editName, setEditName] = useState('');
  const [editBehaviorName, setEditBehaviorName] = useState('');
  const [editBehaviorConfig, setEditBehaviorConfig] = useState('');

  useEffect(() => {
    loadAgent();
    loadLogs();
    
    // Auto-refresh every 10 seconds
    const interval = setInterval(() => {
      loadAgent();
      loadLogs();
    }, 10000);
    
    return () => clearInterval(interval);
  }, [agentId]);

  const loadAgent = async () => {
    try {
      setLoading(true);
      const data = await agentApi.getAgent(agentId);
      setAgent(data);
      setEditName(data.name);
      setEditBehaviorName(data.currentBehaviorName || '');
      setEditBehaviorConfig(data.behaviorConfig || '');
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load agent');
    } finally {
      setLoading(false);
    }
  };

  const loadLogs = async () => {
    try {
      setLogsLoading(true);
      const data = await agentApi.getAgentLogs(agentId, 50);
      setLogs(data);
    } catch (err: any) {
      console.error('Failed to load logs:', err);
    } finally {
      setLogsLoading(false);
    }
  };

  const handleUpdate = async () => {
    try {
      await agentApi.updateAgent(agentId, {
        name: editName,
        behaviorName: editBehaviorName,
        behaviorConfig: editBehaviorConfig,
      });
      setEditMode(false);
      await loadAgent();
      alert('Agent updated successfully');
    } catch (err: any) {
      alert(err.response?.data?.error || 'Failed to update agent');
    }
  };

  const handlePause = async () => {
    try {
      await agentApi.pauseAgent(agentId);
      await loadAgent();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Failed to pause agent');
    }
  };

  const handleResume = async () => {
    try {
      await agentApi.resumeAgent(agentId);
      await loadAgent();
    } catch (err: any) {
      alert(err.response?.data?.error || 'Failed to resume agent');
    }
  };

  const handleDelete = async () => {
    if (!confirm('Are you sure you want to delete this agent?')) {
      return;
    }

    try {
      await agentApi.deleteAgent(agentId);
      router.push(`/games/${gameId}/agents`);
    } catch (err: any) {
      alert(err.response?.data?.error || 'Failed to delete agent');
    }
  };

  const handleTestBehavior = async () => {
    try {
      const result = await agentApi.testBehavior(agentId);
      alert(`Behavior test:\n${JSON.stringify(result, null, 2)}`);
    } catch (err: any) {
      alert(err.response?.data?.error || 'Failed to test behavior');
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

  const getLogLevelColor = (level: string) => {
    switch (level) {
      case 'Error':
        return 'text-red-600';
      case 'Warning':
        return 'text-yellow-600';
      case 'Information':
        return 'text-blue-600';
      default:
        return 'text-gray-600';
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 p-8">
        <div className="max-w-7xl mx-auto">
          <div className="text-center">Loading agent...</div>
        </div>
      </div>
    );
  }

  if (!agent) {
    return (
      <div className="min-h-screen bg-gray-50 p-8">
        <div className="max-w-7xl mx-auto">
          <div className="text-center text-red-600">Agent not found</div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">{agent.name}</h1>
            <p className="text-gray-600 mt-1">Agent ID: {agent.id}</p>
          </div>
          <Link
            href={`/games/${gameId}/agents`}
            className="px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700"
          >
            Back to Agents
          </Link>
        </div>

        {error && (
          <div className="mb-4 p-4 bg-red-100 border border-red-400 text-red-700 rounded">
            {error}
          </div>
        )}

        {/* Agent Details */}
        <div className="bg-white p-6 rounded-lg shadow-md mb-6">
          <div className="flex justify-between items-start mb-6">
            <h2 className="text-xl font-semibold text-gray-900">Agent Details</h2>
            <div className="flex gap-2">
              {!editMode && (
                <button
                  onClick={() => setEditMode(true)}
                  className="px-3 py-1 bg-blue-600 text-white rounded hover:bg-blue-700 text-sm"
                >
                  Edit
                </button>
              )}
              {agent.state === AgentStates.ACTIVE && (
                <button
                  onClick={handlePause}
                  className="px-3 py-1 bg-yellow-600 text-white rounded hover:bg-yellow-700 text-sm"
                >
                  Pause
                </button>
              )}
              {agent.state === AgentStates.PAUSED && (
                <button
                  onClick={handleResume}
                  className="px-3 py-1 bg-green-600 text-white rounded hover:bg-green-700 text-sm"
                >
                  Resume
                </button>
              )}
              <button
                onClick={handleTestBehavior}
                className="px-3 py-1 bg-purple-600 text-white rounded hover:bg-purple-700 text-sm"
              >
                Test
              </button>
              <button
                onClick={handleDelete}
                className="px-3 py-1 bg-red-600 text-white rounded hover:bg-red-700 text-sm"
              >
                Delete
              </button>
            </div>
          </div>

          {editMode ? (
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Name
                </label>
                <input
                  type="text"
                  value={editName}
                  onChange={(e) => setEditName(e.target.value)}
                  className="w-full border border-gray-300 rounded-md px-3 py-2"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Behavior Name
                </label>
                <input
                  type="text"
                  value={editBehaviorName}
                  onChange={(e) => setEditBehaviorName(e.target.value)}
                  className="w-full border border-gray-300 rounded-md px-3 py-2"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Behavior Config (JSON)
                </label>
                <textarea
                  value={editBehaviorConfig}
                  onChange={(e) => setEditBehaviorConfig(e.target.value)}
                  rows={6}
                  className="w-full border border-gray-300 rounded-md px-3 py-2 font-mono text-sm"
                />
              </div>
              <div className="flex gap-2">
                <button
                  onClick={handleUpdate}
                  className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                >
                  Save
                </button>
                <button
                  onClick={() => setEditMode(false)}
                  className="px-4 py-2 bg-gray-600 text-white rounded hover:bg-gray-700"
                >
                  Cancel
                </button>
              </div>
            </div>
          ) : (
            <div className="grid grid-cols-2 gap-4">
              <div>
                <span className="font-medium text-gray-700">Type:</span>
                <span className="ml-2 text-gray-900">{agent.type}</span>
              </div>
              <div>
                <span className="font-medium text-gray-700">State:</span>
                <span className={`ml-2 px-2 py-1 rounded-full text-xs text-white ${getStateColor(agent.state)}`}>
                  {agent.state}
                </span>
              </div>
              <div>
                <span className="font-medium text-gray-700">Behavior:</span>
                <span className="ml-2 text-gray-900">{agent.currentBehaviorName || 'None'}</span>
              </div>
              <div>
                <span className="font-medium text-gray-700">Last Execution:</span>
                <span className="ml-2 text-gray-900">
                  {new Date(agent.lastExecutionTime).toLocaleString()}
                </span>
              </div>
              {agent.builderName && (
                <div>
                  <span className="font-medium text-gray-700">Builder:</span>
                  <span className="ml-2 text-gray-900">{agent.builderName}</span>
                </div>
              )}
              {agent.currentSystemName && (
                <div>
                  <span className="font-medium text-gray-700">Current System:</span>
                  <span className="ml-2 text-gray-900">{agent.currentSystemName}</span>
                </div>
              )}
              {agent.currentPlanetName && (
                <div>
                  <span className="font-medium text-gray-700">Current Planet:</span>
                  <span className="ml-2 text-gray-900">{agent.currentPlanetName}</span>
                </div>
              )}
              <div>
                <span className="font-medium text-gray-700">Created:</span>
                <span className="ml-2 text-gray-900">
                  {new Date(agent.createdAt).toLocaleString()}
                </span>
              </div>
              {agent.behaviorConfig && (
                <div className="col-span-2">
                  <span className="font-medium text-gray-700">Behavior Config:</span>
                  <pre className="mt-2 p-3 bg-gray-100 rounded text-sm overflow-x-auto">
                    {(() => {
                      try {
                        return JSON.stringify(JSON.parse(agent.behaviorConfig), null, 2);
                      } catch {
                        return agent.behaviorConfig;
                      }
                    })()}
                  </pre>
                </div>
              )}
            </div>
          )}
        </div>

        {/* Activity Logs */}
        <div className="bg-white p-6 rounded-lg shadow-md">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-xl font-semibold text-gray-900">Activity Logs</h2>
            <button
              onClick={loadLogs}
              className="px-3 py-1 bg-blue-600 text-white rounded hover:bg-blue-700 text-sm"
              disabled={logsLoading}
            >
              {logsLoading ? 'Loading...' : 'Refresh'}
            </button>
          </div>

          {logs.length === 0 ? (
            <p className="text-gray-500 text-center py-4">No logs yet</p>
          ) : (
            <div className="space-y-2 max-h-96 overflow-y-auto">
              {logs.map((log) => (
                <div
                  key={log.id}
                  className="p-3 border border-gray-200 rounded hover:bg-gray-50"
                >
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        <span className={`font-medium ${getLogLevelColor(log.level)}`}>
                          {log.level}
                        </span>
                        <span className="text-xs text-gray-500">
                          {new Date(log.timestamp).toLocaleString()}
                        </span>
                      </div>
                      <p className="text-gray-900">{log.message}</p>
                      {log.data && (
                        <pre className="mt-2 text-xs bg-gray-100 p-2 rounded overflow-x-auto">
                          {(() => {
                            try {
                              return JSON.stringify(JSON.parse(log.data), null, 2);
                            } catch {
                              return log.data;
                            }
                          })()}
                        </pre>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
