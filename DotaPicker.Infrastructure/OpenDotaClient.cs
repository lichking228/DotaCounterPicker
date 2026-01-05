using System.Net.Http.Json;
using DotaPicker.Domain;

namespace DotaPicker.Infrastructure;

public class OpenDotaClient : IOpenDotaClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.opendota.com/api";

    public OpenDotaClient()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "DotaCounterPicker/1.0");
    }

    public async Task<List<Hero>> GetHeroesAsync()
    {
        try 
        {
            return await _httpClient.GetFromJsonAsync<List<Hero>>($"{BaseUrl}/heroes") 
                   ?? new List<Hero>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке героев: {ex.Message}");
            return new List<Hero>();
        }
    }

    public async Task<List<Matchup>> GetMatchupsAsync(int heroId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Matchup>>($"{BaseUrl}/heroes/{heroId}/matchups") 
                   ?? new List<Matchup>();
        }
        catch (Exception)
        {
            return new List<Matchup>();
        }
    }
}