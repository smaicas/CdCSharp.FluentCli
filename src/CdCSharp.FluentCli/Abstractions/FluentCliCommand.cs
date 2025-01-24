using System.Text;

namespace CdCSharp.FluentCli.Abstractions;
public class FluentCliCommand<TArgs> : IFluentCliCommand where TArgs : new()
{
    private readonly string _name;
    private readonly IReadOnlyList<string> _aliases;
    private readonly string? _description;
    private readonly Func<TArgs, Task> _handler;
    private readonly ArgumentCollection<TArgs> _arguments;

    public FluentCliCommand(string name, IReadOnlyList<string> aliases, string? description, Func<TArgs, Task> handler)
    {
        _name = name;
        _aliases = aliases;
        _description = description;
        _handler = handler;
        _arguments = new ArgumentCollection<TArgs>();
    }

    public async Task ExecuteAsync(string[] args)
    {
        if (args.Length > 0 && args[0] is "-h" or "--help")
        {
            Console.WriteLine(GetHelp());
            return;
        }

        TArgs? parsedArgs = _arguments.Parse(args);
        await _handler(parsedArgs);
    }

    public string GetHelp()
    {
        StringBuilder help = new(_name);

        if (_aliases.Any())
            help.Append($" (aliases: {string.Join(", ", _aliases)})");

        if (_description != null)
            help.AppendLine($"\n  Description: {_description}");

        string argsHelp = _arguments.GetHelp();
        if (!string.IsNullOrEmpty(argsHelp))
            help.AppendLine("\n  Arguments:").Append(argsHelp);

        return help.ToString();
    }
}
