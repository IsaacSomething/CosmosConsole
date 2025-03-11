using Microsoft.Azure.Cosmos;
using Spectre.Console;
using System.Net;
using CosmosConsole;

class Program
{

    private CosmosClient? client;
    private Database? database;
    private Container? container;

    static async Task Main(string[] args)
    {
        Program program = new();
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        await program.GetStartedDemoAsync();
    }

    public async Task GetStartedDemoAsync()
    {
        AnsiConsole.Markup("[bold white on blue]Cosmos Console application[/] starting... \n");

        try
        {
            AnsiConsole.Markup("[bold white on blue]Initializing client[/]...\n");
            this.client = new CosmosClient(Constants.EndpointUri, Constants.PrimaryKey);

            AnsiConsole.Markup("[bold white on blue]Initializing database[/] creating if it does not exist...\n");
            await CreateDatabase();

            AnsiConsole.Markup("[bold white on blue]Initializing container[/] creating if it does not exist...\n");
            await CreateContainer();
        }
        catch (Exception ex)
        {
            AnsiConsole.Markup($"[bold red]An error has occurred: {ex.Message} [/]\n");
        }
    }

    private async Task CreateDatabase()
    {
        string databaseResult = "";

        await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Creating database...", async ctx =>
                {
                    ctx.Status("[bold white on green]Initializing Database...[/]");
                    var response = await client.CreateDatabaseIfNotExistsAsync(Constants.DatabaseName);
                    databaseResult = SetResultMessage(response.StatusCode, "Database");

                    await Task.Delay(1000);
                    database = response.Database;
                });

        AnsiConsole.Markup($"[bold white on blue]{databaseResult}[/] \n");
    }

    private async Task CreateContainer()
    {
        string containerResult = "";

        await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Creating container...", async ctx =>
                {
                    ctx.Status("[bold white on green]Initializing Container...[/]");
                    var response = await database.CreateContainerIfNotExistsAsync(Constants.ContainerName, Constants.PartitionKey);
                    containerResult = SetResultMessage(response.StatusCode, "Container");

                    await Task.Delay(1000);
                    container = response;
                });

        AnsiConsole.Markup($"[bold white on blue]{containerResult}[/] \n");
    }

    private static string SetResultMessage(HttpStatusCode statusCode, string section)
    {
        return statusCode == System.Net.HttpStatusCode.Created ? $"{section} created successfully!" : $"{section} already exists.";
    }
}