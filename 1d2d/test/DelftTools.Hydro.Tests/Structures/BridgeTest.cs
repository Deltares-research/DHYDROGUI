using System.ComponentModel;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using NUnit.Framework;
using Category = NUnit.Framework.CategoryAttribute;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class BridgeTest
    {
        [Test]
        public void IsPillarBridge_WhenIsPillarIsTrue_ShouldReturnTrue()
        {
            // Arrange
            var bridge = new Bridge();
            int callCount = 0;
            ((INotifyPropertyChanged)bridge).PropertyChanged += (s, e) =>
            {
                Assert.AreEqual("BridgeType", e.PropertyName);// Only BridgeType update is expected not IsPillar
                callCount++;
            };
            // Act
            bridge.IsPillar = true;

            // Assert
            Assert.That(bridge.IsPillar, Is.True);
            Assert.That(callCount, Is.EqualTo(1));//1 not 2! 
        }

        [Test]
        public void IsPillarBridge_WhenStandardBridge_ShouldReturnFalse()
        {
            // Arrange
            var bridge = new Bridge();

            // Act
            bool result = bridge.IsPillar;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void PropertyChangedForTabulatedCrossection()
        {
            var bridge = new Bridge();
            bridge.TabulatedCrossSectionDefinition.SetWithHfswData(new[]
                                                             {
                                                                 new HeightFlowStorageWidth(10, 50, 50),
                                                                 new HeightFlowStorageWidth(16, 100, 100)
                                                             });

            int callCount = 0;
            ((INotifyPropertyChanged)bridge).PropertyChanged += (s, e) =>
            {
                Assert.AreEqual("IsEditing", e.PropertyName);
                callCount++;
            };

            var CrossSectionZWRow = bridge.TabulatedCrossSectionDefinition.ZWDataTable[0];
            CrossSectionZWRow.Width = 22;

            Assert.AreEqual(4, callCount);
        }

        [Test]
        public void CopyFrom()
        {
            var sourceBridge = new Bridge("source");
            var targetBridge = new Bridge("target")
                                   {
                                       InletLossCoefficient = 4.2,
                                       OutletLossCoefficient = 4.2,
                                       FlowDirection = FlowDirection.Positive,
                                       FrictionType = BridgeFrictionType.StricklerKs,
                                       Friction = 4.2,
                                       GroundLayerRoughness = 4.2,
                                       GroundLayerThickness = 1.1,
                                       BridgeType = BridgeType.Tabulated,
                                       Shift = 4.2,
                                       Width = 4.2,
                                       Height = 4.2,
                                       OffsetY = 12.0,
                                       ShapeFactor = 1.1,
                                       PillarWidth = 11.12
                                   };
            targetBridge.CopyFrom(sourceBridge);
            Assert.AreEqual(sourceBridge.InletLossCoefficient, targetBridge.InletLossCoefficient);
            Assert.AreEqual(sourceBridge.OutletLossCoefficient, targetBridge.OutletLossCoefficient);
            Assert.AreEqual(sourceBridge.FlowDirection, targetBridge.FlowDirection);
            Assert.AreEqual(sourceBridge.FrictionType, targetBridge.FrictionType);
            Assert.AreEqual(sourceBridge.Friction, targetBridge.Friction);
            Assert.AreEqual(sourceBridge.GroundLayerEnabled, targetBridge.GroundLayerEnabled);
            Assert.AreEqual(sourceBridge.GroundLayerThickness, targetBridge.GroundLayerThickness);
            Assert.AreEqual(sourceBridge.GroundLayerRoughness, targetBridge.GroundLayerRoughness);
            Assert.AreEqual(sourceBridge.Shift, targetBridge.Shift);
            Assert.AreEqual(sourceBridge.Width, targetBridge.Width);
            Assert.AreEqual(sourceBridge.Height, targetBridge.Height);
            Assert.AreEqual(sourceBridge.OffsetY, targetBridge.OffsetY);
            Assert.AreEqual(sourceBridge.ShapeFactor, targetBridge.ShapeFactor);
            Assert.AreEqual(sourceBridge.PillarWidth, targetBridge.PillarWidth);
        }
    }
}