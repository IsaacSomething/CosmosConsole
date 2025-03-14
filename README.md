# Cosmos Console

A simple application that interacts with the Azure Cosmos SDK

##### Libraries
[spectreconsole](https://spectreconsole.net/)

#### Fake data

[CosmicWorks](https://github.com/AzureCosmosDB/CosmicWorks)

```
  >: dotnet tool install --global cosmicworks

  >: cosmicworks --endpoint <my endpoint> --key <my key> --datasets product
```


#### TODO

Implement helper

```
using System;
using System.Diagnostics;

namespace MyConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Type 'add mock data' to run the command:");
            string input = Console.ReadLine();

            if (input.Equals("add mock data", StringComparison.OrdinalIgnoreCase))
            {
                var runSuccess = RunCommand("cosmicworks", "--endpoint <my endpoint> --key <my key> --datasets product");
                if (!runSuccess)
                {
                    Console.WriteLine("CosmicWorks tool not found. Installing...");
                    InstallCosmicWorks();
                    Console.WriteLine("Installation complete. Running the command again...");
                    RunCommand("cosmicworks", "--endpoint <my endpoint> --key <my key> --datasets product");
                }
            }
            else
            {
                Console.WriteLine("Unknown command.");
            }
        }

        static bool RunCommand(string command, string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    string result = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    Console.WriteLine(result);
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Error: " + error);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return false;
            }
        }

        static void InstallCosmicWorks()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "tool install --global cosmicworks",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    string result = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    Console.WriteLine(result);
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Error: " + error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while installing cosmicworks: " + ex.Message);
            }
        }
    }
}
```