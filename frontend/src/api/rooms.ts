const API = "/api/rooms";

export interface RoomInfo {
  id: string;
  name: string;
  playerCount: number;
  maxPlayers: number;
  gridSize: "small" | "medium" | "large";
  abilitiesEnabled: boolean;
  isPrivate: boolean;
}

export interface CreateRoomSettings {
  name?: string;
  gridSize: "small" | "medium" | "large";
  maxPlayers: number;
  abilitiesEnabled: boolean;
  joinCode?: string;
}

export async function getRooms(token: string): Promise<RoomInfo[]> {
  const res = await fetch(API, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error(`failed to fetch rooms: ${res.status}`);
  return res.json();
}

export async function createRoom(token: string, settings: CreateRoomSettings): Promise<RoomInfo> {
  const res = await fetch(API, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({
      name: settings.name || undefined,
      gridSize: settings.gridSize,
      maxPlayers: settings.maxPlayers,
      abilitiesEnabled: settings.abilitiesEnabled,
      joinCode: settings.joinCode || undefined,
    }),
  });
  if (!res.ok) throw new Error(`failed to create room: ${res.status}`);
  return res.json();
}
