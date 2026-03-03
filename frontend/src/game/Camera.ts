export class Camera {
  centerX = 0;
  centerY = 0;
  cellSize = 20;

  update(playerX: number, playerY: number) {
    this.centerX = playerX;
    this.centerY = playerY;
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
