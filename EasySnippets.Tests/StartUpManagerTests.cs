using EasySnippets.Utils;
using Shouldly;
using Xunit;

namespace EasySnippets.Tests;

public class StartUpManagerTests
{
    [Fact]
    public void AddAndRemoveStartupTest()
    {
        StartUpManager.AddApplicationToCurrentUserStartup();
        StartUpManager.IsApplicationAddedToCurrentUserStartup().ShouldBeTrue();
        StartUpManager.RemoveApplicationFromCurrentUserStartup();
        StartUpManager.IsApplicationAddedToCurrentUserStartup().ShouldBeFalse();
    }
}