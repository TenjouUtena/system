'use client';

import { useAuth } from '@/lib/hooks/useAuth';
import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import Link from 'next/link';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { LoadingPage } from '@/components/ui/LoadingSpinner';
import { Tooltip } from '@/components/ui/Tooltip';
import { gameApi } from '@/lib/api/game';
import type { Game } from '@/lib/types/game';

export default function EnhancedDashboardPage() {
  const { user, isLoading: authLoading, isAuthenticated, logout } = useAuth();
  const router = useRouter();
  const [games, setGames] = useState<Game[]>([]);
  const [isLoadingGames, setIsLoadingGames] = useState(true);

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
      setIsLoadingGames(true);
      const gamesData = await gameApi.getGames();
      setGames(gamesData);
    } catch (error) {
      console.error('Error loading games:', error);
    } finally {
      setIsLoadingGames(false);
    }
  };

  const handleLogout = async () => {
    await logout();
    router.push('/auth/login');
  };

  if (authLoading) {
    return <LoadingPage message="Loading dashboard..." />;
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100">
      {/* Navigation Bar */}
      <nav className="bg-white shadow-md border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center space-x-8">
              <Link href="/dashboard">
                <h1 className="text-2xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
                  System
                </h1>
              </Link>
              <div className="hidden md:flex space-x-4">
                <Tooltip content="View and manage your games">
                  <Link href="/games" className="text-gray-700 hover:text-indigo-600 px-3 py-2 rounded-md text-sm font-medium">
                    Games
                  </Link>
                </Tooltip>
                <Tooltip content="View your fleets">
                  <Link href="/fleets" className="text-gray-700 hover:text-indigo-600 px-3 py-2 rounded-md text-sm font-medium">
                    Fleets
                  </Link>
                </Tooltip>
                <Tooltip content="Game help and tutorials">
                  <Link href="/help" className="text-gray-700 hover:text-indigo-600 px-3 py-2 rounded-md text-sm font-medium">
                    Help
                  </Link>
                </Tooltip>
              </div>
            </div>
            <div className="flex items-center space-x-4">
              <Tooltip content="Your profile">
                <span className="text-gray-700 text-sm font-medium">
                  {user?.displayName}
                </span>
              </Tooltip>
              <Button variant="ghost" onClick={handleLogout} size="sm">
                Logout
              </Button>
            </div>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
        {/* Welcome Section */}
        <div className="mb-8">
          <h2 className="text-3xl font-bold text-gray-900 mb-2">
            Welcome back, {user?.displayName}!
          </h2>
          <p className="text-gray-600">
            Manage your galactic empire across multiple star systems
          </p>
        </div>

        {/* Quick Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <Card className="hover:shadow-lg transition-shadow">
            <CardContent>
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">Active Games</p>
                  <p className="text-3xl font-bold text-indigo-600">
                    {games.filter(g => g.isActive).length}
                  </p>
                </div>
                <div className="bg-indigo-100 rounded-full p-3">
                  <svg className="w-8 h-8 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064" />
                  </svg>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card className="hover:shadow-lg transition-shadow">
            <CardContent>
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">Total Systems</p>
                  <p className="text-3xl font-bold text-green-600">
                    {games.reduce((sum, g) => sum + (g.systemCount || 0), 0)}
                  </p>
                </div>
                <div className="bg-green-100 rounded-full p-3">
                  <svg className="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 21v-4m0 0V5a2 2 0 012-2h6.5l1 1H21l-3 6 3 6h-8.5l-1-1H5a2 2 0 00-2 2zm9-13.5V9" />
                  </svg>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card className="hover:shadow-lg transition-shadow">
            <CardContent>
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">Multiplayer Games</p>
                  <p className="text-3xl font-bold text-purple-600">
                    {games.filter(g => (g.playerCount || 0) > 1).length}
                  </p>
                </div>
                <div className="bg-purple-100 rounded-full p-3">
                  <svg className="w-8 h-8 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                  </svg>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Your Games */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          <div>
            <Card>
              <CardHeader 
                title="Your Games" 
                subtitle="Active and recent game sessions"
                action={
                  <Link href="/games">
                    <Button size="sm">View All</Button>
                  </Link>
                }
              />
              <CardContent>
                {isLoadingGames ? (
                  <div className="text-center py-8 text-gray-500">
                    Loading games...
                  </div>
                ) : games.length === 0 ? (
                  <div className="text-center py-8">
                    <p className="text-gray-500 mb-4">No games yet</p>
                    <Link href="/games">
                      <Button>Create Your First Game</Button>
                    </Link>
                  </div>
                ) : (
                  <div className="space-y-3">
                    {games.slice(0, 5).map(game => (
                      <Link key={game.id} href={`/games/${game.id}`}>
                        <div className="p-4 border border-gray-200 rounded-lg hover:border-indigo-300 hover:shadow-md transition-all cursor-pointer">
                          <div className="flex justify-between items-start mb-2">
                            <h4 className="font-semibold text-gray-900">{game.name}</h4>
                            {game.isActive && (
                              <span className="px-2 py-1 text-xs font-medium bg-green-100 text-green-800 rounded-full">
                                Active
                              </span>
                            )}
                          </div>
                          <div className="flex items-center gap-4 text-sm text-gray-600">
                            <span>🌟 {game.systemCount || 0} systems</span>
                            <span>👥 {game.playerCount || 1} players</span>
                          </div>
                        </div>
                      </Link>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
          </div>

          <div className="space-y-8">
            {/* Quick Actions */}
            <Card>
              <CardHeader title="Quick Actions" />
              <CardContent>
                <div className="grid grid-cols-2 gap-3">
                  <Link href="/games?action=create">
                    <Button variant="primary" fullWidth>
                      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                      </svg>
                      New Game
                    </Button>
                  </Link>
                  <Link href="/games?action=join">
                    <Button variant="secondary" fullWidth>
                      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                      </svg>
                      Join Game
                    </Button>
                  </Link>
                  <Link href="/help">
                    <Button variant="secondary" fullWidth>
                      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      Tutorial
                    </Button>
                  </Link>
                  <Link href="/settings">
                    <Button variant="secondary" fullWidth>
                      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      </svg>
                      Settings
                    </Button>
                  </Link>
                </div>
              </CardContent>
            </Card>

            {/* Game Info */}
            <Card>
              <CardHeader title="About System" />
              <CardContent>
                <div className="space-y-3 text-sm">
                  <div className="flex items-start gap-2">
                    <span className="text-indigo-600">✓</span>
                    <p className="text-gray-700">
                      <strong>Real-time Strategy:</strong> Your empire continues to grow even when you're offline
                    </p>
                  </div>
                  <div className="flex items-start gap-2">
                    <span className="text-indigo-600">✓</span>
                    <p className="text-gray-700">
                      <strong>4X Gameplay:</strong> Explore, Expand, Exploit, and Exterminate across the galaxy
                    </p>
                  </div>
                  <div className="flex items-start gap-2">
                    <span className="text-indigo-600">✓</span>
                    <p className="text-gray-700">
                      <strong>Multiplayer:</strong> Compete or cooperate with other players
                    </p>
                  </div>
                  <div className="mt-4 pt-4 border-t border-gray-200">
                    <Link href="/help" className="text-indigo-600 hover:text-indigo-700 font-medium">
                      Learn more →
                    </Link>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </main>
    </div>
  );
}
