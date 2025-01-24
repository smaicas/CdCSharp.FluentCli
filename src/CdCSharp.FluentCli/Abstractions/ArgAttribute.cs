namespace CdCSharp.FluentCli.Abstractions;

[AttributeUsage(AttributeTargets.Property)]
public class ArgAttribute : Attribute
{
    public string Name { get; }
    public bool IsRequired { get; init; }
    public bool HasValue { get; init; } = true;
    public string? Description { get; init; }
    public string? Abbreviation { get; init; }

    public ArgAttribute(string name, string? description = null, string? abbreviation = null) =>
        (Name, Description, Abbreviation) = (name, description, abbreviation);
}
