using DotaPicker.Domain;

namespace DotaPicker.Application;

public class CounterPickService
{
    private readonly IOpenDotaClient _apiClient;
    private List<Hero> _heroesCache = new();

    public CounterPickService(IOpenDotaClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task InitializeAsync()
    {
        _heroesCache = await _apiClient.GetHeroesAsync();
    }

    public List<Hero> SearchHeroes(string query)
    {
        return _heroesCache
            .Where(h => h.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
    
    public async Task<List<(Hero Hero, double Score, string Reason)>> GetCounterPicksAsync(List<Hero> enemyHeroes)
    {
        var totalWinRates = new Dictionary<int, double>();
        var matchCounts = new Dictionary<int, int>();

        foreach (var enemy in enemyHeroes)
        {
            await Task.Delay(100); 
            var matchups = await _apiClient.GetMatchupsAsync(enemy.Id);
            foreach (var matchup in matchups.Where(m => m.GamesPlayed >= 50))
            {
                if (!totalWinRates.ContainsKey(matchup.HeroId)) totalWinRates[matchup.HeroId] = 0;
                totalWinRates[matchup.HeroId] += matchup.WinRate;
                
                if (!matchCounts.ContainsKey(matchup.HeroId)) matchCounts[matchup.HeroId] = 0;
                matchCounts[matchup.HeroId]++;
            }
        }

        var result = new List<(Hero, double, string)>();
        int totalEnemies = enemyHeroes.Count;

        foreach (var (heroId, sumWinRate) in totalWinRates)
        {
            if (enemyHeroes.Any(e => e.Id == heroId)) continue;
            
            var hero = _heroesCache.FirstOrDefault(h => h.Id == heroId);
            if (hero == null) continue;

            int enemiesCounted = matchCounts[heroId];
            
            int requiredMatches = totalEnemies <= 1 ? 1 : (int)Math.Ceiling(totalEnemies / 2.0);
            
            if (enemiesCounted < requiredMatches) continue;

            double avgWinRate = sumWinRate / enemiesCounted;

            result.Add((hero, avgWinRate, $"Avg WinRate vs {enemiesCounted}/{totalEnemies} enemies"));
        }

        return result
            .OrderByDescending(x => x.Item2)
            .Take(10)
            .ToList();
    }
}
