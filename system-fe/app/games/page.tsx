'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/hooks/useAuth';
import { gameApi } from '@/lib/api/game';
import type { Game } from '@/lib/types/game';

export default function GamesPage() {
  const [games, setGames] = useState<Game[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.push('/auth/login');
    }
  }, [isAuthenticated, authLoading, router]);

  useEffect(() => {
    if (isAuthenticated) {
      loadGames();
    }
  }, [isAuthenticated]);

  const loadGames = async () => {
    try {
      setIsLoading(true);
      const data = await gameApi.getGames();
      setGames(data);
    } catch (err: any) {
      setError('Failed to load games');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateGame = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    const gameData = {
      name: formData.get('name') as string,
      description: (formData.get('description') as string) || '',
      systemCount: parseInt(formData.get('systemCount') as string) || 20,
      maxPlayers: parseInt(formData.get('maxPlayers') as string) || 50,
    };

    setIsCreating(true);
    try {
      const game = await gameApi.createGame(gameData);
      router.push(`/games/${game.id}`);
    } catch (err: any) {
      setError('Failed to create game');
      console.error(err);
    } finally {
      setIsCreating(false);
      setShowCreateModal(false);
    }
  };

  const handleJoinGame = async (gameId: number) => {
    try {
      await gameApi.joinGame(gameId);
      router.push(`/games/${gameId}`);
    } catch (err: any) {
      setError('Failed to join game');
      console.error(err);
    }
  };

  const handleDeleteGame = async (gameId: number) => {
    if (!confirm('Are you sure you want to delete this game? This action cannot be undone.')) {
      return;
    }

    try {
      await gameApi.deleteGame(gameId);
      loadGames(); // Refresh the list
    } catch (err: any) {
      setError('Failed to delete game');
      console.error(err);
    }
  };

  if (authLoading || isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-gray-600">Loading...</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <h1 className="text-2xl font-bold text-gray-900">System Game</h1>
            </div>
            <div className="flex items-center space-x-4">
              <button
                onClick={() => router.push('/dashboard')}
                className="px-4 py-2 text-sm font-medium text-gray-700 hover:text-gray-900"
              >
                Dashboard
              </button>
              <button
                onClick={() => setShowCreateModal(true)}
                className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-md hover:bg-indigo-700"
              >
                Create Game
              </button>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <h2 className="text-3xl font-bold text-gray-900 mb-6">Game Lobby</h2>

          {error && (
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
              {error}
            </div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {games.map((game) => (
              <div key={game.id} className="bg-white rounded-lg shadow p-6">
                <h3 className="text-xl font-bold text-gray-900 mb-2">{game.name}</h3>
                {game.description && (
                  <p className="text-gray-600 text-sm mb-4">{game.description}</p>
                )}
                <div className="flex items-center justify-between text-sm text-gray-600 mb-4">
                  <span>{game.playerCount} / {game.maxPlayers} players</span>
                  <span className="px-2 py-1 bg-green-100 text-green-800 rounded">
                    Active
                  </span>
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => router.push(`/games/${game.id}`)}
                    className="flex-1 px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-md hover:bg-indigo-700"
                  >
                    {game.isJoined ? 'View' : 'Join'}
                  </button>
                  {game.isCreator && (
                    <button
                      onClick={() => handleDeleteGame(game.id)}
                      className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-md hover:bg-red-700"
                    >
                      Delete
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>

          {games.length === 0 && !isLoading && (
            <div className="text-center py-12">
              <p className="text-gray-600 mb-4">No games available</p>
              <button
                onClick={() => setShowCreateModal(true)}
                className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-md hover:bg-indigo-700"
              >
                Create First Game
              </button>
            </div>
          )}
        </div>
      </main>

      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl p-8 max-w-md w-full">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">Create New Game</h2>
            <form onSubmit={handleCreateGame}>
              <div className="space-y-4">
                <div>
                  <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
                    Game Name
                  </label>
                  <input
                    id="name"
                    name="name"
                    type="text"
                    required
                    className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  />
                </div>
                <div>
                  <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
                    Description
                  </label>
                  <textarea
                    id="description"
                    name="description"
                    rows={3}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md"
                  />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label htmlFor="systemCount" className="block text-sm font-medium text-gray-700 mb-1">
                      Systems
                    </label>
                    <input
                      id="systemCount"
                      name="systemCount"
                      type="number"
                      min="10"
                      max="100"
                      defaultValue={20}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md"
                    />
                  </div>
                  <div>
                    <label htmlFor="maxPlayers" className="block text-sm font-medium text-gray-700 mb-1">
                      Max Players
                    </label>
                    <input
                      id="maxPlayers"
                      name="maxPlayers"
                      type="number"
                      min="2"
                      max="100"
                      defaultValue={50}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md"
                    />
                  </div>
                </div>
              </div>
              <div className="flex gap-2 mt-6">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="flex-1 px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200"
                  disabled={isCreating}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={isCreating}
                  className="flex-1 px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-md hover:bg-indigo-700 disabled:opacity-50"
                >
                  {isCreating ? 'Creating...' : 'Create Game'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

