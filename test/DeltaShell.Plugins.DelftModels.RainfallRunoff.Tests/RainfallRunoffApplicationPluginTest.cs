using System;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffApplicationPluginTest
    {
        [Test]
        public void GivenRainfallRunoffApplicationPlugin_RainfallRunoffModelInitialize_ShouldLogPluginVersion()
        {
            //Arrange
            var plugin = new RainfallRunoffApplicationPlugin();
            var app = Substitute.For<IApplication>();
            var activityRunner = Substitute.For<IActivityRunner>();

            var rainfallRunoffModel = new RainfallRunoffModel();

            app.ActivityRunner.Returns(activityRunner);

            plugin.Application = app;

            // Act & Assert
            var messages = TestHelper.GetAllRenderedMessages(() =>
            {
                activityRunner.ActivityStatusChanged += Raise.Event<EventHandler<ActivityStatusChangedEventArgs>>(rainfallRunoffModel, new ActivityStatusChangedEventArgs(ActivityStatus.None, ActivityStatus.Initializing));
            });

            Assert.IsTrue(messages.Any(m => m.StartsWith("DeltaShell version")), "RainfallRunoffModel plugin version should be logged");
        }
    }
}