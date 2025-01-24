using CdCSharp.FluentCli;
using CdCSharp.FluentCli.Abstractions;

namespace CdCSharp.Generic.UnitTests.FluentCli_Tests;

internal class Empty { }

public class FCli_Tests
{
    [Fact]
    public async Task ExecuteAsync_WithNoArgs_ShowsHelp()
    {
        // Arrange
        StringWriter output = new();
        Console.SetOut(output);

        FCli cli = new FCli()
            .WithDescription("Test CLI")
            .Command<Empty>("test")
                .OnExecute(_ => Task.CompletedTask);

        // Act
        await cli.ExecuteAsync(Array.Empty<string>());

        // Assert
        string help = output.ToString();
        Assert.Contains("Test CLI", help);
        Assert.Contains("test", help);
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    public async Task ExecuteAsync_WithHelpFlag_ShowsHelp(string helpFlag)
    {
        // Arrange
        StringWriter output = new();
        Console.SetOut(output);

        FCli cli = new FCli()
            .Command<Empty>("test")
                .OnExecute(_ => Task.CompletedTask);

        // Act
        await cli.ExecuteAsync(new[] { helpFlag });

        // Assert
        Assert.Contains("test", output.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCommand_ThrowsException()
    {
        // Arrange
        FCli cli = new();
        Exception exception = await Record.ExceptionAsync(() =>
            cli.ExecuteAsync(new[] { "unknown" }));

        // Assert
        FluentCliException cliException = Assert.IsType<FluentCliException>(exception);
        Assert.Equal("Unknown command: unknown", cliException.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithCommandHelp_ShowsDetailedHelp()
    {
        StringWriter output = new();
        Console.SetOut(output);
        FCli cli = new FCli()
            .Command<TestArgs>("test")
                .WithDescription("Test command")
                .OnExecute(_ => Task.CompletedTask);

        await cli.ExecuteAsync(new[] { "test", "--help" });
        string help = output.ToString();

        Assert.Contains("Test command", help);
        Assert.Contains("-required", help);
        Assert.Contains("-optional", help);
        Assert.Contains("-flag", help);
    }

    [Fact]
    public async Task ExecuteAsync_WithAbbreviations_CallsHandler()
    {
        bool handlerCalled = false;
        FCli cli = new FCli()
            .Command<TestArgs>("test")
                .OnExecute(args =>
                {
                    Assert.Equal("value", args.Required);
                    handlerCalled = true;
                    return Task.CompletedTask;
                });

        await cli.ExecuteAsync(new[] { "test", "-r", "value" });
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task ExecuteAsync_WithExplicitAbbreviation_RespectsConfiguration()
    {
        FCli cli = new FCli()
            .Command<ExplicitAbbrevArgs>("test")
                .OnExecute(_ => Task.CompletedTask);

        await cli.ExecuteAsync(new[] { "test", "-x", "value" });
    }

    [Fact]
    public async Task ExecuteAsync_WithHandlerException_PropagatesException()
    {
        FCli cli = new FCli()
            .Command<Empty>("test")
                .OnExecute(_ => throw new InvalidOperationException("Test error"));

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            cli.ExecuteAsync(new[] { "test" }));
        Assert.Equal("Test error", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithBooleanFlag_ParsesCorrectly()
    {
        bool handlerCalled = false;
        FCli cli = new FCli()
            .Command<FlagArgs>("test")
                .OnExecute(args =>
                {
                    Assert.True(args.Flag1);
                    Assert.True(args.Flag2);
                    handlerCalled = true;
                    return Task.CompletedTask;
                });

        await cli.ExecuteAsync(new[] { "test", "-f1", "-f2" });
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task ExecuteAsync_WithEnumArg_ParsesCorrectly()
    {
        bool handlerCalled = false;
        FCli cli = new FCli()
            .Command<EnumArgs>("test")
                .OnExecute(args =>
                {
                    Assert.Equal(TestEnum.Value2, args.EnumValue);
                    handlerCalled = true;
                    return Task.CompletedTask;
                });

        await cli.ExecuteAsync(new[] { "test", "-e", "Value2" });
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidEnumValue_ThrowsException()
    {
        FCli cli = new FCli()
            .Command<EnumArgs>("test")
            .OnExecute(_ => Task.CompletedTask);

        await Assert.ThrowsAsync<FluentCliException>(() =>
            cli.ExecuteAsync(new[] { "test", "-e", "InvalidValue" }));
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingRequiredArgument_ThrowsException()
    {
        FCli cli = new FCli()
            .Command<TestArgs>("test")
            .OnExecute(_ => Task.CompletedTask);

        await Assert.ThrowsAsync<FluentCliException>(() =>
            cli.ExecuteAsync(new[] { "test" }));
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleAliases_WorksCorrectly()
    {
        bool handlerCalled = false;
        FCli cli = new FCli()
            .Command<Empty>("test")
                .WithAlias("t")
                .WithAlias("try")
                .OnExecute(_ =>
                {
                    handlerCalled = true;
                    return Task.CompletedTask;
                });

        await cli.ExecuteAsync(new[] { "t" });
        Assert.True(handlerCalled);

        handlerCalled = false;
        await cli.ExecuteAsync(new[] { "try" });
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomErrorHandler_CatchesException()
    {
        bool errorHandlerCalled = false;
        FCli cli = new FCli()
            .WithErrorHandler(ex =>
            {
                errorHandlerCalled = true;
                Assert.IsType<InvalidOperationException>(ex);
            })
            .Command<Empty>("test")
                .OnExecute(_ => throw new InvalidOperationException("Test error"));

        await cli.ExecuteAsync(new[] { "test" });
        Assert.True(errorHandlerCalled);
    }

    [Theory]
    [InlineData("test", "-r")]
    [InlineData("test", "--required")]
    public async Task ExecuteAsync_WithTypographicVariations_ParsesCorrectly(
        string command, string requiredFlag)
    {
        bool handlerCalled = false;
        FCli cli = new FCli()
            .Command<RequiredStringArgs>(command)
                .OnExecute(args =>
                {
                    Assert.Equal("value", args.Required);
                    handlerCalled = true;
                    return Task.CompletedTask;
                });

        await cli.ExecuteAsync(new[] { command, requiredFlag, "value" });
        Assert.True(handlerCalled);
    }

    public class RequiredStringArgs
    {
        [Arg("required", IsRequired = true)]
        public string Required { get; set; } = null!;
    }

    public class TestArgs
    {
        [Arg("required", IsRequired = true)]
        public string Required { get; set; } = null!;

        [Arg("optional")]
        public string? Optional { get; set; }

        [Arg("flag", HasValue = false)]
        public bool Flag { get; set; }
    }

    public class ExplicitAbbrevArgs
    {
        [Arg("test", Abbreviation = "x")]
        public string Test { get; set; } = null!;
    }

    public class FlagArgs
    {
        [Arg("flag1", HasValue = false)]
        public bool Flag1 { get; set; }

        [Arg("flag2", HasValue = false)]
        public bool Flag2 { get; set; }
    }

    public enum TestEnum { Value1, Value2 }

    public class EnumArgs
    {
        [Arg("enum")]
        public TestEnum EnumValue { get; set; }
    }
}
