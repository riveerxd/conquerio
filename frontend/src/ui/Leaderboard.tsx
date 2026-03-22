import {useEffect, useRef, useState} from "react";
import type {NetworkClient} from "../game/NetworkClient";
import type {GameState} from "../game/types";
import {getColor} from "../game/colors";
import {fetchLeaderboard, type LeaderboardEntry} from "../api/leaderboard";
import {useSettings} from "./SettingsContext";

interface Props {
    networkClient: NetworkClient;
}

interface LiveRow {
    colorId: number;
    playerId: string;
    territoryPct: number;
    username: string | null;
    elo: number | null;
    rank: number;
}

function computeTerritoryPct(state: GameState): Map<number, number> {
    const counts = new Map<number, number>();
    const total = state.grid.length;

    for (let i = 0; i < total; i++) {
        const c = state.grid[i];
        if (c === 0) continue;
        counts.set(c, (counts.get(c) ?? 0) + 1);
    }

    const pctMap = new Map<number, number>();
    const denom = total > 0 ? total : 1;
    for (const [colorId, count] of counts) {
        pctMap.set(colorId, (count / denom) * 100);
    }
    return pctMap;
}

/** Build a userId → LeaderboardEntry lookup for O(1) matching. */
function buildEloMap(entries: LeaderboardEntry[]): Map<string, LeaderboardEntry> {
    const map = new Map<string, LeaderboardEntry>();
    for (const e of entries) {
        map.set(e.userId, e);
    }
    return map;
}

const DISPLAY_LIMIT = 8;
const RENDER_INTERVAL_MS = 500; // ~2 Hz

export default function Leaderboard({ networkClient }: Props) {
    const [rows, setRows] = useState<LiveRow[]>([]);
    const [myColorId, setMyColorId] = useState<number | null>(null);
    const eloDataRef = useRef<LeaderboardEntry[]>([]);
    const lastRenderRef = useRef<number>(0);
    const { settings } = useSettings();

    // Fetch Elo data every 30 s
    useEffect(() => {
        let cancelled = false;
        const load = async () => {
            try {
                const data = await fetchLeaderboard(DISPLAY_LIMIT);
                if (!cancelled) eloDataRef.current = data;
            } catch {
                // silently ignore – Elo is supplementary
            }
        };
        load();
        const id = setInterval(load, 30_000);
        return () => {
            cancelled = true;
            clearInterval(id);
        };
    }, []);

    // Subscribe to game state ticks
    useEffect(() => {
        const handler = (state: GameState) => {
            // Throttle React state updates to ~2 Hz
            const now = performance.now();
            if (now - lastRenderRef.current < RENDER_INTERVAL_MS) return;
            lastRenderRef.current = now;

            // Discover our own colorId from myPlayerId
            const me = state.players.find((p) => p.id === state.myPlayerId);
            if (me) setMyColorId(me.colorId);

            const pctMap = computeTerritoryPct(state);
            const eloMap = buildEloMap(eloDataRef.current);

            // Build one row per alive player, matching Elo by userId
            const livePlayers = state.players.filter((p) => p.alive);

            const built: LiveRow[] = livePlayers.map((p) => {
                const eloEntry = eloMap.get(p.id) ?? null;
                return {
                    colorId: p.colorId,
                    playerId: p.id,
                    territoryPct: pctMap.get(p.colorId) ?? 0,
                    username: eloEntry?.username ?? null,
                    elo: eloEntry?.elo ?? null,
                    rank: 0,
                };
            });

            // Sort by territory %
            built.sort((a, b) => b.territoryPct - a.territoryPct);
            built.forEach((r, i) => {
                r.rank = i + 1;
            });

            setRows(built);
        };

        networkClient.onStateUpdate(handler);
        return () => {
            networkClient.offStateUpdate();
        };
    }, [networkClient]);

    if (rows.length === 0) return null;

    return (
        <div style={styles.container} role={"region"} aria-labelledby={"leaderboard-heading"}>
            <h2 id={"leaderboard-heading"} style={styles.header}>leaderboard</h2>
            {rows.slice(0, DISPLAY_LIMIT).map((row) => {
                const isMe = row.colorId === myColorId;
                return (
                    <div key={row.playerId} style={{ ...styles.row, ...(isMe ? styles.myRow : {}) }}>
                        <span style={styles.rank}>#{row.rank}</span>
                        <span
                            style={{
                                ...styles.colorDot,
                                background: getColor(row.colorId, settings.colorblindMode),
                            }}
                        />
                        <span style={styles.name}>
                            {isMe ? "You" : (row.username ?? row.playerId.slice(0, 6))}
                        </span>
                        <span style={styles.pct}>{row.territoryPct.toFixed(1)}%</span>
                        {row.elo !== null && (
                            <span style={styles.elo}>{row.elo} elo</span>
                        )}
                    </div>
                );
            })}
        </div>
    );
}

const styles: Record<string, React.CSSProperties> = {
    container: {
        position: "absolute",
        top: 16,
        left: 16,
        minWidth: 210,
        background: "rgba(10, 10, 18, 0.72)",
        backdropFilter: "blur(10px)",
        WebkitBackdropFilter: "blur(10px)",
        border: "1px solid rgba(255, 255, 255, 0.10)",
        borderRadius: 10,
        padding: "10px 14px",
        fontFamily: "'Inter', 'Segoe UI', sans-serif",
        fontSize: 13,
        color: "#e0e0e0",
        pointerEvents: "none",
        userSelect: "none",
        zIndex: 10,
        boxShadow: "0 4px 24px rgba(0,0,0,0.5)",
    },
    header: {
        fontWeight: 700,
        fontSize: 12,
        letterSpacing: "0.08em",
        textTransform: "uppercase",
        color: "#a0a0b0",
        marginBottom: 8,
    },
    row: {
        display: "flex",
        alignItems: "center",
        gap: 6,
        padding: "3px 0",
        borderRadius: 5,
        transition: "background 0.15s",
    },
    myRow: {
        background: "rgba(255,255,255,0.07)",
        paddingLeft: 4,
        paddingRight: 4,
        fontWeight: 600,
        color: "#ffffff",
    },
    rank: {
        width: 24,
        color: "#707080",
        fontSize: 11,
        textAlign: "right",
        flexShrink: 0,
    },
    colorDot: {
        width: 8,
        height: 8,
        borderRadius: "50%",
        flexShrink: 0,
    },
    name: {
        flex: 1,
        overflow: "hidden",
        textOverflow: "ellipsis",
        whiteSpace: "nowrap",
    },
    pct: {
        color: "#7dd3fc",
        fontVariantNumeric: "tabular-nums",
        minWidth: 38,
        textAlign: "right",
    },
    elo: {
        color: "#a78bfa",
        fontSize: 11,
        minWidth: 52,
        textAlign: "right",
        fontVariantNumeric: "tabular-nums",
    },
};
