import React, { useCallback, useEffect, useState } from "react";
import type { CreateRoomSettings } from "../api/rooms";

interface Props {
  onConfirm: (settings: CreateRoomSettings) => void;
  onCancel: () => void;
}

type GridSize = "small" | "medium" | "large";

const GRID_SIZE_LABELS: Record<GridSize, string> = {
  small: "small (100×100)",
  medium: "medium (200×200)",
  large: "large (300×300)",
};

const MAX_PLAYERS_OPTIONS = [5, 10, 20, 50];

export default function CreateRoomModal({ onConfirm, onCancel }: Props) {
  const [name, setName] = useState("");
  const [gridSize, setGridSize] = useState<GridSize>("medium");
  const [maxPlayers, setMaxPlayers] = useState(20);
  const [abilitiesEnabled, setAbilitiesEnabled] = useState(true);
  const [isPrivate, setIsPrivate] = useState(false);
  const [joinCode, setJoinCode] = useState("");
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = useCallback(() => {
    if (isPrivate && joinCode.trim() === "") {
      setError("join code is required for private rooms");
      return;
    }
    onConfirm({
      name: name.trim() || undefined,
      gridSize,
      maxPlayers,
      abilitiesEnabled,
      joinCode: isPrivate ? joinCode.trim() : undefined,
    });
  }, [isPrivate, joinCode, name, gridSize, maxPlayers, abilitiesEnabled, onConfirm]);

  useEffect(() => {
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onCancel();
      if (e.key === "Enter") handleSubmit();
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [onCancel, handleSubmit]);

  return (
    <div style={styles.overlay} onClick={onCancel}>
      <div style={styles.box} onClick={(e) => e.stopPropagation()}>
        <h2 style={styles.title}>create room</h2>

        {error && <div style={styles.errorBanner}>{error}</div>}

        <div style={styles.section}>
          <h3 style={styles.sectionTitle}>name</h3>
          <input
            style={styles.input}
            type="text"
            placeholder="(optional)"
            value={name}
            maxLength={32}
            onChange={(e) => setName(e.target.value)}
          />
        </div>

        <div style={styles.section}>
          <h3 style={styles.sectionTitle}>grid size</h3>
          <div style={styles.segGroup}>
            {(["small", "medium", "large"] as GridSize[]).map((s) => (
              <button
                key={s}
                style={{ ...styles.segBtn, ...(gridSize === s ? styles.segBtnActive : {}) }}
                onClick={() => setGridSize(s)}
              >
                {GRID_SIZE_LABELS[s]}
              </button>
            ))}
          </div>
        </div>

        <div style={styles.section}>
          <h3 style={styles.sectionTitle}>max players</h3>
          <div style={styles.segGroup}>
            {MAX_PLAYERS_OPTIONS.map((n) => (
              <button
                key={n}
                style={{ ...styles.segBtn, ...(maxPlayers === n ? styles.segBtnActive : {}) }}
                onClick={() => setMaxPlayers(n)}
              >
                {n}
              </button>
            ))}
          </div>
        </div>

        <div style={styles.section}>
          <h3 style={styles.sectionTitle}>abilities</h3>
          <div style={styles.segGroup}>
            <button
              style={{ ...styles.segBtn, ...(abilitiesEnabled ? styles.segBtnActive : {}) }}
              onClick={() => setAbilitiesEnabled(true)}
            >
              enabled
            </button>
            <button
              style={{ ...styles.segBtn, ...(!abilitiesEnabled ? styles.segBtnActive : {}) }}
              onClick={() => setAbilitiesEnabled(false)}
            >
              disabled
            </button>
          </div>
        </div>

        <div style={styles.section}>
          <h3 style={styles.sectionTitle}>visibility</h3>
          <div style={styles.segGroup}>
            <button
              style={{ ...styles.segBtn, ...(!isPrivate ? styles.segBtnActive : {}) }}
              onClick={() => { setIsPrivate(false); setError(null); }}
            >
              public
            </button>
            <button
              style={{ ...styles.segBtn, ...(isPrivate ? styles.segBtnActive : {}) }}
              onClick={() => setIsPrivate(true)}
            >
              private
            </button>
          </div>
          {isPrivate && (
            <input
              style={{ ...styles.input, marginTop: "8px" }}
              type="text"
              placeholder="join code"
              value={joinCode}
              maxLength={32}
              autoFocus
              onChange={(e) => { setJoinCode(e.target.value); setError(null); }}
            />
          )}
        </div>

        <div style={styles.footer}>
          <button style={styles.btn} onClick={handleSubmit}>
            create
          </button>
          <button style={{ ...styles.btn, ...styles.cancelBtn }} onClick={onCancel}>
            cancel
          </button>
        </div>
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
    background: "rgba(0,0,0,0.8)",
    zIndex: 20,
    backdropFilter: "blur(4px)",
  },
  box: {
    display: "flex",
    flexDirection: "column",
    width: "400px",
    maxHeight: "90vh",
    overflowY: "auto",
    padding: "32px",
    background: "#111",
    border: "2px solid #333",
    color: "#fff",
    fontFamily: "monospace",
  },
  title: {
    fontSize: 24,
    margin: "0 0 24px 0",
    textAlign: "center",
    letterSpacing: "2px",
    textTransform: "uppercase",
  },
  errorBanner: {
    background: "#ff4444",
    color: "#fff",
    padding: "8px",
    fontSize: "12px",
    marginBottom: "16px",
    textAlign: "center",
  },
  section: {
    marginBottom: "20px",
  },
  sectionTitle: {
    fontSize: 14,
    color: "#888",
    marginBottom: "10px",
    textTransform: "uppercase",
    borderBottom: "1px solid #333",
    paddingBottom: "4px",
  },
  input: {
    width: "100%",
    padding: "8px",
    background: "#222",
    border: "1px solid #444",
    color: "#fff",
    fontFamily: "monospace",
    fontSize: 14,
    boxSizing: "border-box",
  },
  segGroup: {
    display: "flex",
    gap: "8px",
    flexWrap: "wrap",
  },
  segBtn: {
    flex: 1,
    padding: "8px 4px",
    background: "#222",
    border: "1px solid #444",
    color: "#888",
    fontFamily: "monospace",
    fontSize: 12,
    cursor: "pointer",
    textTransform: "lowercase",
  },
  segBtnActive: {
    background: "#fff",
    color: "#111",
    borderColor: "#fff",
  },
  footer: {
    display: "flex",
    flexDirection: "column",
    gap: "10px",
    marginTop: "8px",
  },
  btn: {
    padding: "10px 0",
    background: "#fff",
    color: "#111",
    border: "none",
    fontFamily: "monospace",
    fontSize: 14,
    fontWeight: "bold",
    cursor: "pointer",
    textTransform: "uppercase",
  },
  cancelBtn: {
    background: "none",
    border: "1px solid #555",
    color: "#aaa",
  },
};
