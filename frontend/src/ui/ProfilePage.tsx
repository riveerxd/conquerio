import { useEffect, useState } from "react";
import { fetchStats, type PlayerStats } from "../api/stats";

interface Props {
    userId: string;
    onBack: () => void;
}

export default function ProfilePage({ userId, onBack }: Props) {
    const [stats, setStats] = useState<PlayerStats | null>(null);
    const [error, setError] = useState("");

    useEffect(() => {
        fetchStats(userId)
            .then(setStats)
            .catch(() => setError("could not load stats"));
    }, [userId]);

    const kd =
        stats && stats.totalDeaths > 0
            ? (stats.totalKills / stats.totalDeaths).toFixed(2)
            : stats
            ? stats.totalKills.toFixed(2)
            : "—";

    return (
        <div style={styles.container}>
            <div style={styles.card}>
                <h1 style={styles.title}>
                    {stats ? stats.username : "profile"}
                </h1>

                {error && <p style={styles.error}>{error}</p>}

                {!stats && !error && <p style={styles.muted}>loading...</p>}

                {stats && (
                    <div style={styles.grid}>
                        <StatRow label="elo" value={stats.elo} />
                        <StatRow label="games" value={stats.totalGames} />
                        <StatRow label="kills" value={stats.totalKills} />
                        <StatRow label="deaths" value={stats.totalDeaths} />
                        <StatRow label="k/d" value={kd} />
                        <StatRow
                            label="best territory"
                            value={`${stats.bestTerritoryPct.toFixed(1)}%`}
                        />
                    </div>
                )}

                <button style={styles.backButton} onClick={onBack}>
                    ← back
                </button>
            </div>
        </div>
    );
}

function StatRow({ label, value }: { label: string; value: string | number }) {
    return (
        <div style={styles.row}>
            <span style={styles.label}>{label}</span>
            <span style={styles.value}>{value}</span>
        </div>
    );
}

const styles: Record<string, React.CSSProperties> = {
    container: {
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "100vh",
        background: "#111",
        fontFamily: "monospace",
    },
    card: {
        width: "100%",
        maxWidth: "360px",
        padding: "40px 32px",
        background: "#1a1a1a",
        border: "1px solid #333",
        color: "#fff",
    },
    title: {
        fontSize: "28px",
        marginBottom: "32px",
        letterSpacing: "2px",
        fontWeight: "bold",
    },
    grid: {
        display: "flex",
        flexDirection: "column",
        gap: "12px",
        marginBottom: "32px",
    },
    row: {
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        borderBottom: "1px solid #222",
        paddingBottom: "10px",
    },
    label: {
        color: "#666",
        fontSize: "13px",
    },
    value: {
        fontSize: "15px",
        fontWeight: "bold",
        color: "#fff",
    },
    muted: {
        color: "#666",
        fontSize: "13px",
        marginBottom: "24px",
    },
    error: {
        color: "#e74c3c",
        fontSize: "13px",
        marginBottom: "24px",
    },
    backButton: {
        background: "none",
        border: "none",
        color: "#666",
        cursor: "pointer",
        fontSize: "13px",
        fontFamily: "monospace",
        padding: 0,
    },
};
