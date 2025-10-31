'use client';

import { useEffect, useState } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { useAuth } from '@/lib/hooks/useAuth';
import { planetApi } from '@/lib/api/planet';
import type { PlanetGrid, GridSquare } from '@/lib/types/planet';

export default function PlanetSurfacePage() {
  const params = useParams();
  const gameId = parseInt(params.id as string);
  const systemId = parseInt(params.systemId as string);
  const planetId = parseInt(params.planetId as string);
  
  const [grid, setGrid] = useState<PlanetGrid | null>(null);
  const [selectedSquare, setSelectedSquare] = useState<GridSquare | null>(null);
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
    if (isAuthenticated && planetId) {
      loadPlanetGrid();
    }
  }, [isAuthenticated, planetId]);

  const loadPlanetGrid = async () => {
    try {
      setIsLoading(true);
      const data = await planetApi.getPlanetGrid(planetId);
      setGrid(data);
    } catch (err: any) {
      setError('Failed to load planet grid');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  const getSquareColor = (square: GridSquare): string => {
    if (square.ironAmount) return 'bg-orange-300';
    if (square.copperAmount) return 'bg-amber-300';
    if (square.fuelAmount) return 'bg-blue-300';
    if (square.soilAmount) return 'bg-green-300';
    return 'bg-gray-200';
  };

  const getSquareHint = (square: GridSquare): string => {
    const resources: string[] = [];
    if (square.ironAmount) resources.push(`Iron: ${square.ironAmount.toFixed(1)}`);
    if (square.copperAmount) resources.push(`Copper: ${square.copperAmount.toFixed(1)}`);
    if (square.fuelAmount) resources.push(`Fuel: ${square.fuelAmount.toFixed(1)}`);
    if (square.soilAmount) resources.push(`Soil: ${square.soilAmount.toFixed(1)}`);
    return resources.join(', ') || 'No resources';
  };

  if (authLoading || isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-gray-600">Loading planet surface...</p>
      </div>
    );
  }

  if (!grid) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-red-600">{error || 'Planet grid not found'}</p>
      </div>
    );
  }

  // Organize squares into rows for rendering
  const rows: GridSquare[][] = [];
  for (let y = 0; y < grid.height; y++) {
    rows.push(grid.squares.filter(s => s.y === y).sort((a, b) => a.x - b.x));
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <button
                onClick={() => router.push(`/games/${gameId}/systems/${systemId}`)}
                className="mr-4 text-gray-600 hover:text-gray-900"
              >
                ← Back to System
              </button>
              <h1 className="text-2xl font-bold text-gray-900">Planet Surface</h1>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          {/* Legend */}
          <div className="bg-white rounded-lg shadow p-4 mb-6">
            <h3 className="text-lg font-bold text-gray-900 mb-3">Resource Legend</h3>
            <div className="grid grid-cols-4 gap-4">
              <div className="flex items-center">
                <div className="w-8 h-8 bg-orange-300 border border-orange-500 mr-2"></div>
                <span className="text-sm">Iron</span>
              </div>
              <div className="flex items-center">
                <div className="w-8 h-8 bg-amber-300 border border-amber-500 mr-2"></div>
                <span className="text-sm">Copper</span>
              </div>
              <div className="flex items-center">
                <div className="w-8 h-8 bg-blue-300 border border-blue-500 mr-2"></div>
                <span className="text-sm">Fuel</span>
              </div>
              <div className="flex items-center">
                <div className="w-8 h-8 bg-green-300 border border-green-500 mr-2"></div>
                <span className="text-sm">Soil</span>
              </div>
            </div>
          </div>

          {/* Grid */}
          <div className="bg-white rounded-lg shadow p-6 overflow-auto" style={{ maxHeight: '600px' }}>
            <div className="inline-block">
              {rows.map((row, y) => (
                <div key={y} className="flex">
                  {row.map((square) => (
                    <div
                      key={`${square.x}-${square.y}`}
                      className={`w-8 h-8 border border-gray-400 cursor-pointer hover:opacity-80 ${getSquareColor(square)}`}
                      title={`(${square.x}, ${square.y}) - ${getSquareHint(square)}`}
                      onClick={() => setSelectedSquare(square)}
                    />
                  ))}
                </div>
              ))}
            </div>
          </div>

          {/* Selected Square Details */}
          {selectedSquare && (
            <div className="bg-white rounded-lg shadow p-6 mt-6">
              <h3 className="text-lg font-bold text-gray-900 mb-4">
                Grid Square ({selectedSquare.x}, {selectedSquare.y})
              </h3>
              <div className="grid grid-cols-2 gap-4">
                {selectedSquare.ironAmount && (
                  <div>
                    <span className="text-gray-600">Iron:</span>
                    <span className="ml-2 font-bold">{selectedSquare.ironAmount.toFixed(2)}</span>
                  </div>
                )}
                {selectedSquare.copperAmount && (
                  <div>
                    <span className="text-gray-600">Copper:</span>
                    <span className="ml-2 font-bold">{selectedSquare.copperAmount.toFixed(2)}</span>
                  </div>
                )}
                {selectedSquare.fuelAmount && (
                  <div>
                    <span className="text-gray-600">Fuel:</span>
                    <span className="ml-2 font-bold">{selectedSquare.fuelAmount.toFixed(2)}</span>
                  </div>
                )}
                {selectedSquare.soilAmount && (
                  <div>
                    <span className="text-gray-600">Soil:</span>
                    <span className="ml-2 font-bold">{selectedSquare.soilAmount.toFixed(2)}</span>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      </main>
    </div>
  );
}

