'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { createShipyard } from '@/lib/api/spaceship';
import { getPlayerSpaceStations, SpaceStation } from '@/lib/api/spaceStation';

export default function CreateShipyardPage() {
  const params = useParams();
  const router = useRouter();
  const gameId = parseInt(params.id as string);
  
  const [spaceStations, setSpaceStations] = useState<SpaceStation[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const [formData, setFormData] = useState({
    name: '',
    spaceStationId: 0,
    maxConcurrentBuilds: 1
  });

  useEffect(() => {
    loadData();
  }, [gameId]);

  const loadData = async () => {
    try {
      const stations = await getPlayerSpaceStations(gameId);
      setSpaceStations(stations);
      
      // Auto-select first station if available
      if (stations.length > 0 && formData.spaceStationId === 0) {
        setFormData(prev => ({ ...prev, spaceStationId: stations[0].id }));
      }
      
      setError(null);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load space stations');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.name.trim()) {
      setError('Please enter a shipyard name');
      return;
    }

    if (!formData.spaceStationId) {
      setError('Please select a space station');
      return;
    }

    if (formData.maxConcurrentBuilds < 1) {
      setError('Maximum concurrent builds must be at least 1');
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      await createShipyard({
        gameId,
        name: formData.name,
        spaceStationId: formData.spaceStationId,
        maxConcurrentBuilds: formData.maxConcurrentBuilds
      });
      
      router.push(`/games/${gameId}/ships/shipyards`);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to create shipyard');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 p-8">
        <div className="max-w-2xl mx-auto">
          <p className="text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-2xl mx-auto">
        {/* Header */}
        <button
          onClick={() => router.push(`/games/${gameId}/ships/shipyards`)}
          className="text-blue-600 hover:text-blue-800 mb-4"
        >
          ← Back to Shipyards
        </button>
        
        <div className="bg-white rounded-lg shadow p-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-6">Create Shipyard</h1>

          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-red-800">{error}</p>
            </div>
          )}

          {/* Info Box */}
          <div className="mb-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
            <h3 className="font-semibold text-blue-900 mb-2">How to Create a Shipyard</h3>
            <p className="text-sm text-blue-800 mb-2">
              Shipyards are built at your space stations. To create a shipyard:
            </p>
            <ol className="list-decimal list-inside text-sm text-blue-800 space-y-1">
              <li>You must have a space station in a star system</li>
              <li>Space stations are created when you colonize a planet</li>
              <li>Build a colony ship and colonize a planet to get a space station</li>
            </ol>
          </div>

          {spaceStations.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-gray-600 mb-4">
                You don't have any space stations yet.
              </p>
              <p className="text-sm text-gray-500 mb-6">
                To create a shipyard, you first need to establish a space station by colonizing a planet.
              </p>
              <div className="space-y-3">
                <button
                  onClick={() => router.push(`/games/${gameId}`)}
                  className="block w-full px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >
                  Go to Galaxy Map
                </button>
                <button
                  onClick={() => router.push(`/games/${gameId}/ships/shipyards`)}
                  className="block w-full px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
                >
                  Back to Shipyards
                </button>
              </div>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Shipyard Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Shipyard Name *
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder="e.g., Alpha Shipyard, Main Construction Facility"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                />
              </div>

              {/* Space Station Selection */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Space Station *
                </label>
                <select
                  value={formData.spaceStationId}
                  onChange={(e) => setFormData({ ...formData, spaceStationId: parseInt(e.target.value) })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                >
                  <option value={0}>Select a space station</option>
                  {spaceStations.map((station) => (
                    <option key={station.id} value={station.id}>
                      {station.name} ({station.systemName})
                    </option>
                  ))}
                </select>
                <p className="mt-1 text-sm text-gray-500">
                  Choose which space station will host this shipyard
                </p>
              </div>

              {/* Max Concurrent Builds */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Maximum Concurrent Builds *
                </label>
                <input
                  type="number"
                  min={1}
                  max={10}
                  value={formData.maxConcurrentBuilds}
                  onChange={(e) => setFormData({ ...formData, maxConcurrentBuilds: parseInt(e.target.value) })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                />
                <p className="mt-1 text-sm text-gray-500">
                  How many ships can be built simultaneously at this shipyard (1-10)
                </p>
              </div>

              {/* Info about build capacity */}
              <div className="p-4 bg-gray-50 rounded-lg">
                <h4 className="font-medium text-gray-900 mb-2">Build Capacity</h4>
                <p className="text-sm text-gray-600">
                  A shipyard with capacity {formData.maxConcurrentBuilds} can build up to{' '}
                  {formData.maxConcurrentBuilds} ship{formData.maxConcurrentBuilds !== 1 ? 's' : ''} at the same time.
                  Larger capacity allows for faster fleet expansion but requires more resources.
                </p>
              </div>

              {/* Submit Buttons */}
              <div className="flex gap-3 pt-4">
                <button
                  type="button"
                  onClick={() => router.push(`/games/${gameId}/ships/shipyards`)}
                  className="flex-1 px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
                  disabled={submitting}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400"
                  disabled={submitting}
                >
                  {submitting ? 'Creating...' : 'Create Shipyard'}
                </button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}
