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

export function getTrailColor(colorId: number): string {
  const base = getColor(colorId);
  return base + "88"; // semi-transparent
}

export function getTerritoryColor(colorId: number): string {
  const base = getColor(colorId);
  return base + "44"; // more transparent
}
