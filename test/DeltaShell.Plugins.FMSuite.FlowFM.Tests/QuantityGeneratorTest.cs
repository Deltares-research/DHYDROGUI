using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Hydro.GroupableFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class QuantityGeneratorTest
    {
        [Test]
        [TestCaseSource(nameof(GetArgumentNullCases))]
        public void ArgumentsNull_ThrowsArgumentNullException(
            IFeature feature, IEnumerable<string> tracerDefinitions, IEnumerable<SourceAndSink> sourceAndSinks)
        {
            void Call() => _ = QuantityGenerator.GetQuantitiesForFeature(feature, true, true, tracerDefinitions, sourceAndSinks)
                                                .ToArray();

            Assert.That(Call, Throws.ArgumentNullException);
        }

        private static IEnumerable<TestCaseData> GetArgumentNullCases()
        {
            var feature = Substitute.For<IFeature>();
            IEnumerable<string> emptyTracers = Enumerable.Empty<string>();
            IEnumerable<SourceAndSink> emptySourcesAndSinks = Enumerable.Empty<SourceAndSink>();

            yield return new TestCaseData(null, emptyTracers, emptySourcesAndSinks);
            yield return new TestCaseData(feature, null, emptySourcesAndSinks);
            yield return new TestCaseData(feature, emptyTracers, null);
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases))]
        public void ReturnExpectedQuantitiesForGivenFeature(IFeature feature, IEnumerable<string> expectedQuantities)
        {
            IEnumerable<string> quantities = QuantityGenerator.GetQuantitiesForFeature(
                feature, true, true, Enumerable.Empty<string>(), Enumerable.Empty<SourceAndSink>());

            Assert.That(quantities, Is.EqualTo(expectedQuantities));
        }

        private static IEnumerable<TestCaseData> GetTestCases()
        {
            yield return new TestCaseData(new Pump(), new[] { "Capacity" });

            var simpleWeir2D = new Structure { Formula = new SimpleWeirFormula() };
            yield return new TestCaseData(simpleWeir2D, new[] { "CrestLevel" });

            var gate = new Structure { Formula = new SimpleGateFormula() };
            yield return new TestCaseData(gate, new[] { "CrestLevel", "GateHeight", "GateLowerEdgeLevel", "GateOpeningWidth" });

            var generalStructure2D = new Structure { Formula = new GeneralStructureFormula() };
            yield return new TestCaseData(generalStructure2D, new[] { "CrestLevel", "GateHeight", "GateLowerEdgeLevel", "GateOpeningWidth" });

            yield return new TestCaseData(new ObservationCrossSection2D(), new[] { "discharge", "velocity", "water_level", "water_depth" });
        }

        [Test]
        [TestCaseSource(nameof(GetObservationPoint2DCases))]
        public void ReturnExpectedQuantitiesForObservationPoint2D(
            GroupableFeature2DPoint observationPoint,
            bool useSalinity,
            bool useTemperature,
            IEnumerable<string> tracerDefinitions,
            IEnumerable<string> expectedQuantities
        )
        {
            IEnumerable<string> quantities = QuantityGenerator.GetQuantitiesForFeature(
                observationPoint, useSalinity, useTemperature, tracerDefinitions, Enumerable.Empty<SourceAndSink>());

            Assert.That(quantities, Is.EqualTo(expectedQuantities));
        }

        private static IEnumerable<TestCaseData> GetObservationPoint2DCases()
        {
            var observationPoint = new GroupableFeature2DPoint();
            var tracers = new[] { "tracer1", "tracer2" };
            IEnumerable<string> noTracers = Enumerable.Empty<string>();

            yield return new TestCaseData(observationPoint, false, false, noTracers, new[] { "water_level", "water_depth", "velocity", "discharge" });
            yield return new TestCaseData(observationPoint, true, false, noTracers, new[] { "water_level", "salinity", "water_depth", "velocity", "discharge" });
            yield return new TestCaseData(observationPoint, false, true, noTracers, new[] { "water_level", "temperature", "water_depth", "velocity", "discharge" });
            yield return new TestCaseData(observationPoint, false, false, tracers, new[] { "water_level", "water_depth", "velocity", "discharge", "tracer1", "tracer2" });
            yield return new TestCaseData(observationPoint, true, true, tracers, new[] { "water_level", "salinity", "temperature", "water_depth", "velocity", "discharge", "tracer1", "tracer2" });
        }

        [Test]
        public void ReturnExpectedQuantitiesForSourceAndSinksFeatures()
        {
            // Setup
            var feature = new Feature2D();
            var sourceAndSink = new SourceAndSink() { Feature = feature };
            SourceAndSink[] sourceAndSinks = { sourceAndSink };

            // Call
            IEnumerable<string> quantities = QuantityGenerator.GetQuantitiesForFeature(
                feature, false, false, Enumerable.Empty<string>(), sourceAndSinks);

            // Assert
            var expectedQuantities = new[] { "discharge", "change_in_salinity", "change_in_temperature" };
            Assert.That(quantities, Is.EqualTo(expectedQuantities));
        }
    }
}