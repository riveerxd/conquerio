import { useRef, useEffect, useState } from "react";
import { NetworkClient } from "./NetworkClient";
import { GameLoop } from "./GameLoop";
import { InputHandler } from "./InputHandler";
import Leaderboard from "../ui/Leaderboard";

interface Props {
  token: string;
  roomId?: string;
  onDisconnect: () => void;
}

export default function GameCanvas({ token, roomId, onDisconnect }: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [networkClient, setNetworkClient] = useState<NetworkClient | null>(null);

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

    network.onDeath(() => {
      // TODO: show death screen
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
  }, [token, roomId, onDisconnect]);

  return (
    <div style={{ position: "relative", width: "100vw", height: "100vh", overflow: "hidden" }}>
      <canvas ref={canvasRef} style={{ display: "block" }} />
      {networkClient && (
        <Leaderboard networkClient={networkClient} token={token} />
      )}
    </div>
  );
}
