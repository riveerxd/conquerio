import type { NetworkClient } from "./NetworkClient";
import { Camera } from "./Camera";
import { Renderer } from "./Renderer";
import { StateInterpolator } from "./StateInterpolator";
import type { GameSettings } from "../ui/SettingsContext";

export class GameLoop {
  private renderer: Renderer;
  private camera: Camera;
  private interpolator: StateInterpolator;
  private animFrameId = 0;
  private running = false;
  private lastFrameTime = 0;
  private cameraInitialized = false;
  private spectateTargetId: string | null = null;

  constructor(canvas: HTMLCanvasElement, private network: NetworkClient, settings: GameSettings) {
    this.camera = new Camera();
    this.renderer = new Renderer(canvas, this.camera, settings);
    this.interpolator = new StateInterpolator(network);
  }

  setSettings(settings: GameSettings) {
    this.renderer.setSettings(settings);
  }

  start() {
    this.running = true;
    this.lastFrameTime = performance.now();
    this.tick();
  }

  stop() {
    this.running = false;
    cancelAnimationFrame(this.animFrameId);
  }

  setSpectateTarget(id: string | null) {
    this.spectateTargetId = id;
    this.cameraInitialized = false; // snap to new target on next frame
  }

  private tick = () => {
    if (!this.running) return;

    const now = performance.now();
    const deltaSeconds = Math.min((now - this.lastFrameTime) / 1000, 0.1); // cap at 100ms to avoid jumps after tab blur
    this.lastFrameTime = now;

    const state = this.network.getState();
    if (state) {
      const targetId = this.spectateTargetId ?? state.myPlayerId;
      const target = state.players.find((p) => p.id === targetId)
        ?? (this.spectateTargetId ? state.players[0] : undefined);

      if (target) {
        if (!this.cameraInitialized) {
          this.camera.snapTo(target.x, target.y);
          this.cameraInitialized = true;
        } else {
          this.camera.follow(target.x, target.y, deltaSeconds);
        }
      }

      const interpolated = this.interpolator.getPlayers();
      this.renderer.render(state, interpolated);
    }

    this.animFrameId = requestAnimationFrame(this.tick);
  };
}
