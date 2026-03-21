import {useEffect, useRef} from "react";
import nipplejs from "nipplejs";
import {NetworkClient} from "../game/NetworkClient";
import {Direction} from "../game/types";

interface Props {
  networkClient: NetworkClient;
}

export default function TouchControls({ networkClient }: Props) {
  const joystickRef = useRef<HTMLDivElement>(null);
  const lastDirRef = useRef<Direction | null>(null);

  useEffect(() => {
    if (!joystickRef.current) return;

    const manager = nipplejs.create({
      zone: joystickRef.current,
      mode: "static",
      position: {left: "100px", bottom: "100px"},
      color: "white",
      size: 100,
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

  const handleAbility = (ability: string) => {
    networkClient.sendAbility(ability);
  };

  return (
    <div style={styles.container}>
      <div ref={joystickRef} style={styles.joystickZone} />
      <div style={styles.buttonZone}>
        <button
          style={styles.abilityBtn}
          onPointerDown={() => handleAbility("BOOST")}
          aria-label="Activate Boost"
        >
          BOOST
        </button>
        <button
          style={styles.abilityBtn}
          onPointerDown={() => handleAbility("SHIELD")}
          aria-label="Activate Shield"
        >
          SHIELD
        </button>
      </div>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  container: {
    position: "absolute",
    inset: 0,
    pointerEvents: "none",
    zIndex: 5,
  },
  joystickZone: {
    position: "absolute",
    left: 0,
    bottom: 0,
    width: "200px",
    height: "200px",
    pointerEvents: "auto",
  },
  buttonZone: {
    position: "absolute",
    right: "20px",
    bottom: "40px",
    display: "flex",
    flexDirection: "column",
    gap: "20px",
    pointerEvents: "auto",
  },
  abilityBtn: {
    width: "80px",
    height: "80px",
    borderRadius: "50%",
    border: "2px solid rgba(255, 255, 255, 0.5)",
    background: "rgba(0, 0, 0, 0.5)",
    color: "#fff",
    fontSize: "12px",
    fontWeight: "bold",
    fontFamily: "monospace",
    cursor: "pointer",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    userSelect: "none",
    WebkitTapHighlightColor: "transparent",
  },
};
