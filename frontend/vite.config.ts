import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

const backendOrigin = process.env.BACKEND_ORIGIN ?? "http://172.20.10.2:8080";
const backendWsOrigin = process.env.BACKEND_WS_ORIGIN ?? backendOrigin.replace(/^http/, "ws");

export default defineConfig({
  plugins: [react()],
  server: {
    host: "0.0.0.0",
    port: 5173,
    allowedHosts: true,
    proxy: {
      "/api": {
        target: backendOrigin,
        changeOrigin: true,
      },
      "/ws": {
        target: backendWsOrigin,
        ws: true,
      },
    },
  },
});
