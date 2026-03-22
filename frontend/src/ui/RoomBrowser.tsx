import React, {useEffect, useState} from "react";
import {createRoom, getRooms, type RoomInfo} from "../api/rooms";
import SettingsMenu from "./SettingsMenu";
import CreateRoomModal from "./CreateRoomModal";

interface Props {
  token: string;
  onJoinRoom: (roomId: string, joinCode?: string) => void;
  onQuickPlay: () => void;
  onProfile: () => void;
  onLogout: () => void;
}

export default function RoomBrowser({ token, onJoinRoom, onQuickPlay, onProfile, onLogout }: Props) {
  const [rooms, setRooms] = useState<RoomInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showSettings, setShowSettings] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  // joinPrompt holds the roomId whose join-code dialog is open + the typed code
  const [joinPrompt, setJoinPrompt] = useState<{ roomId: string; code: string } | null>(null);
  const [joinError, setJoinError] = useState("");

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

  const handleJoinClick = (room: RoomInfo) => {
    if (room.isPrivate) {
      setJoinError("");
      setJoinPrompt({ roomId: room.id, code: "" });
    } else {
      onJoinRoom(room.id);
    }
  };

  const handleJoinWithCode = () => {
    if (!joinPrompt) return;
    if (joinPrompt.code.trim() === "") {
      setJoinError("enter the join code");
      return;
    }
    onJoinRoom(joinPrompt.roomId, joinPrompt.code.trim());
    setJoinPrompt(null);
  };

  return (
    <div style={styles.container}>
      <h1 style={styles.title}>conquerio</h1>

      <div style={styles.actions}>
        <button style={styles.primaryButton} onClick={onQuickPlay}>
          quick play
        </button>
        <button style={styles.secondaryButton} onClick={() => setShowCreateModal(true)}>
          create room
        </button>
      </div>

      <div style={styles.roomList} role="list" aria-label="Available rooms">
        {loading && <div style={styles.muted}>loading rooms...</div>}
        {!loading && rooms.length === 0 && (
          <div style={styles.muted}>no active rooms - create one or quick play</div>
        )}
        {rooms.map((room) => {
          const full = room.playerCount >= room.maxPlayers;
          const ariaLabel = `${room.name}, ${room.playerCount} of ${room.maxPlayers} players${full ? ", full" : ""}${room.isPrivate ? ", private" : ""}`;
          return (
            <div key={room.id} style={styles.roomCard} role="listitem" aria-label={ariaLabel}>
              <div style={styles.roomInfo}>
                <div style={styles.roomNameRow}>
                  {room.isPrivate && <span style={styles.lockIcon} title="Private room">&#128274;</span>}
                  <span style={styles.roomName}>{room.name}</span>
                </div>
                <div style={styles.tagRow}>
                  <span style={styles.tag}>{room.gridSize}</span>
                  {!room.abilitiesEnabled && <span style={styles.tag}>no abilities</span>}
                  <span style={full ? styles.playerCountFull : styles.playerCount} aria-hidden="true">
                    {room.playerCount}/{room.maxPlayers}
                  </span>
                </div>
                {joinPrompt?.roomId === room.id && (
                  <div style={styles.codePrompt} onClick={(e) => e.stopPropagation()}>
                    <input
                      style={styles.codeInput}
                      type="text"
                      placeholder="join code"
                      value={joinPrompt.code}
                      maxLength={32}
                      autoFocus
                      onChange={(e) => { setJoinPrompt({ ...joinPrompt, code: e.target.value }); setJoinError(""); }}
                      onKeyDown={(e) => { if (e.key === "Enter") handleJoinWithCode(); if (e.key === "Escape") setJoinPrompt(null); }}
                    />
                    <button style={styles.codeSubmit} onClick={handleJoinWithCode}>go</button>
                    <button style={styles.codeCancel} onClick={() => setJoinPrompt(null)}>✕</button>
                    {joinError && <span style={styles.codeError}>{joinError}</span>}
                  </div>
                )}
              </div>
              <button
                style={full ? styles.joinButtonDisabled : styles.joinButton}
                disabled={full}
                onClick={() => handleJoinClick(room)}
                aria-label={full ? `Room ${room.name} is full` : `Join room ${room.name}`}
              >
                {full ? "full" : "join"}
              </button>
            </div>
          );
        })}
      </div>

      {error && <div style={styles.error}>{error}</div>}

      <div style={styles.footer}>
        <button style={styles.footerButton} onClick={() => setShowSettings(true)}>
          settings
        </button>
        <button style={styles.footerButton} onClick={onProfile}>
          my profile
        </button>
        <button style={styles.footerButton} onClick={onLogout}>
          logout
        </button>
      </div>

      {showSettings && (
        <SettingsMenu onBack={() => setShowSettings(false)} />
      )}

      {showCreateModal && (
        <CreateRoomModal
          onConfirm={async (settings) => {
            setShowCreateModal(false);
            try {
              const room = await createRoom(token, settings);
              onJoinRoom(room.id, settings.joinCode);
            } catch {
              setError("failed to create room");
            }
          }}
          onCancel={() => setShowCreateModal(false)}
        />
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
    maxWidth: "400px",
  },
  roomCard: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "flex-start",
    padding: "12px 16px",
    background: "#222",
    border: "1px solid #333",
  },
  roomInfo: {
    display: "flex",
    flexDirection: "column",
    gap: "4px",
    flex: 1,
  },
  roomNameRow: {
    display: "flex",
    alignItems: "center",
    gap: "6px",
  },
  lockIcon: {
    fontSize: "12px",
  },
  roomName: {
    fontSize: "14px",
    color: "#fff",
  },
  tagRow: {
    display: "flex",
    alignItems: "center",
    gap: "8px",
    flexWrap: "wrap",
  },
  tag: {
    fontSize: "11px",
    color: "#888",
    background: "#333",
    padding: "1px 6px",
  },
  playerCount: {
    fontSize: "12px",
    color: "#999",
  },
  playerCountFull: {
    fontSize: "12px",
    color: "#e74c3c",
  },
  codePrompt: {
    display: "flex",
    alignItems: "center",
    gap: "6px",
    flexWrap: "wrap",
    marginTop: "6px",
  },
  codeInput: {
    padding: "4px 8px",
    background: "#111",
    border: "1px solid #555",
    color: "#fff",
    fontFamily: "monospace",
    fontSize: "13px",
    width: "140px",
  },
  codeSubmit: {
    padding: "4px 10px",
    background: "#fff",
    color: "#111",
    border: "none",
    fontFamily: "monospace",
    fontSize: "13px",
    fontWeight: "bold",
    cursor: "pointer",
  },
  codeCancel: {
    padding: "4px 8px",
    background: "none",
    border: "1px solid #444",
    color: "#888",
    fontFamily: "monospace",
    fontSize: "13px",
    cursor: "pointer",
  },
  codeError: {
    fontSize: "11px",
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
    alignSelf: "flex-start",
  },
  joinButtonDisabled: {
    padding: "6px 16px",
    background: "#333",
    color: "#666",
    border: "none",
    fontSize: "13px",
    fontFamily: "monospace",
    cursor: "not-allowed",
    alignSelf: "flex-start",
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
    color: "#666",
    cursor: "pointer",
    fontSize: "13px",
    fontFamily: "monospace",
    padding: 0,
  },
};
