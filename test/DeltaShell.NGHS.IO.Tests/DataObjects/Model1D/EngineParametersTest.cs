using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
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
        public void AllowedAsQuantityTypeForFeature_FeatureIsSimpleWeir_ReturnsCorrectValue([Values] QuantityType quantityType)
        {
            // Setup
            EngineParameter engineParameter = CreateEngineParameter(quantityType);

            var weir = Substitute.For<IWeir>();
            weir.WeirFormula = new SimpleWeirFormula();

            // Call
            bool result = EngineParameters.AllowedAsQuantityTypeForFeature(weir, engineParameter);

            // Assert
            bool isAllowed = quantityType == QuantityType.CrestLevel;
            Assert.That(result, Is.EqualTo(isAllowed));
        }
        
        [Test]
        public void AllowedAsQuantityTypeForFeature_FeatureIsGatedWeir_ReturnsCorrectValue([Values] QuantityType quantityType)
        {
            // Setup
            EngineParameter engineParameter = CreateEngineParameter(quantityType);

            var weir = Substitute.For<IWeir>();
            weir.WeirFormula = new GatedWeirFormula();

            // Call
            bool result = EngineParameters.AllowedAsQuantityTypeForFeature(weir, engineParameter);

            // Assert
            bool isAllowed = quantityType == QuantityType.GateLowerEdgeLevel;
            Assert.That(result, Is.EqualTo(isAllowed));
        }
        
        [Test]
        public void AllowedAsQuantityTypeForFeature_FeatureIsGeneralStructure_ReturnsCorrectValue([Values] QuantityType quantityType)
        {
            // Setup
            EngineParameter engineParameter = CreateEngineParameter(quantityType);

            var weir = Substitute.For<IWeir>();
            weir.WeirFormula = new GeneralStructureWeirFormula();

            // Call
            bool result = EngineParameters.AllowedAsQuantityTypeForFeature(weir, engineParameter);

            // Assert
            bool isAllowed = engineParameter.QuantityType == QuantityType.CrestLevel
                             || engineParameter.QuantityType == QuantityType.GateOpeningHeight
                             || engineParameter.QuantityType == QuantityType.GateLowerEdgeLevel
                             || engineParameter.QuantityType == QuantityType.GateOpeningWidth;
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