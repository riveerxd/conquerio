import type { Direction, GameState, JoinedMessage, ServerMessage, StateMessage } from "./types";

export class NetworkClient {
  private ws: WebSocket | null = null;
  private grid = new Uint8Array(0);
  private gridWidth = 0;
  private gridHeight = 0;
  private tickRate = 20;
  private myPlayerId = "";
  private latestState: StateMessage | null = null;
  private prevState: StateMessage | null = null;
  private lastTickTime = 0;
  private onDeathCb: ((msg: { killedBy: string | null; reason: string }) => void) | null = null;
  private onJoinedCb: (() => void) | null = null;

  connect(token: string) {
    const proto = location.protocol === "https:" ? "wss:" : "ws:";
    this.ws = new WebSocket(`${proto}//${location.host}/ws/game?token=${token}`);

    this.ws.onmessage = (e) => {
      const msg: ServerMessage = JSON.parse(e.data);
      // console.log("ws msg", msg.type);
      this.handleMessage(msg);
    };

    this.ws.onclose = () => {
      this.ws = null;
    };
  }

  disconnect() {
    this.ws?.close();
    this.ws = null;
  }

  sendInput(dir: Direction) {
    this.ws?.send(JSON.stringify({ type: "input", dir }));
  }

  onDeath(cb: DeathCallback) {
    this.onDeathCb = cb;
  }

  onJoined(cb: () => void) {
    this.onJoinedCb = cb;
  }

  getState(): GameState | null {
    if (!this.latestState) return null;
    return {
      tick: this.latestState.tick,
      players: this.latestState.players,
      grid: this.grid,
      gridWidth: this.gridWidth,
      gridHeight: this.gridHeight,
      myPlayerId: this.myPlayerId,
    };
  }

  getPrevState(): StateMessage | null {
    return this.prevState;
  }

  getLastTickTime(): number {
    return this.lastTickTime;
  }

  getTickRate(): number {
    return this.tickRate;
  }

  private handleMessage(msg: ServerMessage) {
    switch (msg.type) {
      case "joined":
        this.handleJoined(msg);
        break;
      case "state":
        this.handleState(msg);
        break;
      case "death":
        this.onDeathCb?.(msg);
        break;
    }
  }

  private handleJoined(msg: JoinedMessage) {
    this.myPlayerId = msg.playerId;
    this.gridWidth = msg.gridWidth;
    this.gridHeight = msg.gridHeight;
    this.tickRate = msg.tickRate;
    this.grid = new Uint8Array(msg.grid);
    this.onJoinedCb?.();
  }

  private handleState(msg: StateMessage) {
    this.prevState = this.latestState;
    this.latestState = msg;
    this.lastTickTime = performance.now();

    for (const cell of msg.gridDiff) {
      this.grid[cell.y * this.gridWidth + cell.x] = cell.c;
    }
  }
}
