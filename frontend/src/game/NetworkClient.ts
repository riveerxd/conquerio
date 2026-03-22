import type { Direction, GameState, JoinedMessage, KillFeedMessage, ServerMessage, StateMessage, WinMessage } from "./types";

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
  private onKillFeedCb: ((msg: KillFeedMessage) => void) | null = null;
  private onJoinedCb: (() => void) | null = null;
  private onDisconnectCb: (() => void) | null = null;
  private onStateUpdateCb: ((state: import("./types").GameState) => void) | null = null;
  private onWinCb: ((msg: WinMessage) => void) | null = null;

  connect(token: string, roomId?: string, joinCode?: string) {
    const proto = location.protocol === "https:" ? "wss:" : "ws:";
    let url = `${proto}//${location.host}/ws/game?token=${token}`;
    if (roomId) url += `&roomId=${roomId}`;
    // NOTE: joinCode is passed as a query param — acceptable for a game
    if (joinCode) url += `&joinCode=${encodeURIComponent(joinCode)}`;
    this.ws = new WebSocket(url);

    this.ws.onmessage = (e) => {
      const msg: ServerMessage = JSON.parse(e.data);
      // console.log("ws msg", msg.type);
      this.handleMessage(msg);
    };

    this.ws.onclose = () => {
      this.ws = null;
      this.onDisconnectCb?.();
    };
  }

  disconnect() {
    this.ws?.close();
    this.ws = null;
  }

  sendInput(dir: Direction) {
    this.ws?.send(JSON.stringify({ type: "input", dir }));
  }

  sendAbility(ability: string) {
    this.ws?.send(JSON.stringify({ type: "ability", ability }));
  }

  onDeath(cb: (msg: { killedBy: string | null; reason: string }) => void) {
    this.onDeathCb = cb;
  }

  onKillFeed(cb: (msg: KillFeedMessage) => void) {
    this.onKillFeedCb = cb;
  }

  onJoined(cb: () => void) {
    this.onJoinedCb = cb;
  }

  onDisconnect(cb: () => void) {
    this.onDisconnectCb = cb;
  }

  onWin(cb: (msg: WinMessage) => void) {
    this.onWinCb = cb;
  }

  onStateUpdate(cb: (state: import("./types").GameState) => void) {
    this.onStateUpdateCb = cb;
  }

  offStateUpdate() {
    this.onStateUpdateCb = null;
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
      case "kill_feed":
        this.onKillFeedCb?.(msg);
        break;
      case "win":
        this.onWinCb?.(msg);
        break;
    }
  }

  private handleJoined(msg: JoinedMessage) {
    this.myPlayerId = msg.playerId;
    this.gridWidth = msg.gridWidth;
    this.gridHeight = msg.gridHeight;
    this.tickRate = msg.tickRate;

    // Decode base64 RLE grid
    const binaryStr = atob(msg.rleGrid);
    const rleBytes = new Uint8Array(binaryStr.length);
    for (let i = 0; i < binaryStr.length; i++) {
      rleBytes[i] = binaryStr.charCodeAt(i);
    }

    // Decode RLE to full grid
    this.grid = new Uint8Array(this.gridWidth * this.gridHeight);
    let offset = 0;
    for (let i = 0; i < rleBytes.length; i += 2) {
      const count = rleBytes[i];
      const value = rleBytes[i + 1];
      for (let j = 0; j < count; j++) {
        if (offset < this.grid.length) {
          this.grid[offset++] = value;
        }
      }
    }

    this.onJoinedCb?.();
  }

  private handleState(msg: StateMessage) {
    this.prevState = this.latestState;
    this.latestState = msg;
    this.lastTickTime = performance.now();

    for (const cell of msg.gridDiff) {
      this.grid[cell.y * this.gridWidth + cell.x] = cell.c;
    }

    if (this.onStateUpdateCb) {
      const state = this.getState();
      if (state) this.onStateUpdateCb(state);
    }
  }
}
