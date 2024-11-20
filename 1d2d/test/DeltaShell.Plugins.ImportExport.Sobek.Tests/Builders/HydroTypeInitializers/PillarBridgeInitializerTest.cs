using DeltaShell.Plugins.ImportExport.Sobek.Builders.HydroTypeInitializers;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.Builders.HydroTypeInitializers
{
    using DelftTools.Hydro.Structures;
    using DeltaShell.Sobek.Readers.SobekDataObjects;
    using NUnit.Framework;

    [TestFixture]
    public class PillarBridgeInitializerTests
    {
        [Test]
        public void Initialize_SetsPillarWidth()
        {
            // Arrange
            SobekBridge sobekBridge = new SobekBridge { TotalPillarWidth = 80.1f };
            IBridge bridge = new Bridge();

            PillarBridgeInitializer initializer = new PillarBridgeInitializer();

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Assert.AreEqual(sobekBridge.TotalPillarWidth, bridge.PillarWidth);
        }

        [Test]
        public void Initialize_SetsShapeFactor()
        {
            // Arrange
            SobekBridge sobekBridge = new SobekBridge { FormFactor = 80.2f };
            IBridge bridge = new Bridge();

            PillarBridgeInitializer initializer = new PillarBridgeInitializer();

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Assert.AreEqual(sobekBridge.FormFactor, bridge.ShapeFactor);
        }

        [Test]
        public void Initialize_SetsBridgeTypeToPillar()
        {
            // Arrange
            SobekBridge sobekBridge = new SobekBridge();
            IBridge bridge = new Bridge();

            PillarBridgeInitializer initializer = new PillarBridgeInitializer();

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Assert.AreEqual(DelftTools.Hydro.Structures.BridgeType.Pillar, bridge.BridgeType);
        }
    }
}
