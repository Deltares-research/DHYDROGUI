using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class QuantityGeneratorTest
    {
        private readonly MockRepository mocks = new MockRepository();

        [TestCase(true)]
        [TestCase(false)]
        public void GivenPump_WhenGettingQuantitiesForPump_ThenExpectedQuantitiesAreReturned(bool useSalinity)
        {
            // Given
            var pump = mocks.Stub<IPump>();

            // When
            string[] quantities = QuantityGenerator.GetQuantitiesForFeature(pump, useSalinity).ToArray();

            // Then
            Assert.That(quantities.Length, Is.EqualTo(1));
            Assert.That(quantities.Contains(KnownStructureProperties.Capacity));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenSimpleWeir_WhenGettingQuantitiesForSimpleWeir_ThenExpectedQuantitiesAreReturned(bool useSalinity)
        {
            // Given
            IStructure weir = GetWeirStubWithWeirFormulaType<SimpleWeirFormula>();

            // When
            string[] quantities = QuantityGenerator.GetQuantitiesForFeature(weir, useSalinity).ToArray();

            // Then
            Assert.That(quantities.Length, Is.EqualTo(1));
            Assert.That(quantities.Contains(KnownStructureProperties.CrestLevel));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenGeneralStructure_WhenGettingQuantitiesForGeneralStructure_ThenExpectedQuantitiesAreReturned(bool useSalinity)
        {
            // Given
            IStructure generalStructure = GetWeirStubWithWeirFormulaType<GeneralStructureFormula>();

            // When
            string[] quantities = QuantityGenerator.GetQuantitiesForFeature(generalStructure, useSalinity).ToArray();

            // Then
            Assert.That(quantities.Length, Is.EqualTo(3));
            Assert.That(quantities.Contains(KnownGeneralStructureProperties.CrestLevel.GetDescription()));
            Assert.That(quantities.Contains(KnownGeneralStructureProperties.GateLowerEdgeLevel.GetDescription()));
            Assert.That(quantities.Contains(KnownGeneralStructureProperties.GateOpeningWidth.GetDescription()));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenGate_WhenGettingQuantitiesForGate_ThenExpectedQuantitiesAreReturned(bool useSalinity)
        {
            // Given
            IStructure gate = GetWeirStubWithWeirFormulaType<SimpleGateFormula>();

            // When
            string[] quantities = QuantityGenerator.GetQuantitiesForFeature(gate, useSalinity).ToArray();

            // Then
            Assert.That(quantities.Length, Is.EqualTo(3));
            Assert.That(quantities.Contains(KnownStructureProperties.CrestLevel));
            Assert.That(quantities.Contains(KnownStructureProperties.GateLowerEdgeLevel));
            Assert.That(quantities.Contains(KnownStructureProperties.GateOpeningWidth));
        }

        [TestCase(false, "water_level", "water_depth")]
        [TestCase(true, "water_level", "water_depth", "salinity")]
        public void GivenGroupableFeature2DPoint_WhenGettingQuantitiesForGroupableFeature2DPoint_ThenExpectedQuantitiesAreReturned
            (bool useSalinity, params string[] expectedQuantities)
        {
            // Given
            var groupableFeature2DPoint = new GroupableFeature2DPoint();

            // When
            string[] quantities = QuantityGenerator.GetQuantitiesForFeature(groupableFeature2DPoint, useSalinity).ToArray();

            // Then
            Assert.That(quantities.Length, Is.EqualTo(expectedQuantities.Length));
            foreach (string quantity in expectedQuantities)
            {
                Assert.That(quantities.Contains(quantity));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenObservationCrossSection2D_WhenGettingQuantitiesForObservationCrossSection2D_ThenExpectedQuantitiesAreReturned(bool useSalinity)
        {
            // Given
            var observationCrossSection2D = new ObservationCrossSection2D();

            // When
            string[] quantities = QuantityGenerator.GetQuantitiesForFeature(observationCrossSection2D, useSalinity).ToArray();

            // Then
            Assert.That(quantities.Length, Is.EqualTo(4));
            Assert.That(quantities.Contains("water_level"));
            Assert.That(quantities.Contains("water_depth"));
            Assert.That(quantities.Contains("discharge"));
            Assert.That(quantities.Contains("velocity"));
        }

        private IStructure GetWeirStubWithWeirFormulaType<TWeirFormulaType>()
            where TWeirFormulaType : IStructureFormula, new()
        {
            var weir = mocks.Stub<IStructure>();
            weir.Formula = new TWeirFormulaType();
            return weir;
        }
    }
}