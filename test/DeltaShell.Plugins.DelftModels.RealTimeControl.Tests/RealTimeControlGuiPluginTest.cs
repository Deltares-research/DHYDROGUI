using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlGuiPluginTest
    {
        private static readonly MockRepository mocks = new MockRepository();
        private ClipboardMock clipboard;

        [SetUp]
        public void SetUp()
        {
            if (!GuiTestHelper.IsBuildServer) return;
            clipboard = new ClipboardMock();
            clipboard.GetText_Returns_SetText();
            clipboard.GetData_Returns_SetData();
        }

        [TearDown]
        public void TearDown()
        {
            if (!GuiTestHelper.IsBuildServer) return;
            clipboard.Dispose();
        }


        [Test]
        [Category(TestCategory.Integration)]
        public void ReleaseCopiedBranchFeatureOnProjectClosing()
        {
            var gui = mocks.DynamicMock<IGui>();
            var documentViews = mocks.DynamicMock<IViewList>();
            using (var mapView = new MapView())
            {
                var activityRunner = mocks.DynamicMock<IActivityRunner>();
                var application = mocks.DynamicMock<IApplication>();
                var project = new Project();
                Expect.Call(documentViews.ActiveView).Return(mapView);
                Expect.Call(gui.DocumentViews).Return(documentViews).Repeat.Any();
                Expect.Call(gui.ToolWindowViews).Return(documentViews).Repeat.Any();
                Expect.Call(application.ActivityRunner).Return(activityRunner).Repeat.Any();
                Expect.Call(gui.Application).Return(application).Repeat.Any();
                Expect.Call(application.Project).Return(project).Repeat.Any();

                application.ProjectClosing += null;
                var projectClosingRaiser = LastCall.IgnoreArguments().GetEventRaiser();

                mocks.ReplayAll();

                var pluginGui = new RealTimeControlGuiPlugin {Gui = gui};
                pluginGui.Activate();

                RealTimeControlModelCopyPasteHelper.SetRtcObjectsToClipBoard(new ShapeBase[] {new RuleShape()});
                Assert.IsTrue(RealTimeControlModelCopyPasteHelper.IsClipBoardRtcObjectSet());

                projectClosingRaiser.Raise(project);
                Assert.IsFalse(RealTimeControlModelCopyPasteHelper.IsClipBoardRtcObjectSet());
            }
        }

        [Test]
        public void TestGetObjectProperties()
        {
            var guiPlugin = new RealTimeControlGuiPlugin();
            var propertyInfos = guiPlugin.GetPropertyInfos().ToList();

            var propertyInfo = propertyInfos.First(pi => pi.ObjectType == typeof(StandardCondition));
            Assert.AreEqual(typeof(StandardConditionProperties), propertyInfo.PropertyType);

            propertyInfo = propertyInfos.First(pi => pi.ObjectType == typeof(LookupSignal));
            Assert.AreEqual(typeof(LookupSignalProperties), propertyInfo.PropertyType);

            propertyInfo = propertyInfos.First(pi => pi.ObjectType == typeof(HydraulicRule));
            Assert.AreEqual(typeof(HydraulicRuleProperties), propertyInfo.PropertyType);

            propertyInfo = propertyInfos.First(pi => pi.ObjectType == typeof(RelativeTimeRule));
            Assert.AreEqual(typeof(RelativeTimeRuleProperties), propertyInfo.PropertyType);

            propertyInfo = propertyInfos.First(pi => pi.ObjectType == typeof(PIDRule));
            Assert.AreEqual(typeof(PIDRuleProperties), propertyInfo.PropertyType);

            propertyInfo = propertyInfos.First(pi => pi.ObjectType == typeof(TimeRule));
            Assert.AreEqual(typeof(TimeRuleProperties), propertyInfo.PropertyType);

            propertyInfo = propertyInfos.First(pi => pi.ObjectType == typeof(IntervalRule));
            Assert.AreEqual(typeof(IntervalRuleProperties), propertyInfo.PropertyType);

            propertyInfo = propertyInfos.First(pi => pi.ObjectType == typeof(FactorRule));
            Assert.AreEqual(typeof(FactorRuleProperties), propertyInfo.PropertyType);

            propertyInfo = propertyInfos.First(pi => pi.ObjectType == typeof(ControlGroup));
            Assert.AreEqual(typeof(ControlGroupProperties), propertyInfo.PropertyType);
        }
    }
}