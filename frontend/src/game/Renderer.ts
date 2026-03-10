import { Camera } from "./Camera";
import {
  getColor,
  getTerritoryColor,
  getTerritoryBorderColor,
  getTrailColor,
  getTrailStrokeColor,
} from "./colors";
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
  }

  private drawGrid(state: GameState, canvasW: number, canvasH: number) {
    const bounds = this.camera.getVisibleBounds(canvasW, canvasH);
    const cs = this.camera.cellSize;
    const ctx = this.ctx;

    for (let y = Math.max(0, bounds.minY); y < Math.min(state.gridHeight, bounds.maxY); y++) {
      for (let x = Math.max(0, bounds.minX); x < Math.min(state.gridWidth, bounds.maxX); x++) {
        const colorId = state.grid[y * state.gridWidth + x];
        if (colorId === 0) continue;

        const { sx, sy } = this.camera.worldToScreen(x, y, canvasW, canvasH);

        // Fill: low-opacity wash for the claimed territory
        ctx.fillStyle = getTerritoryColor(colorId);
        ctx.fillRect(sx, sy, cs, cs);

        // Inner 1-px border: slightly higher opacity to give each cell definition
        ctx.strokeStyle = getTerritoryBorderColor(colorId);
        ctx.lineWidth = 1;
        ctx.strokeRect(sx + 0.5, sy + 0.5, cs - 1, cs - 1);
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
    const ctx = this.ctx;

    for (const p of players) {
      if (p.trail.length === 0) continue;

      // Fill all trail cells first (single style swap)
      ctx.fillStyle = getTrailColor(p.colorId);
      for (const [tx, ty] of p.trail) {
        const { sx, sy } = this.camera.worldToScreen(tx, ty, canvasW, canvasH);
        ctx.fillRect(sx, sy, cs, cs);
      }

      // Crisp 1-px stroke over each trail cell for a sharp, defined look
      ctx.strokeStyle = getTrailStrokeColor(p.colorId);
      ctx.lineWidth = 1;
      for (const [tx, ty] of p.trail) {
        const { sx, sy } = this.camera.worldToScreen(tx, ty, canvasW, canvasH);
        ctx.strokeRect(sx + 0.5, sy + 0.5, cs - 1, cs - 1);
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

    // trail cells on minimap — use a brighter shade so they remain legible
    for (const p of players) {
      if (p.trail.length === 0) continue;
      ctx.fillStyle = getTrailColor(p.colorId);
      for (const [tx, ty] of p.trail) {
        ctx.fillRect(
          mx + tx * scaleX,
          my + ty * scaleY,
          Math.ceil(scaleX) + 1,
          Math.ceil(scaleY) + 1
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
}
