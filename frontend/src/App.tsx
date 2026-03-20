import { useState } from "react";
import LoginScreen from "./ui/LoginScreen";
import RoomBrowser from "./ui/RoomBrowser";
import GameCanvas from "./game/GameCanvas";
import ProfilePage from "./ui/ProfilePage";
import { getUserIdFromToken } from "./api/stats";

type Screen = "login" | "rooms" | "game" | "profile";

export default function App() {
  const [screen, setScreen] = useState<Screen>("login");
  const [token, setToken] = useState<string | null>(null);
  const [roomId, setRoomId] = useState<string | null>(null);
  const [joinCode, setJoinCode] = useState<string | null>(null);
  const [joinError, setJoinError] = useState<string | null>(null);

  const userId = token ? getUserIdFromToken(token) : null;

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
        joinError={joinError}
        onJoinRoom={(id, code) => {
          setRoomId(id);
          setJoinCode(code ?? null);
          setJoinError(null);
          setScreen("game");
        }}
        onQuickPlay={() => {
          setRoomId(null);
          setJoinCode(null);
          setJoinError(null);
          setScreen("game");
        }}
        onProfile={() => setScreen("profile")}
        onLogout={() => {
          setToken(null);
          setScreen("login");
        }}
      />
    );
  }

  if (screen === "profile" && userId) {
    return (
      <ProfilePage
        userId={userId}
        onBack={() => setScreen("rooms")}
      />
    );
  }

  return (
    <GameCanvas
      token={token}
      roomId={roomId ?? undefined}
      joinCode={joinCode ?? undefined}
      onDisconnect={() => { setJoinError(null); setScreen("rooms"); }}
      onConnectFailed={() => { setJoinError("wrong join code"); setScreen("rooms"); }}
      onProfile={userId ? () => setScreen("profile") : undefined}
    />
  );
}
