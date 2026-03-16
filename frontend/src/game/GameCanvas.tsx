import { useRef, useEffect, useState, useCallback } from "react";
import { NetworkClient } from "./NetworkClient";
import { GameLoop } from "./GameLoop";
import { InputHandler } from "./InputHandler";
import DeathScreen from "../ui/DeathScreen";
import Leaderboard from "../ui/Leaderboard";
import KillFeed from "../ui/KillFeed";

interface Props {
  token: string;
  roomId?: string;
  onDisconnect: () => void;
  onProfile?: () => void;
}

interface DeathInfo {
  reason: string;
  killedBy: string | null;
}

export default function GameCanvas({ token, roomId, onDisconnect, onProfile }: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [death, setDeath] = useState<DeathInfo | null>(null);
  const [networkClient, setNetworkClient] = useState<NetworkClient | null>(null);

  // Bumping this key tears down the entire effect and reconnects
  const [sessionKey, setSessionKey] = useState(0);

  const handleRespawn = useCallback(() => {
    setDeath(null);
    setSessionKey((k) => k + 1);
  }, []);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const resize = () => {
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;
    };
    resize();
    window.addEventListener("resize", resize);

    const network = new NetworkClient();
    const gameLoop = new GameLoop(canvas, network);
    const input = new InputHandler(network);

    network.onJoined(() => {
      console.log("joined game");
      gameLoop.start();
      setNetworkClient(network);
    });

    network.onDeath((msg) => {
      gameLoop.stop();
      setDeath({ reason: msg.reason, killedBy: msg.killedBy });
    });

    let intentionalDisconnect = false;
    network.onDisconnect(() => {
      if (!intentionalDisconnect) onDisconnect();
    });

    network.connect(token, roomId);

    return () => {
      intentionalDisconnect = true;
      window.removeEventListener("resize", resize);
      input.destroy();
      gameLoop.stop();
      network.disconnect();
      setNetworkClient(null);
    };
  }, [token, roomId, onDisconnect, sessionKey]);

  return (
    <div style={{ position: "relative", width: "100vw", height: "100vh", overflow: "hidden" }}>
      <canvas ref={canvasRef} style={{ display: "block" }} />
      {networkClient && (
        <Leaderboard networkClient={networkClient} />
      )}
      {networkClient && (
        <KillFeed networkClient={networkClient} />
      )}
      {death && (
        <DeathScreen
          reason={death.reason}
          killedBy={death.killedBy}
          onRespawn={handleRespawn}
          onProfile={onProfile}
        />
      )}
    </div>
  );
}
