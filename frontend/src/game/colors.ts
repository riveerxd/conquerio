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

const COLORBLIND_COLORS = [
  "#000000", // 0 = unclaimed
  "#0072B2", // 1
  "#D55E00", // 2
  "#CC79A7", // 3
  "#E69F00", // 4
  "#56B4E9", // 5
  "#009E73", // 6
  "#F0E442", // 7
  "#000000", // 8 (looping back with different variants if needed, but Wong palette has 8 colors)
  "#0072B2", // 9
  "#D55E00", // 10
  "#CC79A7", // 11
  "#E69F00", // 12
  "#56B4E9", // 13
  "#009E73", // 14
  "#F0E442", // 15
];

export function getColor(colorId: number, colorblindMode = false): string {
  const palette = colorblindMode ? COLORBLIND_COLORS : COLORS;
  return palette[colorId % palette.length] ?? palette[1];
}

/**
 * Trail fill: solid player colour at 70% opacity so trails are clearly
 * visible against the background but distinct from the player dot itself.
 */
export function getTrailColor(colorId: number, colorblindMode = false): string {
  return getColor(colorId, colorblindMode) + "b3"; // ~70 % opacity
}

/**
 * Trail stroke / outline: full-opacity player colour for a crisp 1-px border
 * that makes each trail segment feel defined and intentional.
 */
export function getTrailStrokeColor(colorId: number, colorblindMode = false): string {
  return getColor(colorId, colorblindMode); // 100 % opacity
}

/**
 * Claimed territory fill: 22% opacity — subtle wash so the grid is readable
 * beneath the action and clearly lower-priority than trails.
 */
export function getTerritoryColor(colorId: number, colorblindMode = false): string {
  return getColor(colorId, colorblindMode) + "38"; // ~22 % opacity
}

/**
 * Claimed territory border tint: 45% opacity used to draw a 1-px inner
 * outline on territory cells, giving them shape without visual noise.
 */
export function getTerritoryBorderColor(colorId: number, colorblindMode = false): string {
  return getColor(colorId, colorblindMode) + "73"; // ~45 % opacity
}
