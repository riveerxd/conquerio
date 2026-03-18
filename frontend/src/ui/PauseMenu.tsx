interface Props {
  onResume: () => void;
  onProfile?: () => void;
  onLeave: () => void;
  colorblindMode: boolean;
  onColorblindToggle: (val: boolean) => void;
}

export default function PauseMenu({
  onResume,
  onProfile,
  onLeave,
  colorblindMode,
  onColorblindToggle,
}: Props) {
  return (
    <div style={styles.overlay} onClick={onResume}>
      <div style={styles.box} onClick={(e) => e.stopPropagation()}>
        <h2 style={styles.title}>menu</h2>
        <button onClick={onResume} style={styles.btn}>resume</button>

        <div style={styles.settingsGroup}>
          <label style={styles.label}>
            <input
              type="checkbox"
              checked={colorblindMode}
              onChange={(e) => onColorblindToggle(e.target.checked)}
            />
            colorblind mode
          </label>
        </div>

        {onProfile && (
          <button onClick={onProfile} style={styles.btn}>my stats</button>
        )}
        <button onClick={onLeave} style={{ ...styles.btn, ...styles.leaveBtn }}>leave game</button>
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
    background: "rgba(0,0,0,0.6)",
    zIndex: 10,
  },
  box: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    gap: 12,
    fontFamily: "monospace",
    color: "#fff",
    background: "#111",
    padding: "32px 48px",
    border: "2px solid #333",
  },
  title: {
    fontSize: 32,
    marginBottom: 8,
  },
  settingsGroup: {
    margin: "8px 0",
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
  },
  label: {
    display: "flex",
    alignItems: "center",
    gap: 8,
    cursor: "pointer",
    fontSize: 14,
    userSelect: "none",
  },
  btn: {
    width: 200,
    padding: "10px 0",
    background: "#fff",
    color: "#111",
    border: "none",
    fontFamily: "monospace",
    fontSize: 15,
    fontWeight: "bold",
    cursor: "pointer",
  },
  leaveBtn: {
    background: "none",
    color: "#888",
    fontWeight: "normal",
    fontSize: 13,
    marginTop: 4,
  },
};
