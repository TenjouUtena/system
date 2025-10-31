'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { getSpaceship } from '@/lib/api/spaceship';
import { Spaceship, getShipStateColor, getShipTypeColor } from '@/lib/types/spaceship';

export default function ShipDetailPage() {
  const params = useParams();
  const router = useRouter();
  const gameId = parseInt(params.id as string);
  const shipId = parseInt(params.shipId as string);
  
  const [ship, setShip] = useState<Spaceship | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadShip();
    
    // Refresh every 5 seconds
    const interval = setInterval(loadShip, 5000);
    return () => clearInterval(interval);
  }, [shipId]);

  const loadShip = async () => {
    try {
      const data = await getSpaceship(shipId);
      setShip(data);
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load spaceship');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 p-8">
        <div className="max-w-4xl mx-auto">
          <p className="text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  if (error || !ship) {
    return (
      <div className="min-h-screen bg-gray-50 p-8">
        <div className="max-w-4xl mx-auto">
          <div className="bg-red-50 border border-red-200 rounded-lg p-6">
            <p className="text-red-800">{error || 'Ship not found'}</p>
            <button
              onClick={() => router.push(`/games/${gameId}/ships`)}
              className="mt-4 text-blue-600 hover:text-blue-800"
            >
              ← Back to Fleet
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <button
          onClick={() => router.push(`/games/${gameId}/ships`)}
          className="text-blue-600 hover:text-blue-800 mb-4"
        >
          ← Back to Fleet
        </button>

        <div className="bg-white rounded-lg shadow mb-6">
          <div className="p-8">
            {/* Ship Title */}
            <div className="flex justify-between items-start mb-6">
              <div>
                <h1 className="text-3xl font-bold text-gray-900 mb-2">{ship.name}</h1>
                <p className={`text-lg font-medium ${getShipTypeColor(ship.type)}`}>
                  {ship.type} Class Vessel
                </p>
              </div>
              <span className={`px-4 py-2 text-sm font-medium rounded-full ${getShipStateColor(ship.state)}`}>
                {ship.state}
              </span>
            </div>

            {/* Construction Progress */}
            {ship.state === 'UnderConstruction' && (
              <div className="mb-8 p-6 bg-yellow-50 rounded-lg">
                <h3 className="text-lg font-semibold text-gray-900 mb-3">Under Construction</h3>
                <div className="flex justify-between text-sm mb-2">
                  <span className="text-gray-600">Progress:</span>
                  <span className="font-medium">{Math.floor(ship.constructionProgress)}%</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-3">
                  <div
                    className="bg-blue-600 h-3 rounded-full transition-all"
                    style={{ width: `${ship.constructionProgress}%` }}
                  />
                </div>
                {ship.constructionStartTime && (
                  <p className="text-sm text-gray-600 mt-2">
                    Started: {new Date(ship.constructionStartTime).toLocaleString()}
                  </p>
                )}
              </div>
            )}

            {/* Ship Stats Grid */}
            <div className="grid grid-cols-2 gap-6 mb-8">
              {/* Location */}
              <div className="p-4 bg-gray-50 rounded-lg">
                <h3 className="text-sm font-medium text-gray-600 mb-2">Location</h3>
                <p className="text-lg font-semibold">{ship.currentSystemName}</p>
                <p className="text-sm text-gray-500">
                  Position: ({ship.positionX.toFixed(1)}, {ship.positionY.toFixed(1)})
                </p>
              </div>

              {/* Health */}
              <div className="p-4 bg-gray-50 rounded-lg">
                <h3 className="text-sm font-medium text-gray-600 mb-2">Health</h3>
                <div className="flex items-baseline gap-2">
                  <span className="text-2xl font-bold">{ship.health}</span>
                  <span className="text-gray-500">/ {ship.maxHealth}</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2 mt-2">
                  <div
                    className="bg-green-600 h-2 rounded-full"
                    style={{ width: `${(ship.health / ship.maxHealth) * 100}%` }}
                  />
                </div>
              </div>

              {/* Combat Stats */}
              <div className="p-4 bg-gray-50 rounded-lg">
                <h3 className="text-sm font-medium text-gray-600 mb-2">Combat</h3>
                <div className="space-y-1">
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-600">Attack:</span>
                    <span className="font-medium">{ship.attack}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-600">Defense:</span>
                    <span className="font-medium">{ship.defense}</span>
                  </div>
                </div>
              </div>

              {/* Speed */}
              <div className="p-4 bg-gray-50 rounded-lg">
                <h3 className="text-sm font-medium text-gray-600 mb-2">Speed</h3>
                <p className="text-2xl font-bold">{ship.speed}</p>
                <p className="text-sm text-gray-500">units per second</p>
              </div>
            </div>

            {/* Cargo */}
            {ship.cargoCapacity > 0 && (
              <div className="mb-8 p-6 bg-gray-50 rounded-lg">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Cargo Hold</h3>
                <div className="grid grid-cols-4 gap-4 mb-3">
                  <div>
                    <p className="text-sm text-gray-600">Iron</p>
                    <p className="text-xl font-bold">{ship.cargoIron || 0}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">Copper</p>
                    <p className="text-xl font-bold">{ship.cargoCopper || 0}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">Fuel</p>
                    <p className="text-xl font-bold">{ship.cargoFuel || 0}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">Soil</p>
                    <p className="text-xl font-bold">{ship.cargoSoil || 0}</p>
                  </div>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Capacity:</span>
                  <span className="font-medium">
                    {(ship.cargoIron || 0) + (ship.cargoCopper || 0) + (ship.cargoFuel || 0) + (ship.cargoSoil || 0)} / {ship.cargoCapacity}
                  </span>
                </div>
              </div>
            )}

            {/* Movement Info */}
            {ship.state === 'Moving' && (
              <div className="mb-8 p-6 bg-blue-50 rounded-lg">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">In Transit</h3>
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span className="text-gray-600">Destination:</span>
                    <span className="font-medium">{ship.destinationSystemName || 'In-system'}</span>
                  </div>
                  {ship.destinationX && ship.destinationY && (
                    <div className="flex justify-between">
                      <span className="text-gray-600">Target Position:</span>
                      <span className="font-medium">
                        ({ship.destinationX.toFixed(1)}, {ship.destinationY.toFixed(1)})
                      </span>
                    </div>
                  )}
                  {ship.estimatedArrivalTime && (
                    <div className="flex justify-between">
                      <span className="text-gray-600">ETA:</span>
                      <span className="font-medium">{new Date(ship.estimatedArrivalTime).toLocaleString()}</span>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Actions */}
            {ship.state === 'Idle' && (
              <div className="flex gap-3 pt-4 border-t">
                <button
                  onClick={() => alert('Move functionality coming soon!')}
                  className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >
                  Move Ship
                </button>
                {ship.type === 'Colony' && (
                  <button
                    onClick={() => alert('Colonize functionality coming soon!')}
                    className="flex-1 px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700"
                  >
                    Colonize Planet
                  </button>
                )}
              </div>
            )}

            {/* Metadata */}
            <div className="mt-8 pt-6 border-t text-sm text-gray-500">
              <p>Created: {new Date(ship.createdAt).toLocaleString()}</p>
              <p>Last Updated: {new Date(ship.lastUpdatedAt).toLocaleString()}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
