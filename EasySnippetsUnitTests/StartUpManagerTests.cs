using EasySnippets.Utils;
using FluentAssertions;
using Xunit;

namespace EasySnippetsUnitTests
{
    public class StartUpManagerTests
    {
        [Fact]
        public void AddAndRemoveStartupTest()
        {
            StartUpManager.AddApplicationToCurrentUserStartup();
            StartUpManager.IsApplicationAddedToCurrentUserStartup().Should().BeTrue();
            StartUpManager.RemoveApplicationFromCurrentUserStartup();
            StartUpManager.IsApplicationAddedToCurrentUserStartup().Should().BeFalse();
        }
    }
}
