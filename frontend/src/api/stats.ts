export interface PlayerStats {
    userId: string;
    username: string;
    elo: number;
    totalKills: number;
    totalDeaths: number;
    bestTerritoryPct: number;
    totalGames: number;
}

export async function fetchStats(userId: string): Promise<PlayerStats> {
    const res = await fetch(`/api/stats/${encodeURIComponent(userId)}`);
    if (res.status === 404) throw new Error("Stats not found");
    if (!res.ok) throw new Error("Failed to fetch stats");
    return res.json();
}

/** Decode the `sub` (userId) from a JWT without verifying the signature. */
export function getUserIdFromToken(token: string): string | null {
    try {
        const payload = token.split(".")[1];
        const decoded = JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/")));
        // ASP.NET Identity JWT uses the ClaimTypes.NameIdentifier claim
        return (
            decoded["sub"] ??
            decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ??
            null
        );
    } catch {
        return null;
    }
}
