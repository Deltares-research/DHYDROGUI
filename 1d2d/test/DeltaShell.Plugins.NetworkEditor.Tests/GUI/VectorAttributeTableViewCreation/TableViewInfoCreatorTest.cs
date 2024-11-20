using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation
{
    [TestFixture]
    public class TableViewInfoCreatorTest
    {
        [Test]
        public void Constructor_WithNull_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new TableViewInfoCreator(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Create_WithNull_ThrowsArgumentNullException()
        {
            // Arrange
            var tableViewInfoCreator = new TableViewInfoCreator(new GuiContainer());

            // Act
            void Call()
            {
                tableViewInfoCreator.Create((ITableViewCreationContext<IFeature, IFeatureRowObject, IHydroRegion>)null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Create_WithDescription_ViewInfoHasDescription()
        {
            // Arrange
            var tableViewInfoCreator = new TableViewInfoCreator(new GuiContainer());
            var viewCreationContext = Substitute.For<ITableViewCreationContext<IFeature, IFeatureRowObject, IHydroRegion>>();
            const string description = "Description";
            viewCreationContext.GetDescription().Returns(description);

            // Act
            ViewInfo<IEnumerable<IFeature>, ILayer, VectorLayerAttributeTableView> viewInfo = tableViewInfoCreator.Create(viewCreationContext);

            // Assert
            Assert.That(viewInfo.Description, Is.EqualTo(description));
            Assert.That(viewInfo.AdditionalDataCheck, Is.Not.Null);
            Assert.That(viewInfo.GetCompositeViewData, Is.Not.Null);
            Assert.That(viewInfo.GetViewData, Is.Not.Null);
            Assert.That(viewInfo.AfterCreate, Is.Not.Null);
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public void InvokeAdditionalDataCheckViewInfo_AfterCreate_ChecksWhetherDataIsInRegion(bool regionContainsData, bool expResult)
        {
            // Arrange
            var region = Substitute.For<IHydroRegion>();
            var regionDataItem = new DataItem(region);
            var guiContainer = new GuiContainer { Gui = CreateGuiWith(regionDataItem) };

            IEnumerable<IFeature> features = Enumerable.Empty<IFeature>();
            var viewCreationContext = Substitute.For<ITableViewCreationContext<IFeature, IFeatureRowObject, IHydroRegion>>();
            viewCreationContext.IsRegionData(region, features).Returns(regionContainsData);

            var tableViewInfoCreator = new TableViewInfoCreator(guiContainer);
            ViewInfo<IEnumerable<IFeature>, ILayer, VectorLayerAttributeTableView> viewInfo = tableViewInfoCreator.Create(viewCreationContext);

            // Call
            bool result = viewInfo.AdditionalDataCheck.Invoke(features);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
            viewCreationContext.Received(1).IsRegionData(region, features);
        }

        [Test]
        public void InvokeGetCompositeViewData_AfterCreate_WhenRegionContainsData_ReturnsRegionDataItem()
        {
            // Arrange
            var region = Substitute.For<IHydroRegion>();
            var regionDataItem = new DataItem(region);
            var guiContainer = new GuiContainer { Gui = CreateGuiWith(regionDataItem) };

            IEnumerable<IFeature> features = Enumerable.Empty<IFeature>();
            var viewCreationContext = Substitute.For<ITableViewCreationContext<IFeature, IFeatureRowObject, IHydroRegion>>();
            viewCreationContext.IsRegionData(region, features).Returns(true);

            var tableViewInfoCreator = new TableViewInfoCreator(guiContainer);
            ViewInfo<IEnumerable<IFeature>, ILayer, VectorLayerAttributeTableView> viewInfo = tableViewInfoCreator.Create(viewCreationContext);

            // Call
            object result = viewInfo.GetCompositeViewData.Invoke(features);

            // Assert
            Assert.That(result, Is.SameAs(regionDataItem));
            viewCreationContext.Received(1).IsRegionData(region, features);
        }

        [Test]
        public void InvokeGetCompositeViewData_AfterCreate_WhenRegionDoesNotContainData_ReturnsNull()
        {
            // Arrange
            var region = Substitute.For<IHydroRegion>();
            var guiContainer = new GuiContainer { Gui = CreateGuiWith(new DataItem(region)) };

            IEnumerable<IFeature> features = Enumerable.Empty<IFeature>();
            var viewCreationContext = Substitute.For<ITableViewCreationContext<IFeature, IFeatureRowObject, IHydroRegion>>();
            viewCreationContext.IsRegionData(region, features).Returns(false);

            var tableViewInfoCreator = new TableViewInfoCreator(guiContainer);
            ViewInfo<IEnumerable<IFeature>, ILayer, VectorLayerAttributeTableView> viewInfo = tableViewInfoCreator.Create(viewCreationContext);

            // Call
            object result = viewInfo.GetCompositeViewData.Invoke(features);

            // Assert
            Assert.That(result, Is.Null);
            viewCreationContext.Received(1).IsRegionData(region, features);
        }

        private static IGui CreateGuiWith(object newObject)
        {
            var gui = Substitute.For<IGui>();
            var application = Substitute.For<IApplication>();
            var project = new Project();

            gui.Application = application;
            application.ProjectService.Project.Returns(project);
            project.RootFolder.Add(newObject);

            return gui;
        }
    }
}