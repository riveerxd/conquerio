# Load Test

Uses [k6](https://k6.io) to simulate concurrent players hitting the API and WebSocket.

## What it tests

- `/api/health` - basic availability
- `/api/rooms` - room list under load
- `/api/leaderboard` - leaderboard under load
- `/ws/game` - WebSocket game connections with movement inputs

## Thresholds

| Metric | Threshold |
|--------|-----------|
| HTTP error rate | < 5% |
| HTTP p95 latency | < 2s |
| WS connection p95 | < 3s |
| Login error rate | < 5% |

## Running

```bash
# install k6: https://k6.io/docs/get-started/installation/

# against local stack
k6 run docs/load-test/load-test.js

# against prod
k6 run -e BASE_URL=https://conquerio.riveer.cz -e WS_URL=wss://conquerio.riveer.cz docs/load-test/load-test.js
```

## Load profile

- Ramp up to 20 VUs over 30s
- Hold at 50 VUs for 1 minute
- Ramp down over 30s
