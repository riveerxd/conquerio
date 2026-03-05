import { useEffect, useState } from "react";
import { getRooms, createRoom, type RoomInfo } from "../api/rooms";

interface Props {
  token: string;
  onJoinRoom: (roomId: string) => void;
  onQuickPlay: () => void;
  onLogout: () => void;
}

export default function RoomBrowser({ token, onJoinRoom, onQuickPlay, onLogout }: Props) {
  const [rooms, setRooms] = useState<RoomInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState("");

  const fetchRooms = async () => {
    try {
      const data = await getRooms(token);
      setRooms(data);
      setError("");
    } catch {
      setError("failed to load rooms");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRooms();
    const interval = setInterval(fetchRooms, 5000);
    return () => clearInterval(interval);
  }, []);

  const handleCreate = async () => {
    setCreating(true);
    try {
      const room = await createRoom(token);
      onJoinRoom(room.id);
    } catch {
      setError("failed to create room");
      setCreating(false);
    }
  };

  return (
    <div style={styles.container}>
      <h1 style={styles.title}>conquerio</h1>

      <div style={styles.actions}>
        <button style={styles.primaryButton} onClick={onQuickPlay}>
          quick play
        </button>
        <button style={styles.secondaryButton} onClick={handleCreate} disabled={creating}>
          {creating ? "..." : "create room"}
        </button>
      </div>

      <div style={styles.roomList}>
        {loading && <div style={styles.muted}>loading rooms...</div>}
        {!loading && rooms.length === 0 && (
          <div style={styles.muted}>no active rooms - create one or quick play</div>
        )}
        {rooms.map((room) => {
          const full = room.playerCount >= room.maxPlayers;
          return (
            <div key={room.id} style={styles.roomCard}>
              <div style={styles.roomInfo}>
                <span style={styles.roomName}>{room.name}</span>
                <span style={full ? styles.playerCountFull : styles.playerCount}>
                  {room.playerCount}/{room.maxPlayers}
                </span>
              </div>
              <button
                style={full ? styles.joinButtonDisabled : styles.joinButton}
                disabled={full}
                onClick={() => onJoinRoom(room.id)}
              >
                {full ? "full" : "join"}
              </button>
            </div>
          );
        })}
      </div>

      {error && <div style={styles.error}>{error}</div>}

      <button style={styles.logoutButton} onClick={onLogout}>
        logout
      </button>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  container: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    minHeight: "100vh",
    background: "#111",
    color: "#fff",
    fontFamily: "monospace",
    padding: "40px 20px",
  },
  title: {
    fontSize: "48px",
    marginBottom: "32px",
    letterSpacing: "4px",
  },
  actions: {
    display: "flex",
    gap: "12px",
    marginBottom: "24px",
  },
  primaryButton: {
    padding: "12px 24px",
    background: "#fff",
    color: "#111",
    border: "none",
    fontSize: "16px",
    fontFamily: "monospace",
    cursor: "pointer",
    fontWeight: "bold",
  },
  secondaryButton: {
    padding: "12px 24px",
    background: "transparent",
    color: "#fff",
    border: "1px solid #333",
    fontSize: "16px",
    fontFamily: "monospace",
    cursor: "pointer",
  },
  roomList: {
    display: "flex",
    flexDirection: "column",
    gap: "8px",
    width: "100%",
    maxWidth: "400px",
  },
  roomCard: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    padding: "12px 16px",
    background: "#222",
    border: "1px solid #333",
  },
  roomInfo: {
    display: "flex",
    flexDirection: "column",
    gap: "4px",
  },
  roomName: {
    fontSize: "14px",
    color: "#fff",
  },
  playerCount: {
    fontSize: "12px",
    color: "#999",
  },
  playerCountFull: {
    fontSize: "12px",
    color: "#e74c3c",
  },
  joinButton: {
    padding: "6px 16px",
    background: "#fff",
    color: "#111",
    border: "none",
    fontSize: "13px",
    fontFamily: "monospace",
    cursor: "pointer",
    fontWeight: "bold",
  },
  joinButtonDisabled: {
    padding: "6px 16px",
    background: "#333",
    color: "#666",
    border: "none",
    fontSize: "13px",
    fontFamily: "monospace",
    cursor: "not-allowed",
  },
  muted: {
    color: "#666",
    fontSize: "13px",
    textAlign: "center",
    padding: "20px",
  },
  error: {
    color: "#e74c3c",
    fontSize: "13px",
    marginTop: "12px",
  },
  logoutButton: {
    background: "none",
    border: "none",
    color: "#666",
    cursor: "pointer",
    fontSize: "13px",
    fontFamily: "monospace",
    marginTop: "24px",
  },
};
