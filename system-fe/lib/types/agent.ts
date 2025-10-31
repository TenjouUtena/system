export interface Agent {
  id: number;
  playerId: string;
  gameId: number;
  type: string;
  name: string;
  state: string;
  currentBehaviorName?: string;
  behaviorConfig?: string;
  lastExecutionTime: string;
  createdAt: string;
  currentSystemId?: number;
  currentSystemName?: string;
  currentPlanetId?: number;
  currentPlanetName?: string;
  builderId?: number;
  builderName?: string;
}

export interface CreateAgentRequest {
  gameId: number;
  type: string;
  name: string;
  behaviorName?: string;
  behaviorConfig?: string;
  builderId?: number;
}

export interface UpdateAgentRequest {
  name?: string;
  behaviorName?: string;
  behaviorConfig?: string;
}

export interface AgentLog {
  id: number;
  timestamp: string;
  level: string;
  message: string;
  data?: string;
}

export interface BehaviorInfo {
  name: string;
  description: string;
  supportedAgentTypes: string[];
  configSchema?: string;
}

export const AgentTypes = {
  BUILDER: 'Builder',
  RESOURCE_FERRY: 'ResourceFerry',
  SCOUT: 'Scout',
  FLEET: 'Fleet',
  CUSTOM: 'Custom',
} as const;

export const AgentStates = {
  IDLE: 'Idle',
  ACTIVE: 'Active',
  PAUSED: 'Paused',
  ERROR: 'Error',
  COMPLETED: 'Completed',
} as const;
