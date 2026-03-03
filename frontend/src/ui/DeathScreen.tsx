interface Props {
  reason: string;
  killedBy: string | null;
  onRespawn: () => void;
}

export default function DeathScreen({ reason, killedBy, onRespawn }: Props) {
  return (
    <div style={styles.overlay}>
      <div style={styles.box}>
        <h2 style={styles.title}>you died</h2>
        <p style={styles.reason}>
          {killedBy ? `killed by ${killedBy}` : reason}
        </p>
        <button onClick={onRespawn} style={styles.button}>
          respawn
        </button>
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
    background: "rgba(0,0,0,0.7)",
    zIndex: 10,
  },
  box: {
    textAlign: "center",
    color: "#fff",
    fontFamily: "monospace",
  },
  title: {
    fontSize: "36px",
    marginBottom: "12px",
  },
  reason: {
    color: "#999",
    marginBottom: "24px",
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
};
