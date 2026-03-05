import { useRef, useEffect } from "react";
import { NetworkClient } from "./NetworkClient";
import { GameLoop } from "./GameLoop";
import { InputHandler } from "./InputHandler";

interface Props {
  token: string;
  roomId?: string;
  onDisconnect: () => void;
}

export default function GameCanvas({ token, roomId, onDisconnect }: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);

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
    };
  }, [token, roomId, onDisconnect]);

  return <canvas ref={canvasRef} style={{ display: "block" }} />;
}
