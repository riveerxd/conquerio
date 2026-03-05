import { useState } from "react";
import LoginScreen from "./ui/LoginScreen";
import RoomBrowser from "./ui/RoomBrowser";
import GameCanvas from "./game/GameCanvas";

type Screen = "login" | "rooms" | "game";

export default function App() {
  const [screen, setScreen] = useState<Screen>("login");
  const [token, setToken] = useState<string | null>(null);
  const [roomId, setRoomId] = useState<string | null>(null);

  if (screen === "login" || !token) {
    return (
      <LoginScreen
        onLogin={(t) => {
          setToken(t);
          setScreen("rooms");
        }}
      />
    );
  }

  if (screen === "rooms") {
    return (
      <RoomBrowser
        token={token}
        onJoinRoom={(id) => {
          setRoomId(id);
          setScreen("game");
        }}
        onQuickPlay={() => {
          setRoomId(null);
          setScreen("game");
        }}
        onLogout={() => {
          setToken(null);
          setScreen("login");
        }}
      />
    );
  }

  return (
    <GameCanvas
      token={token}
      roomId={roomId ?? undefined}
      onDisconnect={() => setScreen("rooms")}
    />
  );
}
