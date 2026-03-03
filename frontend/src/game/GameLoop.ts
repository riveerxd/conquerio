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

  constructor(canvas: HTMLCanvasElement, private network: NetworkClient) {
    this.camera = new Camera();
    this.renderer = new Renderer(canvas, this.camera);
    this.interpolator = new StateInterpolator(network);
  }

  start() {
    this.running = true;
    this.tick();
  }

  stop() {
    this.running = false;
    cancelAnimationFrame(this.animFrameId);
  }

  private tick = () => {
    if (!this.running) return;

    const state = this.network.getState();
    if (state) {
      const me = state.players.find((p) => p.id === state.myPlayerId);
      if (me) this.camera.update(me.x, me.y);

      // TODO: maybe add camera smoothing later so it doesnt snap
      const interpolated = this.interpolator.getPlayers();
      this.renderer.render(state, interpolated);
    }

    this.animFrameId = requestAnimationFrame(this.tick);
  };
}
