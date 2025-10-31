'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { agentApi } from '@/lib/api/agent';
import { BehaviorInfo, AgentTypes } from '@/lib/types/agent';
import Link from 'next/link';

export default function CreateAgentPage() {
  const params = useParams();
  const router = useRouter();
  const gameId = parseInt(params.id as string);
  
  const [name, setName] = useState('');
  const [type, setType] = useState(AgentTypes.BUILDER);
  const [behaviorName, setBehaviorName] = useState('');
  const [behaviorConfig, setBehaviorConfig] = useState('');
  const [builderId, setBuilderId] = useState<number | undefined>();
  const [behaviors, setBehaviors] = useState<BehaviorInfo[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadBehaviors();
  }, []);

  const loadBehaviors = async () => {
    try {
      const data = await agentApi.getAvailableBehaviors();
      setBehaviors(data);
      if (data.length > 0) {
        setBehaviorName(data[0].name);
      }
    } catch (err) {
      console.error('Failed to load behaviors:', err);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const agent = await agentApi.createAgent({
        gameId,
        type,
        name,
        behaviorName: behaviorName || undefined,
        behaviorConfig: behaviorConfig || undefined,
        builderId: builderId || undefined,
      });

      router.push(`/games/${gameId}/agents/${agent.id}`);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to create agent');
    } finally {
      setLoading(false);
    }
  };

  const selectedBehavior = behaviors.find(b => b.name === behaviorName);

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-3xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Create Agent</h1>
            <p className="text-gray-600 mt-1">Configure a new autonomous agent</p>
          </div>
          <Link
            href={`/games/${gameId}/agents`}
            className="px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700"
          >
            Cancel
          </Link>
        </div>

        {error && (
          <div className="mb-4 p-4 bg-red-100 border border-red-400 text-red-700 rounded">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="bg-white p-6 rounded-lg shadow-md space-y-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Name *
            </label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              className="w-full border border-gray-300 rounded-md px-3 py-2"
              placeholder="My Agent"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Type *
            </label>
            <select
              value={type}
              onChange={(e) => setType(e.target.value)}
              required
              className="w-full border border-gray-300 rounded-md px-3 py-2"
            >
              {Object.values(AgentTypes).map((t) => (
                <option key={t} value={t}>
                  {t}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Behavior
            </label>
            <select
              value={behaviorName}
              onChange={(e) => setBehaviorName(e.target.value)}
              className="w-full border border-gray-300 rounded-md px-3 py-2"
            >
              <option value="">None (Idle)</option>
              {behaviors.map((behavior) => (
                <option key={behavior.name} value={behavior.name}>
                  {behavior.name}
                </option>
              ))}
            </select>
            {selectedBehavior && (
              <p className="mt-1 text-sm text-gray-600">{selectedBehavior.description}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Behavior Configuration (JSON)
            </label>
            <textarea
              value={behaviorConfig}
              onChange={(e) => setBehaviorConfig(e.target.value)}
              rows={8}
              className="w-full border border-gray-300 rounded-md px-3 py-2 font-mono text-sm"
              placeholder={`{\n  "example": "value"\n}`}
            />
            <p className="mt-1 text-sm text-gray-600">
              Optional JSON configuration for the behavior
            </p>
          </div>

          {type === AgentTypes.BUILDER && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Builder ID
              </label>
              <input
                type="number"
                value={builderId || ''}
                onChange={(e) => setBuilderId(e.target.value ? parseInt(e.target.value) : undefined)}
                className="w-full border border-gray-300 rounded-md px-3 py-2"
                placeholder="Enter builder ID"
              />
              <p className="mt-1 text-sm text-gray-600">
                Optional: Link this agent to an existing builder
              </p>
            </div>
          )}

          <div className="flex gap-4">
            <button
              type="submit"
              disabled={loading}
              className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
            >
              {loading ? 'Creating...' : 'Create Agent'}
            </button>
            <Link
              href={`/games/${gameId}/agents`}
              className="px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700 text-center"
            >
              Cancel
            </Link>
          </div>
        </form>

        {/* Example Configurations */}
        <div className="mt-6 bg-white p-6 rounded-lg shadow-md">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Example Configurations</h2>
          
          <div className="space-y-4">
            <div>
              <h3 className="font-medium text-gray-700">AutoBuilder</h3>
              <pre className="mt-1 text-xs bg-gray-100 p-3 rounded overflow-x-auto">
{`{
  "planetIds": [1, 2, 3],
  "priorityBuildingTypes": ["IronMiner", "CopperMiner"],
  "maxConcurrentBuildings": 3
}`}
              </pre>
            </div>

            <div>
              <h3 className="font-medium text-gray-700">ResourceFerry</h3>
              <pre className="mt-1 text-xs bg-gray-100 p-3 rounded overflow-x-auto">
{`{
  "sourceSystemId": 1,
  "targetSystemId": 2,
  "resourceType": "Iron",
  "minAmount": 100,
  "maxAmount": 500,
  "ferryInterval": 300
}`}
              </pre>
            </div>

            <div>
              <h3 className="font-medium text-gray-700">ProductionMonitor</h3>
              <pre className="mt-1 text-xs bg-gray-100 p-3 rounded overflow-x-auto">
{`{
  "monitoredResources": ["Iron", "Copper"],
  "alertThresholds": {
    "Iron": 1000,
    "Copper": 500
  },
  "checkInterval": 60
}`}
              </pre>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
