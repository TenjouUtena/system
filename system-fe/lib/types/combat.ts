// Combat-related TypeScript types for Phase 8

export interface Battle {
  id: number;
  gameId: number;
  systemId: number;
  systemName: string;
  state: 'InProgress' | 'Completed' | 'Fled';
  
  positionX: number;
  positionY: number;
  
  startTime: string;
  endTime?: string;
  roundsElapsed: number;
  
  winnerPlayerId?: string;
  winnerPlayerName?: string;
  endReason?: 'AllEnemiesDestroyed' | 'OneSideFled' | 'Timeout';
  
  participants: BattleParticipant[];
  events: BattleEvent[];
  
  createdAt: string;
}

export interface BattleParticipant {
  id: number;
  battleId: number;
  spaceshipId: number;
  spaceshipName: string;
  playerId: string;
  playerName: string;
  isNpc: boolean;
  
  initialHealth: number;
  finalHealth: number;
  attack: number;
  defense: number;
  
  damageDealt: number;
  damageTaken: number;
  survived: boolean;
  fled: boolean;
  
  experienceGained: number;
  lootIron?: number;
  lootCopper?: number;
  lootFuel?: number;
}

export interface BattleEvent {
  id: number;
  battleId: number;
  round: number;
  type: 'BattleStarted' | 'Attack' | 'ShipDestroyed' | 'ShipFled' | 'BattleEnded';
  
  attackerShipId?: number;
  attackerShipName?: string;
  defenderShipId?: number;
  defenderShipName?: string;
  damageDealt?: number;
  description: string;
  
  timestamp: string;
}

export interface BattleSummary {
  id: number;
  gameId: number;
  systemId: number;
  systemName: string;
  state: 'InProgress' | 'Completed' | 'Fled';
  
  startTime: string;
  endTime?: string;
  roundsElapsed: number;
  
  winnerPlayerName?: string;
  participantCount: number;
}

export interface NpcShip {
  id: number;
  spaceshipId: number;
  gameId: number;
  behaviorType: 'Patrol' | 'Ambush' | 'Aggressive' | 'Passive';
  difficultyLevel: number;
  
  patrolTargetX?: number;
  patrolTargetY?: number;
  targetShipId?: number;
  targetShipName?: string;
  
  spawnTime: string;
  spawnSystemId?: number;
  spawnSystemName?: string;
  
  lootIronMin: number;
  lootIronMax: number;
  lootCopperMin: number;
  lootCopperMax: number;
  lootFuelMin: number;
  lootFuelMax: number;
  
  spaceship?: any; // Reference to Spaceship type
}

export interface SpawnNpcRequest {
  systemId?: number;
  difficultyLevel?: number;
}
