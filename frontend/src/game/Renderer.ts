import { Camera } from "./Camera";
import {
  getColor,
  getTerritoryColor,
  getTerritoryBorderColor,
  getTrailColor,
  getTrailStrokeColor,
} from "./colors";
import type { Player, GameState, AbilityInfo } from "./types";
import type { GameSettings } from "../ui/SettingsContext";

export class Renderer {
  private ctx: CanvasRenderingContext2D;
  private canvas: HTMLCanvasElement;
  private camera: Camera;
  private boostImg: HTMLImageElement;
  private shieldImg: HTMLImageElement;
  private settings: GameSettings;
  private isTouchDevice: boolean;

  constructor(canvas: HTMLCanvasElement, camera: Camera, initialSettings: GameSettings, isTouchDevice = false) {
    this.canvas = canvas;
    this.ctx = canvas.getContext("2d")!;
    this.camera = camera;
    this.settings = initialSettings;
    this.isTouchDevice = isTouchDevice;

    this.boostImg = new Image();
    this.boostImg.src = "/img/boost.webp";
    this.shieldImg = new Image();
    this.shieldImg.src = "/img/shield.webp";
  }

  setSettings(settings: GameSettings) {
    this.settings = settings;
  }

  render(state: GameState, interpolatedPlayers: Player[]) {
    const abilities = state.players.find(x => x.id == state.myPlayerId)?.abilities;

    const { width, height } = this.ctx.canvas;
    const ctx = this.ctx;

    ctx.fillStyle = "#111";
    ctx.fillRect(0, 0, width, height);

    this.drawGrid(state, width, height);
    if (this.settings.showGrid) {
      this.drawGridLines(state, width, height);
    }
    this.drawTrails(interpolatedPlayers, width, height);
    this.drawPlayers(interpolatedPlayers, state.myPlayerId, width, height);
    this.drawMinimap(state, interpolatedPlayers, width, height);
    if (abilities) this.drawAbilities(width, height, abilities)
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
        ctx.fillStyle = getTerritoryColor(colorId, this.settings.colorblindMode);
        ctx.fillRect(sx, sy, cs, cs);

        // Inner 1-px border: slightly higher opacity to give each cell definition
        ctx.strokeStyle = getTerritoryBorderColor(colorId, this.settings.colorblindMode);
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
      ctx.fillStyle = getTrailColor(p.colorId, this.settings.colorblindMode);
      for (const [tx, ty] of p.trail) {
        const { sx, sy } = this.camera.worldToScreen(tx, ty, canvasW, canvasH);
        ctx.fillRect(sx, sy, cs, cs);
      }

      // Crisp 1-px stroke over each trail cell for a sharp, defined look
      ctx.strokeStyle = getTrailStrokeColor(p.colorId, this.settings.colorblindMode);
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

      this.ctx.fillStyle = getColor(p.colorId, this.settings.colorblindMode);
      this.ctx.fillRect(sx + 1, sy + 1, cs - 2, cs - 2);
      this.ctx.fillStyle = "#fff";
      this.ctx.font = "12px monospace";
      this.ctx.textAlign = "center";
      const label = p.id === myId ? "you" : p.username;
      this.ctx.fillText(label, sx + cs / 2, sy - 4);
    }
  }

  private drawMinimap(state: GameState, players: Player[], canvasW: number, canvasH: number) {
    const ctx = this.ctx;
    const size = this.settings.minimapSize;
    const padding = 12;
    
    let mx = padding;
    let my = padding;

    const pos = this.settings.minimapPosition;
    if (pos.includes("right")) mx = canvasW - size - padding;
    if (pos.includes("bottom")) my = canvasH - size - padding;

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
        ctx.fillStyle = getColor(c, this.settings.colorblindMode);
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
      ctx.fillStyle = getTrailColor(p.colorId, this.settings.colorblindMode);
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

      ctx.fillStyle = isMe ? "#fff" : getColor(p.colorId, this.settings.colorblindMode);
      ctx.fillRect(dotX - 2, dotY - 2, isMe ? 5 : 3, isMe ? 5 : 3);
    }
  }

  private drawAbilities(
    canvasW: number,
    canvasH: number,
    abilities: Array<AbilityInfo>
  ) {
    const ctx = this.ctx;
    const boxSize = 50;
    const gap = 16;
    const paddingBottom = 24;
    const textOffset = 18;

    const totalWidth = (boxSize * 2) + gap;
    const startX = (canvasW - totalWidth) / 2;
    const startY = canvasH - boxSize - paddingBottom - textOffset;

    const drawAbilityBox = (
      x: number,
      y: number,
      keybind: string,
      label: string,
      cooldownSecondsRemaining: number,
      durationSecondsRemaining: number,
      img?: HTMLImageElement
    ) => {
      ctx.fillStyle = "rgba(0, 0, 0, 0.6)";
      ctx.fillRect(x, y, boxSize, boxSize);

      if (img) {
        ctx.drawImage(img, x, y, boxSize, boxSize);
      }

      const isActive = durationSecondsRemaining > 0;
      const isOnCooldown = cooldownSecondsRemaining > 0 && !isActive;

      ctx.lineWidth = isActive ? 3 : 1;
      ctx.strokeStyle = isActive ? "#00ffcc" : "#444";
      ctx.strokeRect(x, y, boxSize, boxSize);

      if (isOnCooldown) {
        ctx.fillStyle = "rgba(0, 0, 0, 0.7)";
        ctx.fillRect(x, y, boxSize, boxSize);

        ctx.fillStyle = "#fff";
        ctx.font = "bold 20px monospace";
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.fillText(
          Math.ceil(cooldownSecondsRemaining).toString(),
          x + boxSize / 2,
          y + boxSize / 2
        );
      }

      if (isActive) {
        ctx.fillStyle = "rgba(0, 255, 204, 0.2)";
        ctx.fillRect(x, y, boxSize, boxSize);

        ctx.fillStyle = "#00ffcc";
        ctx.font = "bold 16px monospace";
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.fillText(
          Math.ceil(durationSecondsRemaining).toString(),
          x + boxSize / 2,
          y + boxSize / 2
        );
      }

      ctx.fillStyle = "#fff";
      ctx.font = "bold 12px monospace";
      ctx.textAlign = "left";
      ctx.textBaseline = "alphabetic";
      if (!this.isTouchDevice) ctx.fillText(keybind, x + 4, y + 14);

      ctx.font = "12px monospace";
      ctx.fillStyle = isActive ? "#00ffcc" : "#aaa";
      ctx.textAlign = "center";
      ctx.fillText(label, x + boxSize / 2, y + boxSize + textOffset);
    };

    const boostAbility = abilities.find(x => x.name?.toLowerCase() == "boost");
    if (boostAbility) {
      const cooldownSeconds = boostAbility.cooldownSecondsRemaining;
      const durationSeconds = boostAbility.durationSecondsRemaining;
      drawAbilityBox(startX, startY, "Space", "Boost", cooldownSeconds, durationSeconds, this.boostImg);
    }

    const shieldAbility = abilities.find(x => x.name?.toLowerCase() == "shield");
    if (shieldAbility) {
      const cooldownSeconds = shieldAbility.cooldownSecondsRemaining;
      const durationSeconds = shieldAbility.durationSecondsRemaining;
      drawAbilityBox(startX + boxSize + gap, startY, "Shift", "Shield", cooldownSeconds, durationSeconds, this.shieldImg);
    }
  }
}
