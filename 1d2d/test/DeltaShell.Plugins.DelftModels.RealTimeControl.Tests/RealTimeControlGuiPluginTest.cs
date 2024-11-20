using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using DeltaShell.Plugins.DelftModels.RTCShapes.IO;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlGuiPluginTest
    {
        private IGui gui;
        private IApplication application;

        [SetUp]
        public void SetUp()
        {
            gui = Substitute.For<IGui>();
            application = Substitute.For<IApplication>();

            gui.Application.Returns(application);
        }

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

            var documentViews = Substitute.For<IViewList>();
            using (var clipboardMock = new ClipboardMock())
            using (var mapView = new MapView())
            {
                clipboardMock.GetData_Returns_SetData();

                var activityRunner = Substitute.For<IActivityRunner>();
                var project = new Project();

                documentViews.ActiveView.Returns(mapView);
                gui.DocumentViews.Returns(documentViews);
                gui.ToolWindowViews.Returns(documentViews);
                application.ActivityRunner.Returns(activityRunner);
                application.ProjectService.Project.Returns(project);

                RealTimeControlGuiPlugin pluginGui = CreatePlugin();
                pluginGui.Activate();

                // Precondition
                helper.SetCopiedData(new ShapeBase[] { new RuleShape() });
                Assert.IsTrue(helper.IsDataSet);
                CollectionAssert.IsNotEmpty(helper.CopiedShapes);

                // Call
                application.ProjectService.ProjectClosing += Raise.EventWith(this, new EventArgs<Project>(project));

                // Assert
                Assert.IsFalse(helper.IsDataSet);
                CollectionAssert.IsEmpty(helper.CopiedShapes);
            }
        }

        [Test]
        public void TestGetObjectProperties()
        {
            RealTimeControlGuiPlugin guiPlugin = CreatePlugin();
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
            RealTimeControlGuiPlugin guiPlugin = CreatePlugin();

            // When
            ITreeNodePresenter[] nodePresenters = guiPlugin.GetProjectTreeViewNodePresenters().ToArray();

            // Then
            var restartNodePresenter = Contains<RealTimeControlInputRestartFileNodePresenter>(nodePresenters);
            Assert.That(restartNodePresenter.GuiPlugin, Is.SameAs(guiPlugin));
        }

        private static T Contains<T>(IEnumerable<ITreeNodePresenter> source)
        {
            List<T> items = source.OfType<T>().ToList();
            Assert.That(items, Has.Count.EqualTo(1), $"Collection should contain one {typeof(T).Name}");

            return items[0];
        }

        [Test]
        public void CanExport_ReturnsFalseForRealTimeControlModel()
        {
            // Given
            RealTimeControlGuiPlugin guiPlugin = CreatePlugin();
            var model = new RealTimeControlModel();

            // When
            bool canExport = guiPlugin.CanExport(model);

            // Then
            Assert.That(canExport, Is.False);
        }

        [Test]
        public void CanExport_ReturnsFalseForOtherThanRealTimeControlModel()
        {
            // Given
            RealTimeControlGuiPlugin guiPlugin = CreatePlugin();
            var model = Substitute.For<IProjectItem>();

            // When
            bool canExport = guiPlugin.CanExport(model);

            // Then
            Assert.That(canExport, Is.True);
        }

        [Test]
        public void OnApplicationOpened_RegistersShapesXmlReaderWriter()
        {
            // Given
            using (CreatePlugin())
            {
                var project = new Project();
                var importer = new RealTimeControlModelImporter();
                var exporter = new RealTimeControlModelExporter();

                application.FileImporters.Returns(new[] { importer });
                application.FileExporters.Returns(new[] { exporter });

                // When
                application.ProjectService.ProjectOpened += Raise.EventWith(this, new EventArgs<Project>(project));

                // Then
                Assert.That(importer.XmlReaders, Has.One.InstanceOf<ShapesXmlReader>());
                Assert.That(exporter.XmlWriters, Has.One.InstanceOf<ShapesXmlWriter>());
            }
        }
        
        [Test]
        public void OnApplicationOpened_EventRaisedTwice_RegistersSingleShapesXmlReaderWriter()
        {
            // Given
            using (CreatePlugin())
            {
                var project = new Project();
                var importer = new RealTimeControlModelImporter();
                var exporter = new RealTimeControlModelExporter();

                application.FileImporters.Returns(new[] { importer });
                application.FileExporters.Returns(new[] { exporter });

                // When
                application.ProjectService.ProjectOpened += Raise.EventWith(this, new EventArgs<Project>(project));
                application.ProjectService.ProjectOpened += Raise.EventWith(this, new EventArgs<Project>(project));

                // Then
                Assert.That(importer.XmlReaders, Has.One.InstanceOf<ShapesXmlReader>());
                Assert.That(exporter.XmlWriters, Has.One.InstanceOf<ShapesXmlWriter>());
            }
        }

        private RealTimeControlGuiPlugin CreatePlugin()
        {
            return new RealTimeControlGuiPlugin { Gui = gui };
        }
    }
}