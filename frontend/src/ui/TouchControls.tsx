import {useEffect, useRef} from "react";
import nipplejs from "nipplejs";
import {NetworkClient} from "../game/NetworkClient";
import {Direction} from "../game/types";

interface Props {
  networkClient: NetworkClient;
  onPause: () => void;
}

export default function TouchControls({ networkClient, onPause }: Props) {
  const joystickRef = useRef<HTMLDivElement>(null);
  const lastDirRef = useRef<Direction | null>(null);

  useEffect(() => {
    if (!joystickRef.current) return;

    const manager = nipplejs.create({
      zone: joystickRef.current,
      mode: "static",
      position: { left: "50%", bottom: "50%" },
      color: "white",
      size: 120,
    });

    manager.on("move", (_, data) => {
      if (!data.direction) return;
      const angle = data.angle.degree;
      let dir: Direction;

      if (angle > 45 && angle <= 135) dir = "up";
      else if (angle > 135 && angle <= 225) dir = "left";
      else if (angle > 225 && angle <= 315) dir = "down";
      else dir = "right";

      if (dir !== lastDirRef.current) {
        lastDirRef.current = dir;
        networkClient.sendInput(dir);
      }
    });

    manager.on("end", () => {
      lastDirRef.current = null;
    });

    return () => {
      manager.destroy();
    };
  }, [networkClient]);

  const handleAbility = (e: React.PointerEvent, ability: string) => {
    e.preventDefault();
    networkClient.sendAbility(ability);
  };

  return (
    <div style={styles.container}>
      {/* Joystick — bottom-left */}
      <div ref={joystickRef} style={styles.joystickZone} />

      {/* Ability buttons — bottom-right */}
      <div style={styles.buttonZone}>
        <div style={styles.abilityItem}>
          <button
            style={styles.abilityBtn}
            onPointerDown={(e) => handleAbility(e, "BOOST")}
            aria-label="Activate Boost"
          >
            <img src="/img/boost.webp" alt="" style={styles.abilityIcon} draggable={false} />
          </button>
          <span style={styles.abilityLabel}>Boost</span>
        </div>
        <div style={styles.abilityItem}>
          <button
            style={styles.abilityBtn}
            onPointerDown={(e) => handleAbility(e, "SHIELD")}
            aria-label="Activate Shield"
          >
            <img src="/img/shield.webp" alt="" style={styles.abilityIcon} draggable={false} />
          </button>
          <span style={styles.abilityLabel}>Shield</span>
        </div>
      </div>

      {/* Pause / menu button — top-center */}
      <button
        style={styles.pauseBtn}
        onPointerDown={(e) => { e.preventDefault(); onPause(); }}
        aria-label="Open menu"
      >
        &#9776;
      </button>
    </div>
  );
}

const SAFE_BOTTOM = "calc(40px + env(safe-area-inset-bottom, 0px))";
const SAFE_TOP = "calc(16px + env(safe-area-inset-top, 0px))";

const styles: Record<string, React.CSSProperties> = {
  container: {
    position: "absolute",
    inset: 0,
    pointerEvents: "none",
    zIndex: 5,
  },
  joystickZone: {
    position: "absolute",
    left: "20px",
    bottom: SAFE_BOTTOM,
    width: "160px",
    height: "160px",
    pointerEvents: "auto",
    touchAction: "none",
  },
  buttonZone: {
    position: "absolute",
    right: "20px",
    bottom: SAFE_BOTTOM,
    display: "flex",
    flexDirection: "row",
    gap: "16px",
    pointerEvents: "auto",
    alignItems: "flex-end",
  },
  abilityItem: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    gap: "4px",
  },
  abilityBtn: {
    width: "72px",
    height: "72px",
    borderRadius: "12px",
    border: "2px solid rgba(255, 255, 255, 0.4)",
    background: "rgba(0, 0, 0, 0.6)",
    cursor: "pointer",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    padding: "6px",
    userSelect: "none",
    WebkitTapHighlightColor: "transparent",
    touchAction: "none",
  },
  abilityIcon: {
    width: "100%",
    height: "100%",
    objectFit: "contain",
    userSelect: "none",
  },
  abilityLabel: {
    color: "#aaa",
    fontSize: "11px",
    fontFamily: "monospace",
    userSelect: "none",
  },
  pauseBtn: {
    position: "absolute",
    top: SAFE_TOP,
    left: "50%",
    transform: "translateX(-50%)",
    width: "44px",
    height: "44px",
    border: "1px solid rgba(255,255,255,0.3)",
    borderRadius: "8px",
    background: "rgba(0,0,0,0.5)",
    color: "#fff",
    fontSize: "20px",
    lineHeight: "1",
    cursor: "pointer",
    pointerEvents: "auto",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    userSelect: "none",
    WebkitTapHighlightColor: "transparent",
    touchAction: "none",
  },
};
