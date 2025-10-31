// Combat API client for Phase 8
import axios from 'axios';
import type { Battle, BattleSummary, NpcShip, SpawnNpcRequest } from '../types/combat';

/**
 * Get all battles for a game
 */
export async function getBattlesByGame(gameId: number): Promise<BattleSummary[]> {
  const response = await axios.get(`/api/combat/battles/game/${gameId}`);
  return response.data;
}

/**
 * Get detailed battle information
 */
export async function getBattleById(battleId: number): Promise<Battle> {
  const response = await axios.get(`/api/combat/battles/${battleId}`);
  return response.data;
}

/**
 * Get active battles in a system
 */
export async function getActiveBattlesBySystem(systemId: number): Promise<BattleSummary[]> {
  const response = await axios.get(`/api/combat/battles/system/${systemId}/active`);
  return response.data;
}

/**
 * Get all NPC ships in a game
 */
export async function getNpcShipsByGame(gameId: number): Promise<NpcShip[]> {
  const response = await axios.get(`/api/combat/npcs/game/${gameId}`);
  return response.data;
}

/**
 * Spawn a new NPC ship (admin/testing endpoint)
 */
export async function spawnNpc(gameId: number, request: SpawnNpcRequest = {}): Promise<NpcShip> {
  const response = await axios.post(`/api/combat/npcs/spawn?gameId=${gameId}`, request);
  return response.data;
}

/**
 * Get recent battles for display in a feed
 */
export async function getRecentBattles(gameId: number, limit: number = 10): Promise<BattleSummary[]> {
  const battles = await getBattlesByGame(gameId);
  return battles.slice(0, limit);
}

/**
 * Check if ship is currently in battle
 */
export async function isShipInBattle(gameId: number, spaceshipId: number): Promise<boolean> {
  const battles = await getBattlesByGame(gameId);
  const activeBattles = battles.filter(b => b.state === 'InProgress');
  
  // Note: This requires fetching battle details to check participants
  // For better performance, consider adding a dedicated endpoint
  for (const battleSummary of activeBattles) {
    const battle = await getBattleById(battleSummary.id);
    if (battle.participants.some(p => p.spaceshipId === spaceshipId)) {
      return true;
    }
  }
  
  return false;
}
