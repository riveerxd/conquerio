import type { NetworkClient } from "./NetworkClient";
import type { Direction } from "./types";
import type { GameSettings } from "../ui/SettingsContext";

export class InputHandler {
  private currentDir: Direction | null = null;
  private handler;
  private settings: GameSettings;

  constructor(private network: NetworkClient, initialSettings: GameSettings) {
    this.settings = initialSettings;
    this.handler = (e: KeyboardEvent) => {
      if (e.key === this.settings.keybinds.boost) {
        e.preventDefault();
        this.network.sendAbility("BOOST");
        return;
      }
      if (e.key === this.settings.keybinds.shield) {
        e.preventDefault();
        this.network.sendAbility("SHIELD");
        return;
      }

      const dir = this.getDirectionFromKey(e.key);
      if (dir && dir !== this.currentDir) {
        this.currentDir = dir;
        this.network.sendInput(dir);
      }
    };
    window.addEventListener("keydown", this.handler);
  }

  setSettings(settings: GameSettings) {
    this.settings = settings;
  }

  private getDirectionFromKey(key: string): Direction | null {
    const { keybinds } = this.settings;
    if (key === keybinds.up || key === "ArrowUp") return "up";
    if (key === keybinds.down || key === "ArrowDown") return "down";
    if (key === keybinds.left || key === "ArrowLeft") return "left";
    if (key === keybinds.right || key === "ArrowRight") return "right";
    return null;
  }

  destroy() {
    window.removeEventListener("keydown", this.handler);
  }
}
