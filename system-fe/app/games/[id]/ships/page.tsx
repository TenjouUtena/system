'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { getGameSpaceships, getGameShipyards } from '@/lib/api/spaceship';
import { Spaceship, Shipyard, getShipStateColor, getShipTypeColor } from '@/lib/types/spaceship';

export default function ShipsPage() {
  const params = useParams();
  const router = useRouter();
  const gameId = parseInt(params.id as string);
  
  const [spaceships, setSpaceships] = useState<Spaceship[]>([]);
  const [shipyards, setShipyards] = useState<Shipyard[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<string>('all');

  useEffect(() => {
    loadData();
    
    // Refresh every 5 seconds
    const interval = setInterval(loadData, 5000);
    return () => clearInterval(interval);
  }, [gameId]);

  const loadData = async () => {
    try {
      const [shipsData, shipyardsData] = await Promise.all([
        getGameSpaceships(gameId),
        getGameShipyards(gameId)
      ]);
      setSpaceships(shipsData);
      setShipyards(shipyardsData);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load spaceships');
    } finally {
      setLoading(false);
    }
  };

  const filteredShips = filter === 'all' 
    ? spaceships 
    : spaceships.filter(s => s.state === filter);

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 p-8">
        <div className="max-w-7xl mx-auto">
          <p className="text-gray-600">Loading spaceships...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex justify-between items-center mb-8">
          <div>
            <button
              onClick={() => router.push(`/games/${gameId}`)}
              className="text-blue-600 hover:text-blue-800 mb-2"
            >
              ← Back to Game
            </button>
            <h1 className="text-3xl font-bold text-gray-900">Fleet Management</h1>
            <p className="text-gray-600 mt-2">
              {spaceships.length} ships • {shipyards.length} shipyards
            </p>
          </div>
          <div className="flex gap-3">
            <button
              onClick={() => router.push(`/games/${gameId}/ships/shipyards`)}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              Manage Shipyards
            </button>
            <button
              onClick={() => router.push(`/games/${gameId}/ships/create`)}
              className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
            >
              Build New Ship
            </button>
          </div>
        </div>

        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-800">{error}</p>
          </div>
        )}

        {/* Filters */}
        <div className="mb-6 flex gap-2">
          <button
            onClick={() => setFilter('all')}
            className={`px-4 py-2 rounded-lg ${
              filter === 'all' ? 'bg-blue-600 text-white' : 'bg-white text-gray-700 hover:bg-gray-50'
            }`}
          >
            All Ships ({spaceships.length})
          </button>
          <button
            onClick={() => setFilter('Idle')}
            className={`px-4 py-2 rounded-lg ${
              filter === 'Idle' ? 'bg-blue-600 text-white' : 'bg-white text-gray-700 hover:bg-gray-50'
            }`}
          >
            Idle ({spaceships.filter(s => s.state === 'Idle').length})
          </button>
          <button
            onClick={() => setFilter('Moving')}
            className={`px-4 py-2 rounded-lg ${
              filter === 'Moving' ? 'bg-blue-600 text-white' : 'bg-white text-gray-700 hover:bg-gray-50'
            }`}
          >
            Moving ({spaceships.filter(s => s.state === 'Moving').length})
          </button>
          <button
            onClick={() => setFilter('UnderConstruction')}
            className={`px-4 py-2 rounded-lg ${
              filter === 'UnderConstruction' ? 'bg-blue-600 text-white' : 'bg-white text-gray-700 hover:bg-gray-50'
            }`}
          >
            Under Construction ({spaceships.filter(s => s.state === 'UnderConstruction').length})
          </button>
        </div>

        {/* Ships Grid */}
        {filteredShips.length === 0 ? (
          <div className="bg-white rounded-lg shadow p-12 text-center">
            <p className="text-gray-500 mb-4">No spaceships found</p>
            <button
              onClick={() => router.push(`/games/${gameId}/ships/create`)}
              className="text-blue-600 hover:text-blue-800 font-medium"
            >
              Build your first ship →
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredShips.map((ship) => (
              <div
                key={ship.id}
                onClick={() => router.push(`/games/${gameId}/ships/${ship.id}`)}
                className="bg-white rounded-lg shadow hover:shadow-lg transition cursor-pointer"
              >
                <div className="p-6">
                  {/* Ship Header */}
                  <div className="flex justify-between items-start mb-4">
                    <div>
                      <h3 className="text-xl font-bold text-gray-900">{ship.name}</h3>
                      <p className={`text-sm font-medium ${getShipTypeColor(ship.type)}`}>
                        {ship.type}
                      </p>
                    </div>
                    <span className={`px-3 py-1 text-xs font-medium rounded-full ${getShipStateColor(ship.state)}`}>
                      {ship.state}
                    </span>
                  </div>

                  {/* Ship Stats */}
                  <div className="space-y-2 mb-4">
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">Location:</span>
                      <span className="font-medium">{ship.currentSystemName}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">Health:</span>
                      <span className="font-medium">{ship.health}/{ship.maxHealth}</span>
                    </div>
                    {ship.cargoCapacity > 0 && (
                      <div className="flex justify-between text-sm">
                        <span className="text-gray-600">Cargo:</span>
                        <span className="font-medium">
                          {(ship.cargoIron || 0) + (ship.cargoCopper || 0) + (ship.cargoFuel || 0) + (ship.cargoSoil || 0)}/{ship.cargoCapacity}
                        </span>
                      </div>
                    )}
                  </div>

                  {/* Construction Progress */}
                  {ship.state === 'UnderConstruction' && (
                    <div className="mb-4">
                      <div className="flex justify-between text-sm mb-1">
                        <span className="text-gray-600">Construction:</span>
                        <span className="font-medium">{Math.floor(ship.constructionProgress)}%</span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-2">
                        <div
                          className="bg-blue-600 h-2 rounded-full transition-all"
                          style={{ width: `${ship.constructionProgress}%` }}
                        />
                      </div>
                    </div>
                  )}

                  {/* Movement Info */}
                  {ship.state === 'Moving' && ship.estimatedArrivalTime && (
                    <div className="text-sm text-gray-600">
                      <p>Destination: {ship.destinationSystemName || 'In-system'}</p>
                      <p>ETA: {new Date(ship.estimatedArrivalTime).toLocaleTimeString()}</p>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
