import http from "k6/http";
import ws from "k6/ws";
import { check, sleep } from "k6";
import { Rate, Trend } from "k6/metrics";

const BASE_URL = __ENV.BASE_URL || "http://localhost:8080";
const WS_URL = __ENV.WS_URL || "ws://localhost:8080";

const loginErrors = new Rate("login_errors");
const wsConnectTime = new Trend("ws_connect_time", true);

// stages: ramp up to 50 concurrent users, hold, ramp down
export const options = {
  stages: [
    { duration: "30s", target: 20 },
    { duration: "1m",  target: 50 },
    { duration: "30s", target: 0 },
  ],
  thresholds: {
    http_req_failed:        ["rate<0.05"],   // less than 5% HTTP errors
    http_req_duration:      ["p(95)<2000"],  // 95% of requests under 2s
    login_errors:           ["rate<0.05"],
    ws_connect_time:        ["p(95)<3000"],  // 95% of WS connections under 3s
  },
};

// shared pool of pre-registered test accounts
// run setup() once to create them, then reuse across VUs
const TEST_PASSWORD = "loadtest1";

export function setup() {
  const users = [];
  for (let i = 0; i < 60; i++) {
    const username = `loadtest_${Date.now()}_${i}`;
    const email = `${username}@test.invalid`;

    const res = http.post(
      `${BASE_URL}/api/auth/register`,
      JSON.stringify({ username, email, password: TEST_PASSWORD }),
      { headers: { "Content-Type": "application/json" } }
    );

    if (res.status === 200) {
      const loginRes = http.post(
        `${BASE_URL}/api/auth/login`,
        JSON.stringify({ username, password: TEST_PASSWORD }),
        { headers: { "Content-Type": "application/json" } }
      );
      if (loginRes.status === 200) {
        users.push({ username, token: loginRes.json("token") });
      }
    }
  }
  return { users };
}

export default function (data) {
  const user = data.users[__VU % data.users.length];

  // --- HTTP: health check ---
  const health = http.get(`${BASE_URL}/api/health`);
  check(health, { "health ok": (r) => r.status === 200 });

  // --- HTTP: list rooms ---
  const rooms = http.get(`${BASE_URL}/api/rooms`, {
    headers: { Authorization: `Bearer ${user.token}` },
  });
  check(rooms, { "rooms ok": (r) => r.status === 200 });

  // --- HTTP: leaderboard ---
  const lb = http.get(`${BASE_URL}/api/leaderboard`);
  check(lb, { "leaderboard ok": (r) => r.status === 200 });

  // --- WebSocket: connect to game and send a few moves ---
  const start = Date.now();
  const wsRes = ws.connect(
    `${WS_URL}/ws/game?token=${user.token}`,
    {},
    function (socket) {
      wsConnectTime.add(Date.now() - start);

      socket.on("open", () => {
        // send a few direction inputs
        const dirs = ["up", "right", "down", "left"];
        let i = 0;
        const interval = socket.setInterval(() => {
          socket.send(JSON.stringify({ type: "input", direction: dirs[i % 4] }));
          i++;
          if (i >= 8) {
            socket.clearInterval(interval);
            socket.close();
          }
        }, 200);
      });

      socket.on("error", (e) => {
        console.error(`ws error for ${user.username}: ${e}`);
      });
    }
  );

  check(wsRes, { "ws connected": (r) => r && r.status === 101 });

  sleep(1);
}
