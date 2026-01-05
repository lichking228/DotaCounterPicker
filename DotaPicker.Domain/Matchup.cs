using System.Text.Json.Serialization;

namespace DotaPicker.Domain;

public record Matchup(
    [property: JsonPropertyName("hero_id")] int HeroId,
    [property: JsonPropertyName("games_played")] int GamesPlayed,
    [property: JsonPropertyName("wins")] int Wins
)
{
    public double WinRate => GamesPlayed > 0 ? (double)Wins / GamesPlayed : 0;
}