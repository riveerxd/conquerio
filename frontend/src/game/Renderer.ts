import { Camera } from "./Camera";
import { getColor, getTerritoryColor, getTrailColor } from "./colors";
import type { Player, GameState } from "./types";

export class Renderer {
  private ctx: CanvasRenderingContext2D;
  private camera: Camera;

  constructor(private canvas: HTMLCanvasElement, camera: Camera) {
    this.ctx = canvas.getContext("2d")!;
    this.camera = camera;
  }

  render(state: GameState, interpolatedPlayers: Player[]) {
    const { width, height } = this.canvas;
    const ctx = this.ctx;

    ctx.fillStyle = "#111";
    ctx.fillRect(0, 0, width, height);

    this.drawGrid(state, width, height);
    this.drawGridLines(state, width, height);
    this.drawTrails(interpolatedPlayers, width, height);
    this.drawPlayers(interpolatedPlayers, state.myPlayerId, width, height);
    this.drawMinimap(state, interpolatedPlayers, width, height);
    this.drawAbilities(width, height)
  }

  private drawGrid(state: GameState, canvasW: number, canvasH: number) {
    const bounds = this.camera.getVisibleBounds(canvasW, canvasH);
    const cs = this.camera.cellSize;

    for (let y = Math.max(0, bounds.minY); y < Math.min(state.gridHeight, bounds.maxY); y++) {
      for (let x = Math.max(0, bounds.minX); x < Math.min(state.gridWidth, bounds.maxX); x++) {
        const colorId = state.grid[y * state.gridWidth + x];
        if (colorId === 0) continue;

        const { sx, sy } = this.camera.worldToScreen(x, y, canvasW, canvasH);
        this.ctx.fillStyle = getTerritoryColor(colorId);
        this.ctx.fillRect(sx, sy, cs, cs);
      }
    }
  }

  private drawGridLines(state: GameState, canvasW: number, canvasH: number) {
    const bounds = this.camera.getVisibleBounds(canvasW, canvasH);
    const cs = this.camera.cellSize;
    const ctx = this.ctx;

    const topLeft = this.camera.worldToScreen(0, 0, canvasW, canvasH);
    const botRight = this.camera.worldToScreen(state.gridWidth, state.gridHeight, canvasW, canvasH);

    // border
    ctx.strokeStyle = "#555";
    ctx.lineWidth = 2;
    ctx.strokeRect(topLeft.sx, topLeft.sy, botRight.sx - topLeft.sx, botRight.sy - topLeft.sy);

    if (cs >= 15) {
      ctx.strokeStyle = "#1a1a1a";
      ctx.lineWidth = 0.5;
      ctx.beginPath();

      for (let x = Math.max(0, bounds.minX); x <= Math.min(state.gridWidth, bounds.maxX); x++) {
        const { sx } = this.camera.worldToScreen(x, 0, canvasW, canvasH);
        ctx.moveTo(sx, topLeft.sy);
        ctx.lineTo(sx, botRight.sy);
      }

      for (let y = Math.max(0, bounds.minY); y <= Math.min(state.gridHeight, bounds.maxY); y++) {
        const { sy } = this.camera.worldToScreen(0, y, canvasW, canvasH);
        ctx.moveTo(topLeft.sx, sy);
        ctx.lineTo(botRight.sx, sy);
      }

      ctx.stroke();
    }
  }

  private drawTrails(players: Player[], canvasW: number, canvasH: number) {
    const cs = this.camera.cellSize;

    for (const p of players) {
      if (p.trail.length === 0) continue;

      this.ctx.fillStyle = getTrailColor(p.colorId);
      for (const [tx, ty] of p.trail) {
        const { sx, sy } = this.camera.worldToScreen(tx, ty, canvasW, canvasH);
        this.ctx.fillRect(sx, sy, cs, cs);
      }
    }
  }

