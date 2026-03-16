export type Direction = "up" | "down" | "left" | "right";

export interface Player {
  id: string;
  username: string;
  x: number;
  y: number;
  dir: Direction;
  trail: [number, number][];
  alive: boolean;
  colorId: number;
  speedMultiplier: number;
  abilities: Array<AbilityInfo>
}

export interface AbilityInfo {
  name: string;
  durationSecondsRemaining: number;
  cooldownSecondsRemaining: number;
}

export interface GridCell {
  x: number;
  y: number;
  c: number;
}

export interface GameState {
  tick: number;
  players: Player[];
  grid: Uint8Array;
  gridWidth: number;
  gridHeight: number;
  myPlayerId: string;
}

export interface JoinedMessage {
  type: "joined";
  playerId: string;
  colorId: number;
  gridWidth: number;
  gridHeight: number;
  tickRate: number;
  rleGrid: string; // base64 encoded byte array
}

export interface StateMessage {
  type: "state";
  tick: number;
  players: Player[];
  gridDiff: GridCell[];
}

export interface DeathMessage {
  type: "death";
  killedBy: string | null;
  reason: string;
}

export interface KillFeedMessage {
  type: "kill_feed";
  victimName: string;
  killerName: string | null;
  reason: string;
}

export type ServerMessage = JoinedMessage | StateMessage | DeathMessage | KillFeedMessage | { type: "pong"; t: number } | { type: "error"; msg: string };
