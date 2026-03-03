import type { Player } from "./types";
import type { NetworkClient } from "./NetworkClient";

export class StateInterpolator {
  constructor(private network: NetworkClient) {}

  getPlayers(): Player[] {
    const state = this.network.getState();
    const prev = this.network.getPrevState();
    if (!state) return [];

    const tickMs = 1000 / this.network.getTickRate();
    const elapsed = performance.now() - this.network.getLastTickTime();
    const t = Math.min(elapsed / tickMs, 1.0);

    return state.players.map((curr) => {
      const prevPlayer = prev?.players.find((p) => p.id === curr.id);
      if (!prevPlayer) return curr;

      return {
        ...curr,
        x: prevPlayer.x + (curr.x - prevPlayer.x) * t,
        y: prevPlayer.y + (curr.y - prevPlayer.y) * t,
      };
    });
  }
}
