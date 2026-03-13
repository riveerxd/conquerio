const COLORS = [
  "#000000", // 0 = unclaimed
  "#e74c3c", // 1
  "#3498db", // 2
  "#2ecc71", // 3
  "#f39c12", // 4
  "#9b59b6", // 5
  "#1abc9c", // 6
  "#e67e22", // 7
  "#e84393", // 8
  "#00cec9", // 9
  "#6c5ce7", // 10
  "#fdcb6e", // 11
  "#fab1a0", // 12
  "#74b9ff", // 13
  "#a29bfe", // 14
  "#55efc4", // 15
];

export function getColor(colorId: number): string {
  return COLORS[colorId % COLORS.length] ?? COLORS[1];
}

/**
 * Trail fill: solid player colour at 70% opacity so trails are clearly
 * visible against the background but distinct from the player dot itself.
 */
export function getTrailColor(colorId: number): string {
  return getColor(colorId) + "b3"; // ~70 % opacity
}

/**
 * Trail stroke / outline: full-opacity player colour for a crisp 1-px border
 * that makes each trail segment feel defined and intentional.
 */
export function getTrailStrokeColor(colorId: number): string {
  return getColor(colorId); // 100 % opacity
}

/**
 * Claimed territory fill: 22% opacity — subtle wash so the grid is readable
 * beneath the action and clearly lower-priority than trails.
 */
export function getTerritoryColor(colorId: number): string {
  return getColor(colorId) + "38"; // ~22 % opacity
}

/**
 * Claimed territory border tint: 45% opacity used to draw a 1-px inner
 * outline on territory cells, giving them shape without visual noise.
 */
export function getTerritoryBorderColor(colorId: number): string {
  return getColor(colorId) + "73"; // ~45 % opacity
}
