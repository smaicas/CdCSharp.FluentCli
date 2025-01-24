using CdCSharp.FluentCli.Abstractions;

namespace CdCSharp.FluentCli;
public class FCli
{
    private readonly Dictionary<string, IFluentCliCommand> _commands = new(StringComparer.OrdinalIgnoreCase);
    private Action<Exception>? _errorHandler;
    private string? _description;

    public FCli WithErrorHandler(Action<Exception> handler) { _errorHandler = handler; return this; }
    public FCli WithDescription(string description) { _description = description; return this; }
    public FluentCliCommandBuilder<TArgs> Command<TArgs>(string name) where TArgs : new() => new(name, this);
    public FluentCliCommandBuilder<Unit> Command(string name) =>
        Command<Unit>(name);

    public class Unit { }
    public async Task ExecuteAsync(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            string commandName = args[0];
            if (commandName is "-h" or "--help")
            {
                ShowHelp();
                return;
            }

            if (!_commands.TryGetValue(commandName, out IFluentCliCommand? command))
                throw new FluentCliException($"Unknown command: {commandName}");

            await command.ExecuteAsync(args[1..]);
        }
        catch (Exception ex) when (_errorHandler != null)
        {
            _errorHandler(ex);
        }
        catch
        {
            throw;
        }
    }

    internal void AddCommand(string name, IFluentCliCommand command) => _commands[name] = command;

    private void ShowHelp()
    {
        if (_description != null)
            Console.WriteLine(_description + Environment.NewLine);

        Console.WriteLine("Commands:");
        foreach (IFluentCliCommand? cmd in _commands.Values.DistinctBy(x => x.GetType()))
            Console.WriteLine($"  {cmd.GetHelp()}");
    }
}

