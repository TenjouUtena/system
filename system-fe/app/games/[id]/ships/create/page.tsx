'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { createSpaceship, getGameShipyards } from '@/lib/api/spaceship';
import { Shipyard, ShipTypes, getShipTypeDescription } from '@/lib/types/spaceship';

export default function CreateShipPage() {
  const params = useParams();
  const router = useRouter();
  const gameId = parseInt(params.id as string);
  
  const [shipyards, setShipyards] = useState<Shipyard[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const [formData, setFormData] = useState({
    name: '',
    type: 'Scout',
    shipyardId: 0
  });

  useEffect(() => {
    loadShipyards();
  }, [gameId]);

  const loadShipyards = async () => {
    try {
      const data = await getGameShipyards(gameId);
      setShipyards(data.filter(sy => sy.isActive));
      if (data.length > 0) {
        setFormData(prev => ({ ...prev, shipyardId: data[0].id }));
      }
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load shipyards');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.name.trim()) {
      setError('Please enter a ship name');
      return;
    }

    if (!formData.shipyardId) {
      setError('Please select a shipyard');
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      await createSpaceship({
        gameId,
        name: formData.name,
        type: formData.type,
        shipyardId: formData.shipyardId
      });
      
      router.push(`/games/${gameId}/ships`);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to create spaceship');
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
          onClick={() => router.push(`/games/${gameId}/ships`)}
          className="text-blue-600 hover:text-blue-800 mb-4"
        >
          ← Back to Fleet
        </button>
        
        <div className="bg-white rounded-lg shadow p-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-6">Build New Spaceship</h1>

          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-red-800">{error}</p>
            </div>
          )}

          {shipyards.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-gray-600 mb-4">No active shipyards available</p>
              <button
                onClick={() => router.push(`/games/${gameId}/ships/shipyards/create`)}
                className="text-blue-600 hover:text-blue-800 font-medium"
              >
                Create a shipyard first →
              </button>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Ship Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Ship Name *
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder="Enter ship name"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                />
              </div>

              {/* Ship Type */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Ship Type *
                </label>
                <div className="grid grid-cols-1 gap-3">
                  {Object.values(ShipTypes).map((type) => (
                    <label
                      key={type}
                      className={`flex items-center p-4 border-2 rounded-lg cursor-pointer transition ${
                        formData.type === type
                          ? 'border-blue-600 bg-blue-50'
                          : 'border-gray-200 hover:border-gray-300'
                      }`}
                    >
                      <input
                        type="radio"
                        name="type"
                        value={type}
                        checked={formData.type === type}
                        onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                        className="mr-3"
                      />
                      <div className="flex-1">
                        <div className="font-medium text-gray-900">{type}</div>
                        <div className="text-sm text-gray-600">{getShipTypeDescription(type)}</div>
                      </div>
                    </label>
                  ))}
                </div>
              </div>

              {/* Shipyard Selection */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Shipyard *
                </label>
                <select
                  value={formData.shipyardId}
                  onChange={(e) => setFormData({ ...formData, shipyardId: parseInt(e.target.value) })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                >
                  {shipyards.map((shipyard) => (
                    <option key={shipyard.id} value={shipyard.id}>
                      {shipyard.name} ({shipyard.spaceStationName}) - {shipyard.currentBuildsCount}/{shipyard.maxConcurrentBuilds} busy
                    </option>
                  ))}
                </select>
              </div>

              {/* Submit Button */}
              <div className="flex gap-3 pt-4">
                <button
                  type="button"
                  onClick={() => router.push(`/games/${gameId}/ships`)}
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
                  {submitting ? 'Building...' : 'Build Ship'}
                </button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}
