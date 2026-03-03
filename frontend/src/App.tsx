import { useState } from "react";
import LoginScreen from "./ui/LoginScreen";
import GameCanvas from "./game/GameCanvas";

export default function App() {
  const [token, setToken] = useState<string | null>(null);

  if (!token) {
    return <LoginScreen onLogin={setToken} />;
  }

  return <GameCanvas token={token} onDisconnect={() => setToken(null)} />;
}
