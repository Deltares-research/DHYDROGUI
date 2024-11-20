using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.MapLayerProviders;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.MapLayerProviders
{
    [TestFixture]
    public class RainfallRunoffMapLayerProviderTest
    {
        [Test]
        public void CreatesCustomLayerForCatchments()
        {
            var model = new RainfallRunoffModel();
            model.OutputSettings.GetEngineParameter(QuantityType.GroundwaterLevel, ElementSet.UnpavedElmSet).IsEnabled = true;

            var layerProvider = new RainfallRunoffMapLayerProvider();

            OutputMapLayerData outputFolder = layerProvider.ChildLayerObjects(model).OfType<OutputMapLayerData>().First();
            OutputCoverageGroupMapLayerData outputCoverageGroup = layerProvider.ChildLayerObjects(outputFolder).OfType<OutputCoverageGroupMapLayerData>().Single(g => g.Name == "Unpaved");
            ICoverage coverage = model.OutputCoverages.Skip(1).First();
            Assert.IsTrue(layerProvider.ChildLayerObjects(outputCoverageGroup).Contains(coverage));
            Assert.IsTrue(layerProvider.CanCreateLayerFor(coverage, outputCoverageGroup));

            var createdLayer = (FeatureCoverageLayer)layerProvider.CreateLayer(coverage, outputCoverageGroup);
            Assert.IsNull(createdLayer.Renderer.GeometryForFeatureDelegate);
        }

        [Test]
        public void CreateLayer_DataNull_ThrownArgumentNullException()
        {
            // Setup
            var mapLayerProvider = new RainfallRunoffMapLayerProvider();

            // Call
            void Call() => mapLayerProvider.CreateLayer(null, new object());

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("data"));
        }

        [Test]
        public void CreateLayer_WithUnsupportedData_ThrownNotSupportedException()
        {
            // Setup
            var mapLayerProvider = new RainfallRunoffMapLayerProvider();

            // Call
            void Call() => mapLayerProvider.CreateLayer(new object(), null);

            // Assert
            Assert.That(Call, Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void CreateLayer_WithRainfallRunoffModel_ReturnsTheCorrectLayer()
        {
            // Setup
            var mapLayerProvider = new RainfallRunoffMapLayerProvider();
            var rainfallRunoffModel = new RainfallRunoffModel();

            // Call
            ILayer layer = mapLayerProvider.CreateLayer(rainfallRunoffModel, null);

            // Assert
            var groupLayer = layer as GroupLayer;
            Assert.That(groupLayer, Is.Not.Null);
            Assert.That(groupLayer.Name, Is.EqualTo(rainfallRunoffModel.Name));
            Assert.That(groupLayer.LayersReadOnly, Is.True);
            Assert.That(groupLayer.NameIsReadOnly, Is.True);
        }

        [Test]
        public void CreateLayer_WithOutputMapLayerData_ReturnsTheCorrectLayer()
        {
            // Setup
            var mapLayerProvider = new RainfallRunoffMapLayerProvider();
            var model = new RainfallRunoffModel();
            var outputMapLayerData = new OutputMapLayerData(model);

            // Call
            ILayer layer = mapLayerProvider.CreateLayer(outputMapLayerData, null);

            // Assert
            var groupLayer = layer as GroupLayer;
            Assert.That(groupLayer, Is.Not.Null);
            Assert.That(groupLayer.Name, Is.EqualTo("Output"));
            Assert.That(groupLayer.LayersReadOnly, Is.True);
            Assert.That(groupLayer.NameIsReadOnly, Is.True);
        }

        [Test]
        public void CreateLayer_WithFeatureCoverage_WithoutCategory_ThrowsArgumentException()
        {
            // Setup
            var mapLayerProvider = new RainfallRunoffMapLayerProvider();
            var featureCoverage = new FeatureCoverage
            {
                Name = "some_coverage_name",
                CoordinateSystem = Substitute.For<ICoordinateSystem>()
            };

            // Call
            void Call() => mapLayerProvider.CreateLayer(featureCoverage, null);

            // Assert
            Assert.That(Call, Throws.ArgumentException
                                    .With.Property(nameof(ArgumentException.Message))
                                    .EqualTo("Coverage layer cannot be created for coverage with name without a category. Coverage name: some_coverage_name"));
        }

        [Test]
        public void CreateLayer_WithFeatureCoverage_ReturnsTheCorrectLayer()
        {
            // Setup
            var mapLayerProvider = new RainfallRunoffMapLayerProvider();
            var featureCoverage = new FeatureCoverage
            {
                Name = "some_coverage_name (category)",
                CoordinateSystem = Substitute.For<ICoordinateSystem>()
            };

            // Call
            ILayer layer = mapLayerProvider.CreateLayer(featureCoverage, null);

            // Assert
            var featureCoverageLayer = layer as FeatureCoverageLayer;
            Assert.That(featureCoverageLayer, Is.Not.Null);
            Assert.That(featureCoverageLayer.FeatureCoverage, Is.SameAs(featureCoverage));
            Assert.That(featureCoverageLayer.Name, Is.EqualTo("some_coverage_name"));
            Assert.That(featureCoverageLayer.DataSource.CoordinateSystem, Is.SameAs(featureCoverage.CoordinateSystem));
            Assert.That(featureCoverageLayer.Map, Is.Null);

            Assert.That(featureCoverageLayer.Visible, Is.False);
            Assert.That(featureCoverageLayer.NameIsReadOnly, Is.True);
            Assert.That(featureCoverageLayer.AutoUpdateThemeOnDataSourceChanged, Is.True);
        }

        [Test]
        [TestCaseSource(nameof(CanCreateLayerForCases))]
        public void CanCreateLayerFor_ReturnsCorrectResult(object data, object parentData, bool expResult)
        {
            // Setup
            var mapLayerProvider = new RainfallRunoffMapLayerProvider();

            // Call
            bool result = mapLayerProvider.CanCreateLayerFor(data, parentData);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        [Test]
        public void ChildLayerObjects_DataNull_ThrowsArgumentNullException()
        {
            // Setup
            var mapLayerProvider = new RainfallRunoffMapLayerProvider();

            // Call
            void Call() => mapLayerProvider.ChildLayerObjects(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("data"));
        }

        [Test]
        public void ChildLayerObjects_WithUnsupportedData_ThrownNotSupportedException()
        {
            // Setup
            var mapLayerProvider = new RainfallRunoffMapLayerProvider();

            // Call
            void Call() => mapLayerProvider.ChildLayerObjects(new object());

            // Assert
            Assert.That(Call, Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void ChildLayerObjects_WithRainfallRunoffModel_ReturnsCorrectChildLayerObjects()
        {
            // Setup
            var mapLayerProvider = new RainfallRunoffMapLayerProvider();
            var model = new RainfallRunoffModel();

            // Call
            List<object> childLayerObjects = mapLayerProvider.ChildLayerObjects(model).ToList();

            // Assert
            Assert.That(childLayerObjects, Has.Count.EqualTo(2));
            Assert.That(childLayerObjects[0], Is.SameAs(model.Basin));

            var outputMapLayerData = childLayerObjects[1] as OutputMapLayerData;
            Assert.That(outputMapLayerData, Is.Not.Null);
            Assert.That(outputMapLayerData.Model, Is.SameAs(model));
            Assert.That(outputMapLayerData.Name, Is.SameAs("Output"));
        }

        [Test]
        public void ChildLayerObjects_OutputMapLayerData_ReturnsCorrectChildLayerObjects()
        {
            // Setup
            var mapLayerProvider = new RainfallRunoffMapLayerProvider();
            var model = new RainfallRunoffModel();
            var outputMapLayerData = new OutputMapLayerData(model);

            // Call
            object[] childLayerObjects = mapLayerProvider.ChildLayerObjects(outputMapLayerData).ToArray();

            // Assert
            Assert.That(childLayerObjects, Is.Not.Empty);
            Assert.That(childLayerObjects, Has.Length.EqualTo(11));

            OutputCoverageGroupMapLayerData[] outputCoverageGroups = childLayerObjects.OfType<OutputCoverageGroupMapLayerData>().ToArray();
            Assert.That(outputCoverageGroups, Has.Length.EqualTo(11));
            Assert.That(outputCoverageGroups[0].Name, Is.EqualTo("Boundaries"));
            Assert.That(outputCoverageGroups[1].Name, Is.EqualTo("Unpaved"));
            Assert.That(outputCoverageGroups[2].Name, Is.EqualTo("Paved"));
            Assert.That(outputCoverageGroups[3].Name, Is.EqualTo("Greenhouse"));
            Assert.That(outputCoverageGroups[4].Name, Is.EqualTo("Open water"));
            Assert.That(outputCoverageGroups[5].Name, Is.EqualTo("Sacramento"));
            Assert.That(outputCoverageGroups[6].Name, Is.EqualTo("HBV"));
            Assert.That(outputCoverageGroups[7].Name, Is.EqualTo("WWTP"));
            Assert.That(outputCoverageGroups[8].Name, Is.EqualTo("Balances"));
            Assert.That(outputCoverageGroups[9].Name, Is.EqualTo("Links"));
            Assert.That(outputCoverageGroups[10].Name, Is.EqualTo("NWRW"));
        }

        [Test]
        public void ChildLayerObjects_OutputCoverageGroupMapLayerData_ReturnsCorrectChildLayerObjects()
        {
            // Setup
            var mapLayerProvider = new RainfallRunoffMapLayerProvider();

            var coverages = new List<ICoverage>
            {
                Substitute.For<ICoverage>(),
                Substitute.For<ICoverage>(),
                Substitute.For<ICoverage>()
            };
            var outputCoverageGroupMapLayerData = new OutputCoverageGroupMapLayerData("some_name", coverages);

            // Call
            object[] childLayerObjects = mapLayerProvider.ChildLayerObjects(outputCoverageGroupMapLayerData).ToArray();

            // Assert
            Assert.That(childLayerObjects, Is.EqualTo(coverages));
        }

        private static IEnumerable<TestCaseData> CanCreateLayerForCases()
        {
            var model = new RainfallRunoffModel();
            var outputMapLayerData = new OutputMapLayerData(model);
            var outputCoverageGroupMapLayerData = new OutputCoverageGroupMapLayerData("some_name", Enumerable.Empty<ICoverage>());
            var featureCoverage = Substitute.For<IFeatureCoverage>();

            yield return new TestCaseData(model, null, true);
            yield return new TestCaseData(outputMapLayerData, null, true);
            yield return new TestCaseData(outputCoverageGroupMapLayerData, null, true);
            yield return new TestCaseData(featureCoverage, outputCoverageGroupMapLayerData, true);

            yield return new TestCaseData(featureCoverage, null, false);
            yield return new TestCaseData(new object(), new object(), false);
            yield return new TestCaseData(null, null, false);
        }
    }
}