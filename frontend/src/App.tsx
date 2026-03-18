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
  const [colorblindMode, setColorblindMode] = useState(() => {
    return localStorage.getItem("colorblindMode") === "true";
  });

  const userId = token ? getUserIdFromToken(token) : null;

  const handleColorblindToggle = (val: boolean) => {
    setColorblindMode(val);
    localStorage.setItem("colorblindMode", val.toString());
  };

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
        colorblindMode={colorblindMode}
        onColorblindToggle={handleColorblindToggle}
        onJoinRoom={(id) => {
          setRoomId(id);
          setScreen("game");
        }}
        onQuickPlay={() => {
          setRoomId(null);
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
      colorblindMode={colorblindMode}
      onColorblindToggle={handleColorblindToggle}
      onDisconnect={() => setScreen("rooms")}
      onProfile={userId ? () => setScreen("profile") : undefined}
    />
  );
}
