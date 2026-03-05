const API = "/api/rooms";

export interface RoomInfo {
  id: string;
  name: string;
  playerCount: number;
  maxPlayers: number;
}

export async function getRooms(token: string): Promise<RoomInfo[]> {
  const res = await fetch(API, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error(`failed to fetch rooms: ${res.status}`);
  return res.json();
}

export async function createRoom(token: string, name?: string): Promise<RoomInfo> {
  const res = await fetch(API, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(name ? { name } : {}),
  });
  if (!res.ok) throw new Error(`failed to create room: ${res.status}`);
  return res.json();
}
