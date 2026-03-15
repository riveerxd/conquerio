export class Camera {
  centerX = 0;
  centerY = 0;
  cellSize = 20;

  /** Smoothing factor (0–1): higher = snappier, lower = more lag. */
  private readonly LERP = 0.1;

  /**
   * Instantly snap the camera to a position.
   * Use only for initial placement (first frame), not every tick.
   */
  snapTo(playerX: number, playerY: number) {
    this.centerX = playerX;
    this.centerY = playerY;
  }

  /**
   * Smoothly ease the camera toward the target position.
   * Call once per animation frame with the frame delta (seconds).
   */
  follow(targetX: number, targetY: number, deltaSeconds: number) {
    // Frame-rate-independent lerp: factor per second = LERP, scaled to frame time.
    // At 60 fps (deltaSeconds ≈ 0.016) this gives ~0.1 per frame, matching the issue spec.
    const factor = 1 - Math.pow(1 - this.LERP, deltaSeconds * 60);
    this.centerX += (targetX - this.centerX) * factor;
    this.centerY += (targetY - this.centerY) * factor;
  }

  worldToScreen(wx: number, wy: number, canvasWidth: number, canvasHeight: number) {
    const sx = (wx - this.centerX) * this.cellSize + canvasWidth / 2;
    const sy = (wy - this.centerY) * this.cellSize + canvasHeight / 2;
    return { sx, sy };
  }

  getVisibleBounds(canvasWidth: number, canvasHeight: number) {
    const halfW = canvasWidth / 2 / this.cellSize;
    const halfH = canvasHeight / 2 / this.cellSize;
    return {
      minX: Math.floor(this.centerX - halfW) - 1,
      maxX: Math.ceil(this.centerX + halfW) + 1,
      minY: Math.floor(this.centerY - halfH) - 1,
      maxY: Math.ceil(this.centerY + halfH) + 1,
    };
  }
}
