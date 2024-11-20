using System;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Views;
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

        [TestCase(typeof(CreateFmModelSettingView))]
        [TestCase(typeof(MduTemplateView))]

        public void GivenFlowFMGuiPlugin_WhenGetViewInfoObjectsOfProjectTemplateForType_ReturnCorrectView(Type projectTemplateViewType)
        {
            //Arrange
            var plugin = new FlowFMGuiPlugin();


            // Act
            var viewInfoObjects = plugin.GetViewInfoObjects().ToArray();

            // Assert
            Assert.That(viewInfoObjects, Has.Some.Property(nameof(ViewInfo.DataType)).EqualTo(typeof(ProjectTemplate)));
            Assert.That(viewInfoObjects, Has.One.Property(nameof(ViewInfo.ViewType)).EqualTo(projectTemplateViewType));
        }

        [TestCase(typeof(CreateFmModelSettingView), FlowFMApplicationPlugin.FM_MODEL_DEFAULT_PROJECT_TEMPLATE_ID)]
        [TestCase(typeof(MduTemplateView), FlowFMApplicationPlugin.FM_MODEL_MDU_IMPORT_PROJECT_TEMPLATE_ID)]

        public void GivenFlowFMGuiPlugin_WhenGetViewInfoObjectsOfProjectTemplateForType_ReturnCorrectView(Type projectTemplateViewType, string fmProjectTemplateId)
        {
            //Arrange
            var plugin = new FlowFMGuiPlugin();
            var template = Substitute.For<ProjectTemplate>();
            template.Id = fmProjectTemplateId;
            
            // Act
            ViewInfo viewInfo = plugin.GetViewInfoObjects().SingleOrDefault(vi => vi.DataType == typeof(ProjectTemplate) && vi.ViewType == projectTemplateViewType);
            
            // Assert
            Assert.That(viewInfo, Is.Not.Null.And.Property(nameof(ViewInfo.AdditionalDataCheck)).Not.Null);
            Assert.That(viewInfo?.AdditionalDataCheck(template), Is.Not.Null.And.True);

        }
    }
}
