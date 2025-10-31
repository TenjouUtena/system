'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { getGameShipyards } from '@/lib/api/spaceship';
import { Shipyard } from '@/lib/types/spaceship';

export default function ShipyardsPage() {
  const params = useParams();
  const router = useRouter();
  const gameId = parseInt(params.id as string);
  
  const [shipyards, setShipyards] = useState<Shipyard[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadShipyards();
  }, [gameId]);

  const loadShipyards = async () => {
    try {
      const data = await getGameShipyards(gameId);
      setShipyards(data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load shipyards');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 p-8">
        <div className="max-w-6xl mx-auto">
          <p className="text-gray-600">Loading shipyards...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-6xl mx-auto">
        {/* Header */}
        <div className="flex justify-between items-center mb-8">
          <div>
            <button
              onClick={() => router.push(`/games/${gameId}/ships`)}
              className="text-blue-600 hover:text-blue-800 mb-2"
            >
              ← Back to Fleet
            </button>
            <h1 className="text-3xl font-bold text-gray-900">Shipyards</h1>
            <p className="text-gray-600 mt-2">{shipyards.length} shipyards</p>
          </div>
          <button
            onClick={() => router.push(`/games/${gameId}/ships/shipyards/create`)}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Create Shipyard
          </button>
        </div>

        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-800">{error}</p>
          </div>
        )}

        {/* Shipyards List */}
        {shipyards.length === 0 ? (
          <div className="bg-white rounded-lg shadow p-12 text-center">
            <p className="text-gray-500 mb-4">No shipyards found</p>
            <button
              onClick={() => router.push(`/games/${gameId}/ships/shipyards/create`)}
              className="text-blue-600 hover:text-blue-800 font-medium"
            >
              Create your first shipyard →
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {shipyards.map((shipyard) => (
              <div key={shipyard.id} className="bg-white rounded-lg shadow p-6">
                {/* Shipyard Header */}
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="text-xl font-bold text-gray-900">{shipyard.name}</h3>
                    <p className="text-sm text-gray-600">{shipyard.spaceStationName}</p>
                  </div>
                  <span
                    className={`px-3 py-1 text-xs font-medium rounded-full ${
                      shipyard.isActive
                        ? 'bg-green-100 text-green-800'
                        : 'bg-gray-100 text-gray-800'
                    }`}
                  >
                    {shipyard.isActive ? 'Active' : 'Inactive'}
                  </span>
                </div>

                {/* Shipyard Stats */}
                <div className="space-y-3">
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-600">Build Capacity:</span>
                    <span className="font-medium">
                      {shipyard.currentBuildsCount} / {shipyard.maxConcurrentBuilds} busy
                    </span>
                  </div>
                  
                  {shipyard.currentBuildsCount > 0 && (
                    <div className="w-full bg-gray-200 rounded-full h-2">
                      <div
                        className="bg-blue-600 h-2 rounded-full"
                        style={{
                          width: `${(shipyard.currentBuildsCount / shipyard.maxConcurrentBuilds) * 100}%`
                        }}
                      />
                    </div>
                  )}

                  <div className="flex justify-between text-sm">
                    <span className="text-gray-600">Created:</span>
                    <span className="font-medium">
                      {new Date(shipyard.createdAt).toLocaleDateString()}
                    </span>
                  </div>
                </div>

                {/* Action Button */}
                <button
                  onClick={() => router.push(`/games/${gameId}/ships/create?shipyard=${shipyard.id}`)}
                  className="mt-4 w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400"
                  disabled={!shipyard.isActive || shipyard.currentBuildsCount >= shipyard.maxConcurrentBuilds}
                >
                  {shipyard.currentBuildsCount >= shipyard.maxConcurrentBuilds
                    ? 'At Capacity'
                    : 'Build Ship'}
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
