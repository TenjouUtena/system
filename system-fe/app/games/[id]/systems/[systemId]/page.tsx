'use client';

import { useEffect, useState } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { useAuth } from '@/lib/hooks/useAuth';
import { gameApi } from '@/lib/api/game';
import type { SystemDetail } from '@/lib/types/game';

export default function SystemDetailPage() {
  const params = useParams();
  const gameId = parseInt(params.id as string);
  const systemId = parseInt(params.systemId as string);
  const [system, setSystem] = useState<SystemDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.push('/auth/login');
    }
  }, [isAuthenticated, authLoading, router]);

  useEffect(() => {
    if (isAuthenticated && gameId && systemId) {
      loadSystemData();
    }
  }, [isAuthenticated, gameId, systemId]);

  const loadSystemData = async () => {
    try {
      setIsLoading(true);
      const data = await gameApi.getSystemDetail(gameId, systemId);
      setSystem(data);
    } catch (err: any) {
      setError('Failed to load system data');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  if (authLoading || isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-gray-600">Loading...</p>
      </div>
    );
  }

  if (!system) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-red-600">{error || 'System not found'}</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <button
                onClick={() => router.push(`/games/${gameId}`)}
                className="mr-4 text-gray-600 hover:text-gray-900"
              >
                ← Back to Map
              </button>
              <h1 className="text-2xl font-bold text-gray-900">{system.name}</h1>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="bg-white rounded-lg shadow p-6 mb-6">
            <h2 className="text-xl font-bold text-gray-900 mb-4">System Information</h2>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <span className="text-sm text-gray-600">Coordinates:</span>
                <span className="ml-2 font-bold">({system.x}, {system.y})</span>
              </div>
              <div>
                <span className="text-sm text-gray-600">Planets:</span>
                <span className="ml-2 font-bold">{system.planets.length}</span>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-bold text-gray-900 mb-4">Planets</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {system.planets.map((planet) => (
                <div key={planet.id} className="border border-gray-300 rounded-lg p-4 hover:shadow-md transition-shadow">
                  <h3 className="text-lg font-bold text-gray-900 mb-2">{planet.name}</h3>
                  <div className="space-y-2 text-sm">
                    <div>
                      <span className="text-gray-600">Type:</span>
                      <span className="ml-2 font-medium">{planet.type}</span>
                    </div>
                    <div>
                      <span className="text-gray-600">Size:</span>
                      <span className="ml-2 font-medium">{planet.size}</span>
                    </div>
                    <div>
                      <span className="text-gray-600">Grid:</span>
                      <span className="ml-2 font-medium">{planet.gridWidth} × {planet.gridHeight}</span>
                    </div>
                    <button
                      onClick={() => router.push(`/games/${gameId}/systems/${systemId}/planets/${planet.id}`)}
                      className="mt-2 w-full px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-md hover:bg-indigo-700"
                    >
                      View Surface
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}