  private drawPlayers(players: Player[], myId: string, canvasW: number, canvasH: number) {
    const cs = this.camera.cellSize;

    for (const p of players) {
      if (!p.alive) continue;

      const { sx, sy } = this.camera.worldToScreen(p.x, p.y, canvasW, canvasH);

      this.ctx.fillStyle = getColor(p.colorId);
      this.ctx.fillRect(sx + 1, sy + 1, cs - 2, cs - 2);

      // TODO: show actual username instead of id
      this.ctx.fillStyle = "#fff";
      this.ctx.font = "12px monospace";
      this.ctx.textAlign = "center";
      const label = p.id === myId ? "you" : p.id.slice(0, 6);
      this.ctx.fillText(label, sx + cs / 2, sy - 4);
    }
  }

  private drawMinimap(state: GameState, players: Player[], canvasW: number, _canvasH: number) {
    const ctx = this.ctx;
    const size = 150;
    const padding = 12;
    const mx = canvasW - size - padding;
    const my = padding;

    // bg
    ctx.fillStyle = "rgba(0, 0, 0, 0.6)";
    ctx.fillRect(mx, my, size, size);
    ctx.strokeStyle = "#444";
    ctx.lineWidth = 1;
    ctx.strokeRect(mx, my, size, size);

    const scaleX = size / state.gridWidth;
    const scaleY = size / state.gridHeight;

    // territory
    for (let y = 0; y < state.gridHeight; y++) {
      for (let x = 0; x < state.gridWidth; x++) {
        const c = state.grid[y * state.gridWidth + x];
        if (c === 0) continue;
        ctx.fillStyle = getColor(c);
        ctx.fillRect(
          mx + x * scaleX,
          my + y * scaleY,
          Math.ceil(scaleX),
          Math.ceil(scaleY)
        );
      }
    }

    // players as dots
    for (const p of players) {
      if (!p.alive) continue;
      const dotX = mx + p.x * scaleX;
      const dotY = my + p.y * scaleY;
      const isMe = p.id === state.myPlayerId;

      ctx.fillStyle = isMe ? "#fff" : getColor(p.colorId);
      ctx.fillRect(dotX - 2, dotY - 2, isMe ? 5 : 3, isMe ? 5 : 3);
    }
  }

  private drawAbilities(canvasW: number, canvasH: number, img1?: HTMLImageElement, img2?: HTMLImageElement) {
    const ctx = this.ctx;
    const boxSize = 50;
    const gap = 16;
    const paddingBottom = 24;
    const textOffset = 18;

    const totalWidth = (boxSize * 2) + gap;
    const startX = (canvasW - totalWidth) / 2;
    const startY = canvasH - boxSize - paddingBottom - textOffset;

    const drawAbilityBox = (x: number, y: number, keybind: string, label: string, img?: HTMLImageElement) => {
      ctx.fillStyle = "rgba(0, 0, 0, 0.6)";
      ctx.fillRect(x, y, boxSize, boxSize);

      if (img) {
        ctx.drawImage(img, x, y, boxSize, boxSize);
      }

      ctx.strokeStyle = "#444";
      ctx.lineWidth = 1;
      ctx.strokeRect(x, y, boxSize, boxSize);

      ctx.fillStyle = "#fff";
      ctx.font = "bold 12px monospace";
      ctx.textAlign = "left";
      ctx.fillText(keybind, x + 4, y + 14);

      ctx.font = "12px monospace";
      ctx.fillStyle = "#aaa";
      ctx.textAlign = "center";
      ctx.fillText(label, x + boxSize / 2, y + boxSize + textOffset);
    };

    const boostImg = new Image();
    boostImg.src = "/img/boost.png";

    const shieldImg = new Image();
    shieldImg.src = "/img/shield.png";

    drawAbilityBox(startX, startY, "Q", "Ability 1", boostImg);
    drawAbilityBox(startX + boxSize + gap, startY, "E", "Ability 2", shieldImg);
  }
}
