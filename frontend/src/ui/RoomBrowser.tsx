import { useEffect, useState } from "react";
import { getRooms, createRoom, type RoomInfo, type CreateRoomSettings } from "../api/rooms";
import CreateRoomModal from "./CreateRoomModal";

interface Props {
  token: string;
  joinError?: string | null;
  onJoinRoom: (roomId: string, joinCode?: string) => void;
  onQuickPlay: () => void;
  onProfile: () => void;
  onLogout: () => void;
}

export default function RoomBrowser({ token, joinError, onJoinRoom, onQuickPlay, onProfile, onLogout }: Props) {
  const [rooms, setRooms] = useState<RoomInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [error, setError] = useState("");
  const [joinPrompt, setJoinPrompt] = useState<{ room: RoomInfo; code: string } | null>(null);

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

  const handleCreate = async (settings: CreateRoomSettings) => {
    setCreating(true);
    try {
      const room = await createRoom(token, settings);
      setShowModal(false);
      onJoinRoom(room.id, settings.joinCode);
    } catch {
      setError("failed to create room");
      setCreating(false);
    }
  };

  const handleJoinClick = (room: RoomInfo) => {
    if (room.isPrivate) {
      setJoinPrompt({ room, code: "" });
    } else {
      onJoinRoom(room.id);
    }
  };

  return (
    <div style={styles.container}>
      <h1 style={styles.title}>conquerio</h1>

      <div style={styles.actions}>
        <button style={styles.primaryButton} onClick={onQuickPlay}>
          quick play
        </button>
        <button style={styles.secondaryButton} onClick={() => setShowModal(true)}>
          create room
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
                <div style={styles.roomNameRow}>
                  <span style={styles.roomName}>{room.isPrivate ? "🔒 " : ""}{room.name}</span>
                </div>
                <div style={styles.roomTags}>
                  <span style={styles.tag}>{room.gridSize}</span>
                  {room.abilitiesEnabled && <span style={styles.tag}>abilities</span>}
                  <span style={full ? styles.playerCountFull : styles.playerCount}>
                    {room.playerCount}/{room.maxPlayers}
                  </span>
                </div>
              </div>
              <button
                style={full ? styles.joinButtonDisabled : styles.joinButton}
                disabled={full}
                onClick={() => handleJoinClick(room)}
              >
                {full ? "full" : room.isPrivate ? "enter code" : "join"}
              </button>
            </div>
          );
        })}
      </div>

      {(error || joinError) && <div style={styles.error}>{error || joinError}</div>}

      <div style={styles.footer}>
        <button style={styles.footerButton} onClick={onProfile}>my profile</button>
        <button style={styles.footerButton} onClick={onLogout}>logout</button>
      </div>

      {showModal && (
        <CreateRoomModal
          loading={creating}
          onConfirm={handleCreate}
          onClose={() => setShowModal(false)}
        />
      )}

      {joinPrompt && (
        <div style={styles.promptBackdrop} onClick={() => setJoinPrompt(null)}>
          <div style={styles.promptBox} onClick={(e) => e.stopPropagation()}>
            <h3 style={styles.promptTitle}>🔒 private room</h3>
            <p style={styles.promptSub}>{joinPrompt.room.name}</p>
            <input
              style={styles.promptInput}
              placeholder="enter join code"
              autoFocus
              value={joinPrompt.code}
              onChange={(e) => setJoinPrompt({ ...joinPrompt, code: e.target.value })}
              onKeyDown={(e) => {
                if (e.key === "Enter") { setJoinPrompt(null); onJoinRoom(joinPrompt.room.id, joinPrompt.code); }
              }}
            />
            <div style={styles.promptActions}>
              <button style={styles.promptCancel} onClick={() => setJoinPrompt(null)}>cancel</button>
              <button style={styles.promptJoin} onClick={() => { setJoinPrompt(null); onJoinRoom(joinPrompt.room.id, joinPrompt.code); }}>
                join
              </button>
            </div>
          </div>
        </div>
      )}
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
    maxWidth: "440px",
  },
  roomCard: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    padding: "12px 16px",
    background: "#1a1a1a",
    border: "1px solid #2a2a2a",
  },
  roomInfo: {
    display: "flex",
    flexDirection: "column",
    gap: "5px",
  },
  roomNameRow: {
    display: "flex",
    alignItems: "center",
    gap: "6px",
  },
  roomName: {
    fontSize: "14px",
    color: "#fff",
  },
  roomTags: {
    display: "flex",
    gap: "8px",
    alignItems: "center",
  },
  tag: {
    fontSize: "11px",
    color: "#555",
  },
  playerCount: {
    fontSize: "12px",
    color: "#666",
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
    background: "#222",
    color: "#555",
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
  footer: {
    display: "flex",
    gap: "20px",
    marginTop: "24px",
  },
  footerButton: {
    background: "none",
    border: "none",
    color: "#555",
    cursor: "pointer",
    fontSize: "13px",
    fontFamily: "monospace",
    padding: 0,
  },
  promptBackdrop: {
    position: "fixed",
    inset: 0,
    background: "rgba(0,0,0,0.7)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    zIndex: 50,
    fontFamily: "monospace",
  },
  promptBox: {
    background: "#1a1a1a",
    border: "1px solid #333",
    padding: "28px 32px",
    width: "100%",
    maxWidth: "340px",
    display: "flex",
    flexDirection: "column",
    gap: "12px",
    color: "#fff",
  },
  promptTitle: {
    margin: 0,
    fontSize: "16px",
    letterSpacing: "1px",
  },
  promptSub: {
    margin: 0,
    fontSize: "13px",
    color: "#777",
  },
  promptInput: {
    background: "#111",
    border: "1px solid #333",
    color: "#fff",
    fontFamily: "monospace",
    fontSize: "14px",
    padding: "9px 12px",
    outline: "none",
  },
  promptActions: {
    display: "flex",
    gap: "8px",
    marginTop: "4px",
  },
  promptCancel: {
    flex: 1,
    padding: "9px",
    background: "none",
    border: "1px solid #333",
    color: "#666",
    fontFamily: "monospace",
    fontSize: "13px",
    cursor: "pointer",
  },
  promptJoin: {
    flex: 1,
    padding: "9px",
    background: "#fff",
    border: "none",
    color: "#111",
    fontFamily: "monospace",
    fontSize: "13px",
    fontWeight: "bold",
    cursor: "pointer",
  },
};
