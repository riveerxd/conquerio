import type { NetworkClient } from "./NetworkClient";
import { Camera } from "./Camera";
import { Renderer } from "./Renderer";
import { StateInterpolator } from "./StateInterpolator";

export class GameLoop {
  private renderer: Renderer;
  private camera: Camera;
  private interpolator: StateInterpolator;
  private animFrameId = 0;
  private running = false;
  private lastFrameTime = 0;
  private cameraInitialized = false;

  constructor(canvas: HTMLCanvasElement, private network: NetworkClient) {
    this.camera = new Camera();
    this.renderer = new Renderer(canvas, this.camera);
    this.interpolator = new StateInterpolator(network);
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

  private tick = () => {
    if (!this.running) return;

    const now = performance.now();
    const deltaSeconds = Math.min((now - this.lastFrameTime) / 1000, 0.1); // cap at 100ms to avoid jumps after tab blur
    this.lastFrameTime = now;

    const state = this.network.getState();
    if (state) {
      const me = state.players.find((p) => p.id === state.myPlayerId);
      if (me) {
        if (!this.cameraInitialized) {
          // Snap on first frame so camera doesn't slide in from (0,0)
          this.camera.snapTo(me.x, me.y);
          this.cameraInitialized = true;
        } else {
          this.camera.follow(me.x, me.y, deltaSeconds);
        }
      }

      const interpolated = this.interpolator.getPlayers();
      this.renderer.render(state, interpolated);
    }

    this.animFrameId = requestAnimationFrame(this.tick);
  };
}
