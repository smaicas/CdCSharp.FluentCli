namespace CdCSharp.FluentCli.Abstractions;
public interface IFluentCliCommand
{
    Task ExecuteAsync(string[] args);
    string GetHelp();
}
