using System;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlGuiPluginTest
    {
        [Test]
        public void GivenRealTimeControlGuiPlugin_RealTimeControlModelFails_ShouldShowValidationView()
        {
            //Arrange
            var plugin = new RealTimeControlGuiPlugin();
            var gui = Substitute.For<IGui>();
            var guiCommandHandler = Substitute.For<IGuiCommandHandler>();
            var app = Substitute.For<IApplication>();
            var activityRunner = Substitute.For<IActivityRunner>();

            var realTimeControlModel = new RealTimeControlModel();

            gui.Application.Returns(app);
            gui.CommandHandler.Returns(guiCommandHandler);
            app.ActivityRunner.Returns(activityRunner);

            plugin.Gui = gui;

            // Act
            activityRunner.ActivityStatusChanged += Raise.Event<EventHandler<ActivityStatusChangedEventArgs>>(realTimeControlModel, new ActivityStatusChangedEventArgs(ActivityStatus.Cleaning, ActivityStatus.Failed));

            // Assert
            guiCommandHandler.Received().OpenView(realTimeControlModel, Arg.Is<Type>(t => t.Implements(typeof(IView))));
        }
    }
}