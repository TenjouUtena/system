'use client';

import { useEffect, useState } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { useAuth } from '@/lib/hooks/useAuth';
import { gameApi } from '@/lib/api/game';
import type { Game, GalaxyMap, SystemMap } from '@/lib/types/game';

export default function GameDetailPage() {
  const params = useParams();
  const gameId = parseInt(params.id as string);
  const [game, setGame] = useState<Game | null>(null);
  const [galaxyMap, setGalaxyMap] = useState<GalaxyMap | null>(null);
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
    if (isAuthenticated && gameId) {
      loadGameData();
    }
  }, [isAuthenticated, gameId]);

  const loadGameData = async () => {
    try {
      setIsLoading(true);
      const [gameData, galaxyData] = await Promise.all([
        gameApi.getGame(gameId),
        gameApi.getGalaxyMap(gameId),
      ]);
      setGame(gameData);
      setGalaxyMap(galaxyData);
    } catch (err: any) {
      setError('Failed to load game data');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  // Calculate bounds and scale for the map
  const getMapTransform = () => {
    if (!galaxyMap || galaxyMap.systems.length === 0) {
      return { scale: 1, offsetX: 0, offsetY: 0 };
    }

    const xs = galaxyMap.systems.map(s => s.x);
    const ys = galaxyMap.systems.map(s => s.y);
    const minX = Math.min(...xs);
    const maxX = Math.max(...xs);
    const minY = Math.min(...ys);
    const maxY = Math.max(...ys);

    const width = maxX - minX;
    const height = maxY - minY;
    const scale = Math.min(800 / width, 600 / height, 1.5);

    return {
      scale,
      offsetX: -minX,
      offsetY: -minY,
    };
  };

  const handleSystemClick = (system: SystemMap) => {
    router.push(`/games/${gameId}/systems/${system.id}`);
  };

  if (authLoading || isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-gray-600">Loading...</p>
      </div>
    );
  }

  if (!game || !galaxyMap) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-red-600">{error || 'Game not found'}</p>
      </div>
    );
  }

  const { scale, offsetX, offsetY } = getMapTransform();

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <button
                onClick={() => router.push('/games')}
                className="mr-4 text-gray-600 hover:text-gray-900"
              >
                ← Back
              </button>
              <h1 className="text-2xl font-bold text-gray-900">{game.name}</h1>
            </div>
            <div className="flex items-center space-x-4">
              <button
                onClick={() => router.push(`/games/${gameId}/agents`)}
                className="px-4 py-2 text-sm font-medium text-blue-600 hover:text-blue-800"
              >
                Agents
              </button>
              <button
                onClick={() => router.push('/games')}
                className="px-4 py-2 text-sm font-medium text-gray-700 hover:text-gray-900"
              >
                Game List
              </button>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="bg-white rounded-lg shadow p-6 mb-6">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">{galaxyMap.name}</h2>
            <div className="flex items-center justify-between">
              <div className="flex gap-6">
                <div>
                  <span className="text-sm text-gray-600">Systems:</span>
                  <span className="ml-2 font-bold">{galaxyMap.systems.length}</span>
                </div>
                <div>
                  <span className="text-sm text-gray-600">Players:</span>
                  <span className="ml-2 font-bold">{game.playerCount} / {game.maxPlayers}</span>
                </div>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-xl font-bold text-gray-900 mb-4">Galaxy Map</h3>
            <div className="overflow-auto border border-gray-300 rounded" style={{ height: '600px' }}>
              <svg
                width="100%"
                height="100%"
                viewBox="0 0 2000 1500"
                className="bg-gray-900"
              >
                {/* Draw wormholes */}
                {galaxyMap.wormholes.map((wh) => {
                  const systemA = galaxyMap.systems.find(s => s.id === wh.systemAId);
                  const systemB = galaxyMap.systems.find(s => s.id === wh.systemBId);
                  if (!systemA || !systemB) return null;

                  const x1 = (systemA.x + offsetX) * scale + 1000;
                  const y1 = (systemA.y + offsetY) * scale + 750;
                  const x2 = (systemB.x + offsetX) * scale + 1000;
                  const y2 = (systemB.y + offsetY) * scale + 750;

                  return (
                    <line
                      key={wh.id}
                      x1={x1}
                      y1={y1}
                      x2={x2}
                      y2={y2}
                      stroke={wh.isActive ? '#8B5CF6' : '#4B5563'}
                      strokeWidth="1"
                      opacity="0.3"
                    />
                  );
                })}

                {/* Draw systems */}
                {galaxyMap.systems.map((system) => {
                  const x = (system.x + offsetX) * scale + 1000;
                  const y = (system.y + offsetY) * scale + 750;

                  return (
                    <g key={system.id}>
                      <circle
                        cx={x}
                        cy={y}
                        r="8"
                        fill="#FBBF24"
                        stroke="#F59E0B"
                        strokeWidth="2"
                        className="cursor-pointer hover:fill-#FCD34D"
                        onClick={() => handleSystemClick(system)}
                      />
                      <text
                        x={x}
                        y={y - 15}
                        textAnchor="middle"
                        fill="#FFFFFF"
                        fontSize="12"
                        className="pointer-events-none"
                      >
                        {system.name}
                      </text>
                      <text
                        x={x}
                        y={y + 25}
                        textAnchor="middle"
                        fill="#9CA3AF"
                        fontSize="10"
                        className="pointer-events-none"
                      >
                        {system.planetCount} planets
                      </text>
                    </g>
                  );
                })}
              </svg>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}

