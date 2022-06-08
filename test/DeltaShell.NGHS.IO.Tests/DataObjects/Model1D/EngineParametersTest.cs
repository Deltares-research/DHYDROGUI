using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.DataObjects.Model1D
{
    [TestFixture]
    public class EngineParametersTest
    {
        [Test]
        public void AllowedAsQuantityTypeForFeature_FeatureIsWeir_ReturnsCorrectValue([Values] QuantityType quantityType, 
                                                                                      [Values]bool isGated)
        {
            // Setup
            EngineParameter engineParameter = CreateEngineParameter(quantityType);

            var weir = Substitute.For<IWeir>();
            weir.IsGated.Returns(isGated);

            // Call
            bool result = EngineParameters.AllowedAsQuantityTypeForFeature(weir, engineParameter);

            // Assert
            bool isAllowed = weir.IsGated && quantityType == QuantityType.GateLowerEdgeLevel ||
                             !weir.IsGated && (quantityType == QuantityType.CrestLevel ||
                                               quantityType == QuantityType.CrestWidth);
            Assert.That(result, Is.EqualTo(isAllowed));
        }
        
        [Test]
        public void AllowedAsQuantityTypeForFeature_FeatureIsCulvert_ReturnsCorrectValue([Values] QuantityType quantityType,
                                                                                         [Values]bool isGated)
        {
            // Setup
            EngineParameter engineParameter = CreateEngineParameter(quantityType);

            var culvert = Substitute.For<ICulvert>();
            culvert.IsGated.Returns(isGated);

            // Call
            bool result = EngineParameters.AllowedAsQuantityTypeForFeature(culvert, engineParameter);

            // Assert
            bool isAllowed = culvert.IsGated && quantityType == QuantityType.ValveOpening;
            Assert.That(result, Is.EqualTo(isAllowed));
        }

        [Test]
        public void AllowedAsQuantityTypeForFeature_FeatureIsPump_ReturnsCorrectValue([Values] QuantityType quantityType)
        {
            // Setup
            EngineParameter engineParameter = CreateEngineParameter(quantityType);

            var pump = Substitute.For<IPump>();

            // Call
            bool result = EngineParameters.AllowedAsQuantityTypeForFeature(pump, engineParameter);

            // Assert
            bool isAllowed = quantityType == QuantityType.PumpCapacity;
            Assert.That(result, Is.EqualTo(isAllowed));
        }
        
        [Test]
        public void AllowedAsQuantityTypeForFeature_FeatureIsObservationPoint_ReturnsCorrectValue([Values] QuantityType quantityType)
        {
            // Setup
            EngineParameter engineParameter = CreateEngineParameter(quantityType);

            var observationPoint = Substitute.For<IObservationPoint>();

            // Call
            bool result = EngineParameters.AllowedAsQuantityTypeForFeature(observationPoint, engineParameter);

            // Assert
            bool isAllowed = quantityType == QuantityType.WaterLevel ||
                             quantityType == QuantityType.WaterDepth ||
                             quantityType == QuantityType.Discharge  ||
                             quantityType == QuantityType.Velocity;
            Assert.That(result, Is.EqualTo(isAllowed));
        }

        private static EngineParameter CreateEngineParameter(QuantityType quantityType)
        {
            const ElementSet randomElementSet = ElementSet.Branches;
            const DataItemRole randomDataItemRole = DataItemRole.Input;
            const string randomName = "string";
            var randomUnit = new Unit();
            
            return new EngineParameter(quantityType, randomElementSet, randomDataItemRole, randomName, randomUnit);
        }
            
    }
}