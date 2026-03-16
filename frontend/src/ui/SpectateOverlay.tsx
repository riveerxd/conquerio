import { useEffect, useState, useCallback } from "react";
import type { NetworkClient } from "../game/NetworkClient";

interface Props {
  killedBy: string | null;
  reason: string;
  spectatedPlayerId: string | null;
  onPlayerChange: (id: string) => void;
  onRespawn: () => void;
  onProfile?: () => void;
}

export default function SpectateOverlay({
  killedBy,
  reason,
  spectatedPlayerId,
  onPlayerChange,
  onRespawn,
  onProfile,
  networkClient,
}: Props & { networkClient: NetworkClient }) {
  const [spectatedName, setSpectatedName] = useState<string>("");

  // poll state to get spectated player name + auto-switch if they die
  useEffect(() => {
    const interval = setInterval(() => {
      const state = networkClient.getState();
      if (!state) return;

      const target = state.players.find((p) => p.id === spectatedPlayerId);
      if (target) {
        setSpectatedName(target.username);
      } else if (state.players.length > 0) {
        // spectated player died, switch to next alive
        onPlayerChange(state.players[0].id);
      }
    }, 100);
    return () => clearInterval(interval);
  }, [spectatedPlayerId, networkClient, onPlayerChange]);

  const cyclePlayer = useCallback(
    (dir: 1 | -1) => {
      const state = networkClient.getState();
      if (!state || state.players.length === 0) return;
      const idx = state.players.findIndex((p) => p.id === spectatedPlayerId);
      const next = (idx + dir + state.players.length) % state.players.length;
      onPlayerChange(state.players[next].id);
    },
    [spectatedPlayerId, networkClient, onPlayerChange]
  );

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === "ArrowLeft") cyclePlayer(-1);
      else if (e.key === "ArrowRight") cyclePlayer(1);
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, [cyclePlayer]);

  return (
    <div style={styles.overlay}>
      <div style={styles.box}>
        <h2 style={styles.title}>you died</h2>
        <p style={styles.reason}>
          {killedBy ? `killed by ${killedBy}` : reason}
        </p>

        <div style={styles.spectateRow}>
          <button onClick={() => cyclePlayer(-1)} style={styles.arrowBtn}>{"<"}</button>
          <span style={styles.spectatingLabel}>
            spectating: {spectatedName || "..."}
          </span>
          <button onClick={() => cyclePlayer(1)} style={styles.arrowBtn}>{">"}</button>
        </div>

        <button onClick={onRespawn} style={styles.button}>
          respawn
        </button>
        {onProfile && (
          <button onClick={onProfile} style={styles.statsLink}>
            my stats
          </button>
        )}
      </div>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  overlay: {
    position: "fixed",
    inset: 0,
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    background: "rgba(0,0,0,0.55)",
    zIndex: 10,
    pointerEvents: "none",
  },
  box: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    textAlign: "center",
    color: "#fff",
    fontFamily: "monospace",
    pointerEvents: "all",
  },
  title: {
    fontSize: "36px",
    marginBottom: "12px",
  },
  reason: {
    color: "#999",
    marginBottom: "20px",
  },
  spectateRow: {
    display: "flex",
    alignItems: "center",
    gap: 10,
    marginBottom: 20,
  },
  spectatingLabel: {
    fontSize: 13,
    color: "#aaa",
    minWidth: 160,
    textAlign: "center",
  },
  arrowBtn: {
    background: "none",
    border: "1px solid #555",
    color: "#aaa",
    fontFamily: "monospace",
    fontSize: 16,
    cursor: "pointer",
    padding: "2px 10px",
  },
  button: {
    padding: "12px 32px",
    background: "#fff",
    color: "#111",
    border: "none",
    fontSize: "16px",
    fontFamily: "monospace",
    cursor: "pointer",
    fontWeight: "bold",
  },
  statsLink: {
    display: "block",
    background: "none",
    border: "none",
    color: "#666",
    cursor: "pointer",
    fontSize: "13px",
    fontFamily: "monospace",
    marginTop: "12px",
  },
};
