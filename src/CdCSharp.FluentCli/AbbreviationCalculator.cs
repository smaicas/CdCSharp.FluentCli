namespace CdCSharp.FluentCli;
internal class AbbreviationCalculator
{
    private readonly Dictionary<string, string> _baseAbbreviations = [];
    private readonly HashSet<string> _used = [];

    public Dictionary<string, string> Calculate(IEnumerable<string> names)
    {
        Dictionary<string, string> result = [];
        Dictionary<string, List<string>> grouped = GroupByBaseNames(names);

        // First process numeric to store their bases
        foreach ((string baseName, List<string> variants) in grouped.Where(g => g.Value.Any(HasNumber)))
        {
            string baseAbbr = GetBaseAbbreviation(baseName);
            foreach (string? name in variants.Where(HasNumber))
            {
                (string _, int? number) = ExtractNumberSuffix(name);
                result[name] = $"{baseAbbr}{number}";
            }
        }

        foreach ((string _, List<string> variants) in grouped)
        {
            foreach (string? name in variants.Where(n => !HasNumber(n) && !result.ContainsKey(n)))
            {
                result[name] = GetUniqueAbbreviation(name);
            }
        }

        return result;
    }

    private static bool HasNumber(string name) =>
        ExtractNumberSuffix(name).Number.HasValue;

    private Dictionary<string, List<string>> GroupByBaseNames(IEnumerable<string> names)
    {
        Dictionary<string, List<string>> groups = [];

        foreach (string name in names)
        {
            (string baseName, int? _) = ExtractNumberSuffix(name);
            if (!groups.ContainsKey(baseName))
                groups[baseName] = [];

            groups[baseName].Add(name);
        }

        return groups;
    }

    private string GetBaseAbbreviation(string name)
    {
        if (_baseAbbreviations.TryGetValue(name, out string? existing))
            return existing;

        string abbr = GetUniqueAbbreviation(name);
        _baseAbbreviations[name] = abbr;
        return abbr;
    }

    private string GetUniqueAbbreviation(string name)
    {
        for (int len = 1; len <= name.Length; len++)
        {
            string abbr = name[..len].ToLower();
            if (!_used.Contains(abbr))
            {
                _used.Add(abbr);
                return abbr;
            }
        }

        throw new InvalidOperationException($"Cannot generate unique abbreviation for {name}");
    }

    private static (string BaseName, int? Number) ExtractNumberSuffix(string name)
    {
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(name, @"^(.+?)(\d+)$");
        return match.Success
            ? (match.Groups[1].Value, int.Parse(match.Groups[2].Value))
            : (name, null);
    }
}
