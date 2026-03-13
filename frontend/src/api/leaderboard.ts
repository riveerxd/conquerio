export interface LeaderboardEntry {
    rank: number;
    userId: string;
    username: string;
    elo: number;
    bestTerritoryPct: number;
}

export async function fetchLeaderboard(maxPlayers = 10): Promise<LeaderboardEntry[]> {
    const res = await fetch(`/api/leaderboard?maxPlayers=${maxPlayers}`);
    if (!res.ok) throw new Error("Failed to fetch leaderboard");
    return res.json();
}
