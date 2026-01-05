using DotaPicker.Application;
using DotaPicker.Domain;
using DotaPicker.Infrastructure;
using Spectre.Console;

IOpenDotaClient client = new OpenDotaClient();
var service = new CounterPickService(client);

AnsiConsole.Write(new FigletText("Dota 2 Picker").Color(Color.Red));

await AnsiConsole.Status()
    .StartAsync("Loading hero data...", async ctx =>
    {
        await service.InitializeAsync();
    });

var enemyHeroes = new List<Hero>();

while (enemyHeroes.Count < 5)
{
    var heroName = AnsiConsole.Ask<string>($"[bold yellow]Enemy #{enemyHeroes.Count + 1}[/] (or 'go'):");
    
    if (heroName.ToLower() == "go" && enemyHeroes.Count > 0) break;

    var foundHeroes = service.SearchHeroes(heroName);

    if (foundHeroes.Count == 0)
    {
        AnsiConsole.MarkupLine("[red]Hero not found![/]");
        continue;
    }

    var selectedHero = foundHeroes.First();
    if (foundHeroes.Count > 1)
    {
        selectedHero = AnsiConsole.Prompt(
            new SelectionPrompt<Hero>()
                .Title("Select hero:")
                .PageSize(10)
                .AddChoices(foundHeroes)
                .UseConverter(h => h.Name));
    }

    if (enemyHeroes.Any(h => h.Id == selectedHero.Id))
    {
        AnsiConsole.MarkupLine("[yellow]Already added![/]");
        continue;
    }

    enemyHeroes.Add(selectedHero);
    AnsiConsole.MarkupLine($"Added: [green]{selectedHero.Name}[/]");
}

if (enemyHeroes.Count > 0)
{
    await AnsiConsole.Status()
        .StartAsync("Analyzing matchups...", async ctx =>
        {
            var recommendations = await service.GetCounterPicksAsync(enemyHeroes);

            var table = new Table();
            table.AddColumn("Hero");
            table.AddColumn("Win Rate");
            table.AddColumn("Reason");

            foreach (var (hero, score, reason) in recommendations)
            {
                var color = score > 0.52 ? "green" : "yellow";
                table.AddRow(hero.Name, $"[{color}]{score:P1}[/]", reason);
            }

            AnsiConsole.Write(table);
        });
}

Console.WriteLine("\nDone. Press Enter to exit.");
Console.ReadLine();
