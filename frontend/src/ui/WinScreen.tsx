interface Props {
  winnerName: string;
  isLocalWinner: boolean;
  onPlay: () => void;
  onProfile?: () => void;
}

export default function WinScreen({ winnerName, isLocalWinner, onPlay, onProfile }: Props) {
  return (
    <div style={styles.overlay} role="dialog" aria-modal aria-labelledby="win-title">
      <div style={styles.box}>
        <h2 id="win-title" style={styles.title}>
          {isLocalWinner ? "you won!" : `${winnerName} won`}
        </h2>
        <p style={styles.sub}>
          {isLocalWinner ? "you filled the entire map" : "they filled the entire map"}
        </p>
        <button onClick={onPlay} style={styles.button}>
          play again
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
    background: "rgba(0,0,0,0.7)",
    zIndex: 10,
  },
  box: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    textAlign: "center",
    color: "#fff",
    fontFamily: "monospace",
  },
  title: {
    fontSize: "36px",
    marginBottom: "12px",
  },
  sub: {
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
