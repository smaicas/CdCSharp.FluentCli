using System.Reflection;

namespace CdCSharp.FluentCli.Abstractions;
internal sealed class ArgumentInfo
{
    public PropertyInfo Property { get; }
    public ArgAttribute Attribute { get; }
    public string Abbreviation { get; }

    public ArgumentInfo(PropertyInfo property, ArgAttribute attribute, string abbreviation) =>
        (Property, Attribute, Abbreviation) = (property, attribute, abbreviation);
}
