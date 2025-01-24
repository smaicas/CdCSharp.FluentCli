# CdCSharp.FluentCli 📚

[![NuGet](https://img.shields.io/nuget/v/CdCSharp.FluentCli.svg)](https://www.nuget.org/packages/CdCSharp.FluentCli)
[![License](https://img.shields.io/github/license/smaicas/CdCSharp.FluentCli)](LICENSE)
[![Build Status](https://img.shields.io/github/actions/workflow/status/smaicas/CdCSharp.FluentCli/dotnet.yml?branch=<BRANCH>)](https://github.com/smaicas/CdCSharp.FluentCli/actions/workflows/dotnet.yml)

🚀 A Fluent CLI Command manager.

## 🌟 Example

```csharp
public static async Task Main(string[] args)
{
    try
    {
        Cli cli = new Cli()
           .WithDescription("CLI Tool")
           .WithErrorHandler(ex => Console.WriteLine(ex.Message))
           .Command<ThemeArgs>("command1")
               .WithAlias("c1)
               .WithDescription("Executes Command 1")
               .OnExecute(async args =>
                   await Command1.ExecuteCommand1Async(
                       rootPath: args.Path,
                       outputFolder: args.Output,
                       outputFile: args.File))
           .Command<VariablesArgs>("command2")
               .WithAlias("c2")
               .WithDescription("Executes Command 2")
               .OnExecute(async args =>
                   await Command2.ExecuteCommand2Async(
                       rootPath: args.Path,
                       outputFolder: args.Output,
                       outputFile: args.File));

        await cli.ExecuteAsync(args);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

```


## 📦 Installation

```bash
dotnet add package CdCSharp.FluentCli
```

## 🤝 Contributing

Contributions are welcome. Please read our [contribution guide](https://github.com/smaicas/CdCSharp.FluentCli/blob/master/CONTRIBUTE.md) before submitting a PR.
Join the [discord](https://discord.gg/MpUfe7zD)

## 📄 License

This project is licensed under the GPL v3 License - see the [LICENSE](https://github.com/smaicas/CdCSharp.FluentCli/blob/master/LICENSE) file for details.

## 🙏 Acknowledgments

- The .NET community
- The Blazor Community
- All contributors
