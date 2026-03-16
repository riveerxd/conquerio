import { useRef, useEffect, useState, useCallback } from "react";
import { NetworkClient } from "./NetworkClient";
import { GameLoop } from "./GameLoop";
import { InputHandler } from "./InputHandler";
import Leaderboard from "../ui/Leaderboard";
import KillFeed from "../ui/KillFeed";
import SpectateOverlay from "../ui/SpectateOverlay";

interface Props {
  token: string;
  roomId?: string;
  onDisconnect: () => void;
  onProfile?: () => void;
}

interface SpectateInfo {
  reason: string;
  killedBy: string | null;
}

export default function GameCanvas({ token, roomId, onDisconnect, onProfile }: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const gameLoopRef = useRef<GameLoop | null>(null);
  const [spectate, setSpectate] = useState<SpectateInfo | null>(null);
  const [spectatedPlayerId, setSpectatedPlayerId] = useState<string | null>(null);
  const [networkClient, setNetworkClient] = useState<NetworkClient | null>(null);

  // keep game loop in sync when spectated player changes
  useEffect(() => {
    if (gameLoopRef.current) {
      gameLoopRef.current.setSpectateTarget(spectatedPlayerId);
    }
  }, [spectatedPlayerId]);

  // Bumping this key tears down the entire effect and reconnects
  const [sessionKey, setSessionKey] = useState(0);

  const handleRespawn = useCallback(() => {
    setSpectate(null);
    setSpectatedPlayerId(null);
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
    gameLoopRef.current = gameLoop;
    const input = new InputHandler(network);

    network.onJoined(() => {
      console.log("joined game");
      gameLoop.start();
      setNetworkClient(network);
    });

    network.onDeath((msg) => {
      // keep game loop running for spectate
      setSpectate({ reason: msg.reason, killedBy: msg.killedBy });
      const state = network.getState();
      const firstAlive = state?.players[0];
      if (firstAlive) {
        setSpectatedPlayerId(firstAlive.id);
        gameLoop.setSpectateTarget(firstAlive.id);
      }
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
      gameLoopRef.current = null;
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
      {spectate && networkClient && (
        <SpectateOverlay
          networkClient={networkClient}
          killedBy={spectate.killedBy}
          reason={spectate.reason}
          spectatedPlayerId={spectatedPlayerId}
          onPlayerChange={setSpectatedPlayerId}
          onRespawn={handleRespawn}
          onProfile={onProfile}
        />
      )}
    </div>
  );
}
