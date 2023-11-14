using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    [TestFixture]
    public class BridgePillarTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new BridgePillarTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Bridge pillar table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new BridgePillarTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, Enumerable.Empty<BridgePillar>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new BridgePillarTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(new HydroArea(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainBridgePillars_ReturnsFalse()
        {
            // Arrange
            var creationContext = new BridgePillarTableViewCreationContext();
            var region = new HydroArea();
            region.BridgePillars.AddRange(new BridgePillar[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new BridgePillar[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsBridgePillars_ReturnsTrue()
        {
            // Arrange
            var creationContext = new BridgePillarTableViewCreationContext();
            var region = new HydroArea();
            region.BridgePillars.AddRange(new BridgePillar[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.BridgePillars);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new BridgePillarTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new BridgePillarTableViewCreationContext();
            var feature = new BridgePillar();

            // Act
            BridgePillarRow result = creationContext.CreateFeatureRowObject(feature);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableView_WithViewNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new BridgePillarTableViewCreationContext();
            var data = new EventedList<BridgePillar>();
            var guiContainer = new GuiContainer();

            // Act
            void Call()
            {
                creationContext.CustomizeTableView(null, data, guiContainer);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CustomizeTableView_WithDataNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new BridgePillarTableViewCreationContext();
            var view = new VectorLayerAttributeTableView();
            var guiContainer = new GuiContainer();

            // Act
            void Call()
            {
                creationContext.CustomizeTableView(view, null, guiContainer);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CustomizeTableView_WithGuiContainerNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new BridgePillarTableViewCreationContext();
            var view = new VectorLayerAttributeTableView();
            var data = new EventedList<BridgePillar>();

            // Act
            void Call()
            {
                creationContext.CustomizeTableView(view, data, null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
    }
}