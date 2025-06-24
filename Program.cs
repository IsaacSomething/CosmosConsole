using Microsoft.Azure.Cosmos;
using Spectre.Console;
using System.Net;
using CosmosConsole.Shared;
using CosmosConsole.Models;
using Microsoft.Azure.Cosmos.Linq;

class Program
{
    private static CosmosClient? client;
    private static Database? database;
    private static Container? container;

    static async Task Main(string[] args)
    {
        Program program = new();
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        await GetStartedDemoAsync();
        await PromptContainerCreate();
        //await CreateDocument();
        WaitToExit();
    }

    public static async Task GetStartedDemoAsync()
    {
        AnsiConsole.Markup(":ringed_planet: \n");
        AnsiConsole.Markup("[bold black on orange3]Cosmos Console application[/] starting... \n");

        try
        {
            AnsiConsole.Markup("[bold black on orange3]Initializing client[/]...\n");
            client = new CosmosClient(Constants.EndpointUri, Constants.PrimaryKey);

            AnsiConsole.Markup($"[italic]Client started on {client.Endpoint}[/]\n");

            await GetInfo();

            AnsiConsole.Markup("[bold black on orange3]Initializing database[/] creating if none exist...\n");
            await CreateDatabase();
        }
        catch (Exception ex)
        {
            AnsiConsole.Markup($"[bold red]An error has occurred: {ex.Message} [/]\n");
        }
    }

    private static async Task CreateDocument()
    {
        if (container == null) return;

        Product product = new()
        {
            ProductId = "027D0B9A-F9D9-4C96-8213-C8546C4AAE71",
            CategoryId = "26C74104-40BC-4541-8EF5-9892F7F03D72",
            Name = "Product test",
            Price = 12.32d,
            Tags = ["test", "initial"]
        };

        try
        {
            ItemResponse<Product> newProduct = await container.CreateItemAsync<Product>(product);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            // add logic to handle conflicting ids
            AnsiConsole.Markup($"[bold white on red]Error has occurred[/] {ex}");
        }
        catch (CosmosException ex)
        {
            // add general exception handling
            AnsiConsole.Markup($"[bold white on red]Error has occurred[/] {ex}");
        }
    }

    private static async Task PromptContainerCreate()
    {
        var addContainer = AnsiConsole.Prompt(new ConfirmationPrompt("Would you like to create a container?"));
        if (addContainer)
        {
            string containerName = AnsiConsole.Prompt(new TextPrompt<string>("Give a name for the container:"));
            if (string.IsNullOrEmpty(containerName)) return;
            string partitionKey = AnsiConsole.Prompt(new TextPrompt<string>("What is the property used for the partition key?:"));
            AnsiConsole.Markup($"[bold black on orange3]Initializing container[/] {containerName}");
            await CreateContainer(containerName, partitionKey);
        }
    }

    private static async Task CreateDatabase()
    {
        string databaseResult = "";

        await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Creating database...", async ctx =>
                {
                    ctx.Status("[bold white on green]Initializing Database...[/]");
                    if (client == null) return;
                    var response = await client.CreateDatabaseIfNotExistsAsync(Constants.DatabaseName);
                    databaseResult = SetResultMessage(response.StatusCode, $"Database {Constants.DatabaseName}");

                    await Task.Delay(1000);
                    database = response.Database;
                });

        AnsiConsole.Markup($"[bold black on orange3]{databaseResult}[/] \n");
    }

    private static async Task CreateContainer(string? name = null, string? key = null)
    {
        string containerResult = "";
        string containerName = string.IsNullOrEmpty(name) ? Constants.ContainerName : name;
        string partitionKey = string.IsNullOrEmpty(key) ? Constants.PartitionKey : $"/{key}";

        await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Creating container...", async ctx =>
                {
                    ctx.Status("[bold white on green]Initializing Container...[/]");
                    if (database == null) return;
                    var response = await database.CreateContainerIfNotExistsAsync(containerName, partitionKey);
                    containerResult = SetResultMessage(response.StatusCode, "Container");

                    await Task.Delay(1000);
                    container = response;
                });

        AnsiConsole.Markup($"[bold black on orange3]{containerResult}[/] \n");
    }

    private static async Task GetInfo()
    {
        if (client == null) return;
        var databaseIterator = client.GetDatabaseQueryIterator<DatabaseProperties>();
        int databaseCount = 0;
        int containerCount = 0;
        int documentCount = 0;

        while (databaseIterator.HasMoreResults)
        {
            foreach (var database in await databaseIterator.ReadNextAsync())
            {
                databaseCount++;
                var queryDefinition = new QueryDefinition("SELECT * FROM c");
                var containerIterator = client.GetDatabase(database.Id).GetContainerQueryIterator<ContainerProperties>(queryDefinition);

                while (containerIterator.HasMoreResults)
                {
                    foreach (var container in await containerIterator.ReadNextAsync())
                    {
                        containerCount++;
                        var containerClient = client.GetContainer(database.Id, container.Id);
                        documentCount = await containerClient.GetItemLinqQueryable<dynamic>(true).CountAsync();
                    }
                }
            }
        }

        AnsiConsole.Write(new BarChart()
                   .Width(60)
                   .CenterLabel()
                   .AddItem("Databases", databaseCount, Color.Orange3)
                   .AddItem("Containers", containerCount, Color.Orange4)
                   .AddItem("Documents", documentCount, Color.Orange1));

    }

    private static string SetResultMessage(HttpStatusCode statusCode, string section)
    {
        return statusCode == HttpStatusCode.Created ? $"{section} created successfully!" : $"{section} already exists.";
    }

    private static void WaitToExit()
    {
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
}