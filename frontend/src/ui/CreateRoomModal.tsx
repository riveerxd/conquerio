import { useState } from "react";
import type { CreateRoomSettings } from "../api/rooms";

interface Props {
  onConfirm: (settings: CreateRoomSettings) => void;
  onClose: () => void;
  loading: boolean;
}

export default function CreateRoomModal({ onConfirm, onClose, loading }: Props) {
  const [name, setName] = useState("");
  const [gridSize, setGridSize] = useState<"small" | "medium" | "large">("medium");
  const [maxPlayers, setMaxPlayers] = useState(20);
  const [abilitiesEnabled, setAbilitiesEnabled] = useState(true);
  const [isPrivate, setIsPrivate] = useState(false);
  const [joinCode, setJoinCode] = useState("");

  const handleSubmit = () => {
    onConfirm({
      name: name.trim() || undefined,
      gridSize,
      maxPlayers,
      abilitiesEnabled,
      joinCode: isPrivate ? joinCode.trim() || undefined : undefined,
    });
  };

  const gridOptions: { value: "small" | "medium" | "large"; label: string; desc: string }[] = [
    { value: "small", label: "small", desc: "100×100" },
    { value: "medium", label: "medium", desc: "200×200" },
    { value: "large", label: "large", desc: "300×300" },
  ];

  return (
    <div style={styles.backdrop} onClick={onClose}>
      <div style={styles.modal} onClick={(e) => e.stopPropagation()}>
        <div style={styles.header}>
          <h2 style={styles.title}>create room</h2>
          <button style={styles.closeBtn} onClick={onClose}>✕</button>
        </div>

        <div style={styles.field}>
          <label style={styles.label}>room name</label>
          <input
            style={styles.input}
            placeholder="optional"
            value={name}
            onChange={(e) => setName(e.target.value)}
            maxLength={40}
          />
        </div>

        <div style={styles.field}>
          <label style={styles.label}>grid size</label>
          <div style={styles.segmented}>
            {gridOptions.map((opt) => (
              <button
                key={opt.value}
                style={gridSize === opt.value ? styles.segActiveBtn : styles.segBtn}
                onClick={() => setGridSize(opt.value)}
              >
                {opt.label}
                <span style={styles.segDesc}>{opt.desc}</span>
              </button>
            ))}
          </div>
        </div>

        <div style={styles.field}>
          <label style={styles.label}>max players</label>
          <div style={styles.segmented}>
            {[5, 10, 20, 50].map((n) => (
              <button
                key={n}
                style={maxPlayers === n ? styles.segActiveBtn : styles.segBtn}
                onClick={() => setMaxPlayers(n)}
              >
                {n}
              </button>
            ))}
          </div>
        </div>

        <div style={styles.field}>
          <label style={styles.label}>abilities</label>
          <div style={styles.segmented}>
            <button
              style={abilitiesEnabled ? styles.segActiveBtn : styles.segBtn}
              onClick={() => setAbilitiesEnabled(true)}
            >
              enabled
            </button>
            <button
              style={!abilitiesEnabled ? styles.segActiveBtn : styles.segBtn}
              onClick={() => setAbilitiesEnabled(false)}
            >
              disabled
            </button>
          </div>
        </div>

        <div style={styles.field}>
          <label style={styles.label}>visibility</label>
          <div style={styles.segmented}>
            <button
              style={!isPrivate ? styles.segActiveBtn : styles.segBtn}
              onClick={() => setIsPrivate(false)}
            >
              public
            </button>
            <button
              style={isPrivate ? styles.segActiveBtn : styles.segBtn}
              onClick={() => setIsPrivate(true)}
            >
              private
            </button>
          </div>
        </div>

        {isPrivate && (
          <div style={styles.field}>
            <label style={styles.label}>join code</label>
            <input
              style={styles.input}
              placeholder="share with friends"
              value={joinCode}
              onChange={(e) => setJoinCode(e.target.value)}
              maxLength={32}
            />
          </div>
        )}

        <button
          style={loading ? styles.submitBtnDisabled : styles.submitBtn}
          onClick={handleSubmit}
          disabled={loading}
        >
          {loading ? "creating..." : "create room"}
        </button>
      </div>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  backdrop: {
    position: "fixed",
    inset: 0,
    background: "rgba(0,0,0,0.7)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    zIndex: 50,
    fontFamily: "monospace",
  },
  modal: {
    background: "#1a1a1a",
    border: "1px solid #333",
    padding: "28px 32px",
    width: "100%",
    maxWidth: "420px",
    display: "flex",
    flexDirection: "column",
    gap: "18px",
    color: "#fff",
  },
  header: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
  },
  title: {
    fontSize: "20px",
    margin: 0,
    letterSpacing: "2px",
  },
  closeBtn: {
    background: "none",
    border: "none",
    color: "#666",
    cursor: "pointer",
    fontSize: "16px",
    fontFamily: "monospace",
    padding: "0 4px",
  },
  field: {
    display: "flex",
    flexDirection: "column",
    gap: "8px",
  },
  label: {
    fontSize: "11px",
    color: "#777",
    textTransform: "uppercase",
    letterSpacing: "1px",
  },
  input: {
    background: "#111",
    border: "1px solid #333",
    color: "#fff",
    fontFamily: "monospace",
    fontSize: "14px",
    padding: "8px 12px",
    outline: "none",
  },
  segmented: {
    display: "flex",
    gap: "6px",
  },
  segBtn: {
    flex: 1,
    padding: "8px 0",
    background: "#111",
    border: "1px solid #333",
    color: "#666",
    fontFamily: "monospace",
    fontSize: "13px",
    cursor: "pointer",
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    gap: "2px",
  },
  segActiveBtn: {
    flex: 1,
    padding: "8px 0",
    background: "#fff",
    border: "1px solid #fff",
    color: "#111",
    fontFamily: "monospace",
    fontSize: "13px",
    cursor: "pointer",
    fontWeight: "bold",
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    gap: "2px",
  },
  segDesc: {
    fontSize: "10px",
    opacity: 0.6,
    fontWeight: "normal",
  },
  submitBtn: {
    width: "100%",
    padding: "12px",
    background: "#fff",
    color: "#111",
    border: "none",
    fontFamily: "monospace",
    fontSize: "15px",
    fontWeight: "bold",
    cursor: "pointer",
    marginTop: "4px",
  },
  submitBtnDisabled: {
    width: "100%",
    padding: "12px",
    background: "#333",
    color: "#666",
    border: "none",
    fontFamily: "monospace",
    fontSize: "15px",
    cursor: "not-allowed",
    marginTop: "4px",
  },
};
