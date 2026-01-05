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
        var scores = new Dictionary<int, double>();
        var matchCounts = new Dictionary<int, int>();

        foreach (var enemy in enemyHeroes)
        {
            await Task.Delay(100); 
            
            var matchups = await _apiClient.GetMatchupsAsync(enemy.Id);
            
            foreach (var matchup in matchups.Where(m => m.GamesPlayed > 10))
            {
                if (!scores.ContainsKey(matchup.HeroId)) scores[matchup.HeroId] = 0;
                scores[matchup.HeroId] += matchup.WinRate;
                
                if (!matchCounts.ContainsKey(matchup.HeroId)) matchCounts[matchup.HeroId] = 0;
                matchCounts[matchup.HeroId]++;
            }
        }

        var result = new List<(Hero, double, string)>();

        foreach (var (heroId, totalWinRate) in scores)
        {
            if (enemyHeroes.Any(e => e.Id == heroId)) continue;

            var hero = _heroesCache.FirstOrDefault(h => h.Id == heroId);
            if (hero == null) continue;

            int enemiesCounted = matchCounts[heroId];
            double averageWinRate = totalWinRate / enemiesCounted;

            if (enemiesCounted >= enemyHeroes.Count / 2 && averageWinRate > 0.51)
            {
                result.Add((hero, averageWinRate, $"WinRate {averageWinRate:P1} vs {enemiesCounted} enemies"));
            }
        }

        return result
            .OrderByDescending(x => x.Item2)
            .Take(5)
            .ToList();
    }
}
