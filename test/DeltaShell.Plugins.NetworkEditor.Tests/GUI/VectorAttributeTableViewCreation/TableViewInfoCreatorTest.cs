using System.Collections.Generic;
using DelftTools.Controls;
using DelftTools.Hydro;
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
            void Call() => new TableViewInfoCreator(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Create_WithNull_ThrowsArgumentNullException()
        {
            // Arrange
            var tableViewInfoCreator = new TableViewInfoCreator(new GuiContainer());

            // Act
            void Call() => tableViewInfoCreator.Create((ITableViewCreationContext<IFeature, IFeatureRowObject, IHydroRegion>)null);

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
    }
}