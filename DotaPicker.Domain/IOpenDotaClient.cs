namespace DotaPicker.Domain;

public interface IOpenDotaClient
{
    Task<List<Hero>> GetHeroesAsync();
    Task<List<Matchup>> GetMatchupsAsync(int heroId);
}