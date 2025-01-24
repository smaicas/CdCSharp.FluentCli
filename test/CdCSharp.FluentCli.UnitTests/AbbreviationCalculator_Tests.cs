using CdCSharp.FluentCli;
using CdCSharp.FluentCli.Abstractions;
using System.Reflection;

namespace CdCSharp.Generic.UnitTests.Cli_Tests;
public class AbbreviationCalculator_Tests
{
    private readonly AbbreviationCalculator _calculator = new();

    [Fact]
    public void Calculate_SimpleNames_GeneratesBaseAbbreviations()
    {
        Dictionary<string, string> abbreviations = _calculator.Calculate(new[] { "start", "stop" });
        Assert.Equal("s", abbreviations["start"]);
        Assert.Equal("st", abbreviations["stop"]);
    }

    [Fact]
    public void Calculate_SimilarNames_DifferentiatesAbbreviations()
    {
        Dictionary<string, string> abbreviations = _calculator.Calculate(new[] { "fire", "feature" });
        Assert.Equal("f", abbreviations["fire"]);
        Assert.Equal("fe", abbreviations["feature"]);
    }

    [Fact]
    public void Calculate_NumberedSequence_MaintainsBaseAndNumbers()
    {
        Dictionary<string, string> abbreviations = _calculator.Calculate(new[] { "flag1", "flag2" });
        Assert.Equal("f1", abbreviations["flag1"]);
        Assert.Equal("f2", abbreviations["flag2"]);
    }

    [Fact]
    public void Calculate_MixedNumberAndRegular_UsesAppropriateScheme()
    {
        Dictionary<string, string> abbreviations = _calculator.Calculate(new[] { "flag1", "fire" });
        Assert.Equal("f1", abbreviations["flag1"]);
        Assert.Equal("fi", abbreviations["fire"]);
    }

    [Fact]
    public void Calculate_LongerBaseNames_HandlesNumberedSequence()
    {
        Dictionary<string, string> abbreviations = _calculator.Calculate(new[] { "feature1", "feature2" });
        Assert.Equal("f1", abbreviations["feature1"]);
        Assert.Equal("f2", abbreviations["feature2"]);
    }

    [Fact]
    public void Calculate_MultipleSeries_HandlesCorrectly()
    {
        Dictionary<string, string> abbreviations = _calculator.Calculate(new[] {
           "flag1", "flag2",
           "feature1", "feature2",
           "fire", "first"
       });

        Assert.Equal("f1", abbreviations["flag1"]);
        Assert.Equal("f2", abbreviations["flag2"]);
        Assert.Equal("fe1", abbreviations["feature1"]);
        Assert.Equal("fe2", abbreviations["feature2"]);
        Assert.Equal("fi", abbreviations["fire"]);
        Assert.Equal("fir", abbreviations["first"]);
    }

    [Fact]
    public void Calculate_WithExplicitAbbreviations_RespectsConfiguration()
    {
        IEnumerable<string?> props = typeof(TestArgs).GetProperties()
           .Select(p => p.GetCustomAttribute<ArgAttribute>()?.Name)
           .Where(n => n != null);

        Dictionary<string, string> abbreviations = _calculator.Calculate(props!);
        Assert.Equal("f1", abbreviations["flag1"]);
        Assert.Equal("f2", abbreviations["flag2"]);
        Assert.Equal("c", abbreviations["custom"]);
    }

    private class TestArgs
    {
        [Arg("flag1")]
        public bool Flag1 { get; set; }

        [Arg("flag2")]
        public bool Flag2 { get; set; }

        [Arg("custom")]
        public string Custom { get; set; } = null!;
    }
}
