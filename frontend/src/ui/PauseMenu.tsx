interface Props {
  onResume: () => void;
  onProfile?: () => void;
  onLeave: () => void;
}

export default function PauseMenu({ onResume, onProfile, onLeave }: Props) {
  return (
      <div style={styles.overlay} onClick={onResume} role={"dialog"} aria-modal aria-labelledby="pause-title">
      <div style={styles.box} onClick={(e) => e.stopPropagation()}>
          <h2 id="pause-title" style={styles.title}>menu</h2>
        <button onClick={onResume} style={styles.btn}>resume</button>
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
  },
  title: {
    fontSize: 32,
    marginBottom: 8,
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
