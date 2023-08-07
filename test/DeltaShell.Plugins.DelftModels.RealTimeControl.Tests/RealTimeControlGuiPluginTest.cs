using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Gui.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlGuiPluginTest
    {
        private static readonly MockRepository mocks = new MockRepository();

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ReleaseCopiedBranchFeatureOnProjectClosing()
        {
            // Setup
            var helper = RealTimeControlModelCopyPasteHelper.Instance;
            helper.ClearData();

            // Precondition
            // Note: the helper is a singleton, so for every test make sure 
            // that the helper is in a clear state
            Assert.That(helper.IsDataSet, Is.False);
            Assert.That(helper.CopiedShapes, Is.Empty);

            var gui = mocks.DynamicMock<IGui>();
            var documentViews = mocks.DynamicMock<IViewList>();
            using (var clipboardMock = new ClipboardMock())
            using (var mapView = new MapView())
            {
                clipboardMock.GetData_Returns_SetData();

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
                IEventRaiser projectClosingRaiser = LastCall.IgnoreArguments().GetEventRaiser();

                mocks.ReplayAll();

                var pluginGui = new RealTimeControlGuiPlugin {Gui = gui};
                pluginGui.Activate();

                // Precondition
                helper.SetCopiedData(new ShapeBase[]
                {
                    new RuleShape()
                });
                Assert.IsTrue(helper.IsDataSet);
                CollectionAssert.IsNotEmpty(helper.CopiedShapes);

                // Call
                projectClosingRaiser.Raise(project);

                // Assert
                Assert.IsFalse(helper.IsDataSet);
                CollectionAssert.IsEmpty(helper.CopiedShapes);
            }
        }

        [Test]
        public void TestGetObjectProperties()
        {
            var guiPlugin = new RealTimeControlGuiPlugin();
            List<PropertyInfo> propertyInfos = guiPlugin.GetPropertyInfos().ToList();

            PropertyInfo propertyInfo = propertyInfos.First(pi => pi.ObjectType == typeof(StandardCondition));
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

        [Test]
        public void GetProjectTreeViewNodePresenters_ContainsCorrectNodePresenters()
        {
            // Given
            var guiPlugin = new RealTimeControlGuiPlugin();

            // When
            ITreeNodePresenter[] nodePresenters = guiPlugin.GetProjectTreeViewNodePresenters().ToArray();

            // Then
            var restartNodePresenter = Contains<RestartFileNodePresenter<RealTimeControlRestartFile>>(nodePresenters);
            Assert.That(restartNodePresenter.GuiPlugin, Is.SameAs(guiPlugin));
        }

        private static T Contains<T>(IEnumerable<ITreeNodePresenter> source)
        {
            List<T> items = source.OfType<T>().ToList();
            Assert.That(items, Has.Count.EqualTo(1), $"Collection should contain one {typeof(T).Name}");

            return items[0];
        }
    }
}