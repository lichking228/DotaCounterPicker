using DotaPicker.Application;
using DotaPicker.Domain;
using NSubstitute;
using Xunit;

namespace DotaPicker.Tests;

public class CounterServiceTests
{
    [Fact]
    public async Task GetCounterPicks_ShouldRecommendHero_WithHighWinRate()
    {
        /*
           Сценарий:
           1. Враг - Axe.
           2. API сообщает, что Viper имеет 60% винрейта против Axe, а Crystal Maiden - 40%.
           3. Сервис должен вернуть Viper как лучшую рекомендацию.
        */

        // Arrange
        var mockClient = Substitute.For<IOpenDotaClient>();
        
        var enemyAxe = new Hero(Id: 1, Name: "Axe", PrimaryAttr: "str", AttackType: "Melee", Roles: new[] { "Offlane" });
        var heroViper = new Hero(Id: 2, Name: "Viper", PrimaryAttr: "agi", AttackType: "Ranged", Roles: new[] { "Carry" });
        var heroCM = new Hero(Id: 3, Name: "Crystal Maiden", PrimaryAttr: "int", AttackType: "Ranged", Roles: new[] { "Support" });

        mockClient.GetHeroesAsync().Returns(Task.FromResult(new List<Hero> { enemyAxe, heroViper, heroCM }));

        // Viper (60% wins), CM (40% wins) vs Axe
        mockClient.GetMatchupsAsync(enemyAxe.Id).Returns(Task.FromResult(new List<Matchup>
        {
            new Matchup(HeroId: heroViper.Id, GamesPlayed: 100, Wins: 60),
            new Matchup(HeroId: heroCM.Id, GamesPlayed: 100, Wins: 40)
        }));

        var service = new CounterPickService(mockClient);
        await service.InitializeAsync();

        // Act
        var result = await service.GetCounterPicksAsync(new List<Hero> { enemyAxe });

        // Assert
        Assert.NotEmpty(result);
        var bestPick = result.First();
        
        Assert.Equal("Viper", bestPick.Hero.Name);
        Assert.True(bestPick.Score > 0, "Score should be positive (advantage)");
    }
}