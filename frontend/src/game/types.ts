export type Direction = "up" | "down" | "left" | "right";

export interface Player {
  id: string;
  x: number;
  y: number;
  dir: Direction;
  trail: [number, number][];
  alive: boolean;
  colorId: number;
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
  grid: number[];
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

export type ServerMessage = JoinedMessage | StateMessage | DeathMessage | { type: "pong"; t: number } | { type: "error"; msg: string };
