using System;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class FlowFMGuiPluginTest
    {
        [Test]
        public void GivenFlowFMGuiPlugin_WaterFlowFMModelFails_ShouldShowValidationView()
        {
            //Arrange
            var plugin = new FlowFMGuiPlugin();
            var gui = Substitute.For<IGui>();
            var guiCommandHandler = Substitute.For<IGuiCommandHandler>();
            var app = Substitute.For<IApplication>();
            var activityRunner = Substitute.For<IActivityRunner>();

            var waterFlowFMModel = new WaterFlowFMModel();

            gui.Application.Returns(app);
            gui.CommandHandler.Returns(guiCommandHandler);
            app.ActivityRunner.Returns(activityRunner);

            plugin.Gui = gui;

            // Act
            activityRunner.ActivityStatusChanged += Raise.Event<EventHandler<ActivityStatusChangedEventArgs>>(waterFlowFMModel, new ActivityStatusChangedEventArgs(ActivityStatus.Cleaning, ActivityStatus.Failed));
            
            // Assert
            guiCommandHandler.Received().OpenView(waterFlowFMModel, typeof(ValidationView));
        }
    }
}
