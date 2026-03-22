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

// Paul Tol's qualitative "bright"+"muted" palette — validated to be
// distinguishable under deuteranopia, protanopia and tritanopia.
// See: https://personal.sron.nl/~pault/#sec:qualitative
const CB_COLORS = [
  "#000000", // 0 = unclaimed
  "#4477AA", // 1  - blue
  "#EE6677", // 2  - rose
  "#228833", // 3  - green
  "#CCBB44", // 4  - yellow
  "#66CCEE", // 5  - cyan
  "#AA3377", // 6  - purple
  "#BBBBBB", // 7  - grey
  "#332288", // 8  - indigo
  "#44AA99", // 9  - teal
  "#999933", // 10 - olive
  "#DDCC77", // 11 - sand
  "#CC6677", // 12 - pink
  "#882255", // 13 - wine
  "#AA4499", // 14 - violet
  "#117733", // 15 - dark green
];

function palette(colorblind: boolean) {
  return colorblind ? CB_COLORS : COLORS;
}

export function getColor(colorId: number, colorblind = false): string {
  const p = palette(colorblind);
  return p[colorId % p.length] ?? p[1];
}

/**
 * Trail fill: solid player colour at 70% opacity so trails are clearly
 * visible against the background but distinct from the player dot itself.
 */
export function getTrailColor(colorId: number, colorblind = false): string {
  return getColor(colorId, colorblind) + "b3"; // ~70 % opacity
}

/**
 * Trail stroke / outline: full-opacity player colour for a crisp 1-px border
 * that makes each trail segment feel defined and intentional.
 */
export function getTrailStrokeColor(colorId: number, colorblind = false): string {
  return getColor(colorId, colorblind); // 100 % opacity
}

/**
 * Claimed territory fill: 22% opacity — subtle wash so the grid is readable
 * beneath the action and clearly lower-priority than trails.
 */
export function getTerritoryColor(colorId: number, colorblind = false): string {
  return getColor(colorId, colorblind) + "38"; // ~22 % opacity
}

/**
 * Claimed territory border tint: 45% opacity used to draw a 1-px inner
 * outline on territory cells, giving them shape without visual noise.
 */
export function getTerritoryBorderColor(colorId: number, colorblind = false): string {
  return getColor(colorId, colorblind) + "73"; // ~45 % opacity
}
