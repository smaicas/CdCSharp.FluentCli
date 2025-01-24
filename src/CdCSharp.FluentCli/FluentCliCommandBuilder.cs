using CdCSharp.FluentCli.Abstractions;

namespace CdCSharp.FluentCli;
public class FluentCliCommandBuilder<TArgs> where TArgs : new()
{
    private readonly string _name;
    private readonly FCli _cli;
    private readonly List<string> _aliases = [];
    private string? _description;
    private Func<TArgs, Task>? _handler;

    public FluentCliCommandBuilder(string name, FCli cli) => (_name, _cli) = (name, cli);
    public FluentCliCommandBuilder<TArgs> WithAlias(string alias) { _aliases.Add(alias); return this; }
    public FluentCliCommandBuilder<TArgs> WithDescription(string description) { _description = description; return this; }

    public FCli OnExecute(Func<TArgs, Task> handler)
    {
        _handler = handler;
        FluentCliCommand<TArgs> command = new(_name, _aliases, _description, _handler);
        _cli.AddCommand(_name, command);
        foreach (string alias in _aliases)
            _cli.AddCommand(alias, command);
        return _cli;
    }

    public FCli OnExecute(Action<TArgs> handler) =>
        OnExecute(args => { handler(args); return Task.CompletedTask; });
}
