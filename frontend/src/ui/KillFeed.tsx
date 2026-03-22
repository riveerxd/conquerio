import {useEffect, useRef, useState} from "react";
import type {NetworkClient} from "../game/NetworkClient";
import type {KillFeedMessage} from "../game/types";

interface Props {
    networkClient: NetworkClient;
}

interface Entry {
    id: number;
    text: string;
    createdAt: number;
}

const FADE_AFTER_MS = 5000;
let nextId = 0;

function buildText(msg: KillFeedMessage): string {
    if (msg.killerName) return `${msg.killerName} killed ${msg.victimName}`;
    if (msg.reason === "self") return `${msg.victimName} hit their own trail`;
    return `${msg.victimName} died (${msg.reason})`;
}

export default function KillFeed({ networkClient }: Props) {
    const [entries, setEntries] = useState<Entry[]>([]);
    const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

    useEffect(() => {
        networkClient.onKillFeed((msg) => {
            const entry: Entry = {
                id: nextId++,
                text: buildText(msg),
                createdAt: Date.now(),
            };
            setEntries((prev) => [...prev.slice(-9), entry]);
        });

        timerRef.current = setInterval(() => {
            const cutoff = Date.now() - FADE_AFTER_MS;
            setEntries((prev) => prev.filter((e) => e.createdAt > cutoff));
        }, 500);

        return () => {
            if (timerRef.current) clearInterval(timerRef.current);
        };
    }, [networkClient]);

    if (entries.length === 0) return null;

    return (
        <div style={{
            position: "absolute",
            bottom: 16,
            right: 16,
            display: "flex",
            flexDirection: "column",
            alignItems: "flex-end",
            gap: 4,
            pointerEvents: "none",
        }} role={"log"} aria-live={"polite"}>
            {entries.map((entry) => {
                const age = Date.now() - entry.createdAt;
                const opacity = age > FADE_AFTER_MS * 0.7
                    ? 1 - (age - FADE_AFTER_MS * 0.7) / (FADE_AFTER_MS * 0.3)
                    : 1;
                return (
                    <div
                        key={entry.id}
                        style={{
                            opacity: Math.max(0, opacity),
                            transition: "opacity 0.5s",
                            background: "rgba(0,0,0,0.55)",
                            color: "#fff",
                            fontSize: 13,
                            fontFamily: "monospace",
                            padding: "3px 8px",
                            borderRadius: 4,
                            whiteSpace: "nowrap",
                        }}
                    >
                        {entry.text}
                    </div>
                );
            })}
        </div>
    );
}
