using System.Reflection;

namespace CdCSharp.FluentCli.Abstractions;
internal sealed class ArgumentCollection<T> where T : new()
{
    private readonly Dictionary<string, ArgumentInfo> _arguments = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ArgumentInfo> _abbreviations = new(StringComparer.OrdinalIgnoreCase);

    internal ArgumentCollection()
    {
        List<(PropertyInfo Property, ArgAttribute? Attribute)> props = typeof(T).GetProperties()
            .Select(p => (Property: p, Attribute: p.GetCustomAttribute<ArgAttribute>()))
            .Where(x => x.Attribute != null)
            .ToList();

        AbbreviationCalculator calculator = new();
        Dictionary<string, string> abbreviations = calculator.Calculate(props.Select(x => x.Attribute!.Name));

        foreach ((PropertyInfo prop, ArgAttribute attr) in props)
        {
            string abbreviation = attr!.Abbreviation ?? abbreviations[attr.Name];
            ArgumentInfo info = new(prop, attr, abbreviation);

            _arguments[attr.Name] = info;
            _abbreviations[abbreviation] = info;
        }
    }

    public T Parse(string[] args)
    {
        T result = new();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].TrimStart('-');

            if (!TryGetArgumentInfo(arg, out ArgumentInfo? info))
                throw new FluentCliException($"Unknown argument: {args[i]}");

            string value = info.Attribute.HasValue
                ? GetArgumentValue(args, ref i)
                : "true";

            SetPropertyValue(result, info, value);
        }

        ValidateRequiredArguments(result);
        return result;
    }

    public string GetHelp() =>
        string.Join("\n", _arguments.Values.Select(arg =>
            $"    -{arg.Attribute.Name,-20} (-{arg.Abbreviation,-3}) " +
            $"{(arg.Attribute.IsRequired ? "*" : " ")} {arg.Attribute.Description ?? ""}"));

    private bool TryGetArgumentInfo(string arg, out ArgumentInfo info) =>
        _arguments.TryGetValue(arg, out info) || _abbreviations.TryGetValue(arg, out info);

    private string GetArgumentValue(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
            throw new FluentCliException($"Missing value for {args[index]}");

        return args[++index];
    }

    private void SetPropertyValue(T target, ArgumentInfo info, string value)
    {
        try
        {
            object convertedValue = info.Property.PropertyType.IsEnum
                ? Enum.Parse(info.Property.PropertyType, value, true)
                : Convert.ChangeType(value, info.Property.PropertyType);

            info.Property.SetValue(target, convertedValue);
        }
        catch
        {
            throw new FluentCliException($"Invalid value for -{info.Attribute.Name}: {value}");
        }
    }

    private void ValidateRequiredArguments(T target)
    {
        ArgumentInfo? missing = _arguments.Values
            .FirstOrDefault(x => x.Attribute.IsRequired && x.Property.GetValue(target) == null);

        if (missing != null)
            throw new FluentCliException($"Missing required argument: -{missing.Attribute.Name}");
    }
}
