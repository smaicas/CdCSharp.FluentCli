using CdCSharp.FluentCli;
using CdCSharp.FluentCli.Abstractions;
using CdCSharp.Generic.UnitTests.FluentCli_Tests;

namespace CdCSharp.Generic.UnitTests.Cli_Tests;
public class CommandTests
{
    public class TestArgs
    {
        [Arg("required", IsRequired = true)]
        public string Required { get; set; } = null!;

        [Arg("optional")]
        public string? Optional { get; set; }

        [Arg("flag", HasValue = false)]
        public bool Flag { get; set; }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidArgs_CallsHandler()
    {
        // Arrange
        bool handlerCalled = false;
        FCli cli = new FCli()
            .Command<TestArgs>("test")
                .OnExecute(args =>
                {
                    Assert.Equal("value", args.Required);
                    Assert.Equal("optional", args.Optional);
                    Assert.True(args.Flag);
                    handlerCalled = true;
                    return Task.CompletedTask;
                });

        // Act
        await cli.ExecuteAsync(new[] {
            "test",
            "-required", "value",
            "-optional", "optional",
            "-flag"
        });

        // Assert
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingRequiredArg_ThrowsException()
    {
        // Arrange
        FCli cli = new FCli()
            .Command<TestArgs>("test")
                .OnExecute(_ => Task.CompletedTask);

        // Act & Assert
        Exception exception = await Record.ExceptionAsync(() =>
            cli.ExecuteAsync(new[] { "test" }));

        FluentCliException cliException = Assert.IsType<FluentCliException>(exception);
        Assert.Equal("Missing required argument: -required", cliException.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidArgValue_ThrowsException()
    {
        // Arrange
        FCli cli = new FCli()
            .Command<NumericArgs>("test")
                .OnExecute(_ => Task.CompletedTask);

        // Act & Assert
        Exception exception = await Record.ExceptionAsync(() =>
            cli.ExecuteAsync(new[] { "test", "-number", "notanumber" }));

        FluentCliException cliException = Assert.IsType<FluentCliException>(exception);
        Assert.Equal("Invalid value for -number: notanumber", cliException.Message);
    }

    public class NumericArgs
    {
        [Arg("number")]
        public int Number { get; set; }
    }

    [Fact]
    public async Task ExecuteAsync_WithAlias_CallsHandler()
    {
        // Arrange
        bool handlerCalled = false;
        FCli cli = new FCli()
            .Command<Empty>("test")
                .WithAlias("alias")
                .OnExecute(_ =>
                {
                    handlerCalled = true;
                    return Task.CompletedTask;
                });

        // Act
        await cli.ExecuteAsync(new[] { "alias" });

        // Assert
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task ExecuteAsync_WithErrorHandler_HandlesException()
    {
        // Arrange
        Exception? caught = null;
        FCli cli = new FCli()
            .WithErrorHandler(ex => caught = ex)
            .Command<TestArgs>("test")
                .OnExecute(_ => Task.CompletedTask);

        // Act
        await cli.ExecuteAsync(new[] { "test" });

        // Assert
        Assert.NotNull(caught);
        Assert.IsType<FluentCliException>(caught);
    }
}