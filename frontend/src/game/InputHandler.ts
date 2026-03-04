import type { NetworkClient } from "./NetworkClient";
import type { Direction } from "./types";

const KEY_MAP: Record<string, Direction> = {
  w: "up",
  a: "left",
  s: "down",
  d: "right",
  ArrowUp: "up",
  ArrowLeft: "left",
  ArrowDown: "down",
  ArrowRight: "right",
};

export class InputHandler {
  private currentDir: Direction | null = null;
  private handler;

  constructor(private network: NetworkClient) {
    this.handler = (e: KeyboardEvent) => {
      if (e.key === " ") {
        e.preventDefault();
        this.network.sendAbility("BOOST");
        return;
      }
      const dir = KEY_MAP[e.key];
      if (dir && dir !== this.currentDir) {
        this.currentDir = dir;
        this.network.sendInput(dir);
      }
    };
    window.addEventListener("keydown", this.handler);
  }

  destroy() {
    window.removeEventListener("keydown", this.handler);
  }
}
