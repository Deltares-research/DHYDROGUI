using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class WaterFlowFMModelHydroAreaExtensionsTest
    {
        private TemporaryDirectory modelDir;
        private TemporaryDirectory featuresDir;
        private WaterFlowFMModel model;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string testDataDir = TestHelper.GetTestFilePath(@"HydroAreaCollection\MduFileProjects\MduFileWithoutFeatureFileReferences\FlowFM");

            modelDir = new TemporaryDirectory();
            modelDir.CopyDirectoryToTempDirectory(testDataDir);

            featuresDir = new TemporaryDirectory();
            featuresDir.CopyDirectoryToTempDirectory(Path.Combine(testDataDir, "FeatureFiles"));

            string mduFilePath = Path.Combine(modelDir.Path, @"FlowFM\MDU\FlowFM.mdu");

            model = new WaterFlowFMModel();
            model.ImportFromMdu(mduFilePath);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ((IDisposable)modelDir).Dispose();
            ((IDisposable)featuresDir).Dispose();
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("myName", "myName")]
        [TestCase("mySubFolder/myName", "mySubFolder/myName")]
        [TestCase("mySubFolder/myName.ext", "mySubFolder/myName.ext")]
        [TestCase("mySubFolder/myName.ini", "mySubFolder/myName.ini")]
        public void GivenFeatureWithUnRootedGroupName_WhenUpdatingGroupName_ThenGroupNameWillNotChange(string groupName, string expectedGroupName)
        {
            // structures
            AssertUpdatedGroupName<Structure>(groupName, expectedGroupName);
            AssertUpdatedGroupName<Pump>(groupName, expectedGroupName);

            // other features
            AssertUpdatedGroupName<LandBoundary2D>(groupName, expectedGroupName);
            AssertUpdatedGroupName<GroupableFeature2DPolygon>(groupName, expectedGroupName);
            AssertUpdatedGroupName<ThinDam2D>(groupName, expectedGroupName);
            AssertUpdatedGroupName<FixedWeir>(groupName, expectedGroupName);
            AssertUpdatedGroupName<GroupableFeature2DPoint>(groupName, expectedGroupName);
            AssertUpdatedGroupName<ObservationCrossSection2D>(groupName, expectedGroupName);
            AssertUpdatedGroupName<BridgePillar>(groupName, expectedGroupName);
        }

        [Test]
        [TestCase("myFile.pli", "FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate01.pli", "FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate02.pli", "FlowFM_structures.ini")]
        public void GivenStructureWithGroupNameThatIsOutsideModelDir_WhenUpdatingGroupName_ThenGroupNameWillBeTheStructureFileName(string fileName, string expectedGroupName)
        {
            AssertUpdatedGroupNameForStructures(Path.Combine(featuresDir.Path, fileName), expectedGroupName);
        }

        [Test]
        [TestCase("myFile.ext", "myFile.ext")]
        [TestCase("FeatureFiles/myFile.ext", "myFile.ext")]
        public void GivenHydroAreaFeatureWithGroupNameThatIsOutsideModelDir_WhenUpdatingGroupName_ThenGroupNameWillBeTheFileName(string fileName, string expectedGroupName)
        {
            AssertUpdatedGroupNameForHydroAreaFeatures(Path.Combine(featuresDir.Path, fileName), expectedGroupName);
        }

        [Test]
        [TestCase("myFile.pli", "../FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate01.pli", "../FeatureFiles/FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate02.pli", "../FeatureFiles/FlowFM_structures.ini")]
        public void GivenStructureWithGroupNameThatIsAboveMduDir_WhenUpdatingGroupName_ThenGroupNameWillBeTheRelativePath(string fileName, string expectedGroupName)
        {
            AssertUpdatedGroupNameForStructures(Path.Combine(model.GetModelDirectory(), fileName), expectedGroupName);
        }

        [Test]
        [TestCase("myFile.ext", "../myFile.ext")]
        [TestCase("FeatureFiles/myFile.ext", "../FeatureFiles/myFile.ext")]
        public void GivenHydroAreaFeatureWithGroupNameThatIsAboveMduDir_WhenUpdatingGroupName_ThenGroupNameWillBeTheRelativePath(string fileName, string expectedGroupName)
        {
            AssertUpdatedGroupNameForHydroAreaFeatures(Path.Combine(model.GetModelDirectory(), fileName), expectedGroupName);
        }

        [Test]
        [TestCase("myFile.pli", "FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate01.pli", "FeatureFiles/FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate02.pli", "FeatureFiles/FlowFM_structures.ini")]
        public void GivenStructureWithGroupNameThatIsInMduSubDir_WhenUpdatingGroupName_ThenGroupNameWillBeTheRelativePath(string fileName, string expectedGroupName)
        {
            AssertUpdatedGroupNameForStructures(Path.Combine(model.GetMduDirectory(), fileName), expectedGroupName);
        }

        [Test]
        [TestCase("myFile.ext", "myFile.ext")]
        [TestCase("FeatureFiles/myFile.ext", "FeatureFiles/myFile.ext")]
        public void GivenHydroAreaFeatureWithGroupNameThatIsInMduSubDir_WhenUpdatingGroupName_ThenGroupNameWillBeTheRelativePath(string fileName, string expectedGroupName)
        {
            AssertUpdatedGroupNameForHydroAreaFeatures(Path.Combine(model.GetMduDirectory(), fileName), expectedGroupName);
        }

        [Test]
        public void GetFeaturesFromCategory_ReturnsExceptionForUnknownCategory()
        {
            // Given
            var area = new HydroArea();
            const string category = "Unknown";

            // When Then
            var ex = Assert.Throws<ArgumentException>(() => area.GetFeaturesFromCategory(category));
            Assert.AreEqual($"unknown category {category} used.", ex?.Message, "The exception message is different than expected");
        }

        [TestCaseSource(nameof(DifferentTestCaseData))]
        public void GetFeaturesFromCategory_ReturnsExpectedFeatures(HydroArea area, string category, IEnumerable<IFeature> areaFeatures)
        {
            // When
            IFeature[] retrievedFeatures = area.GetFeaturesFromCategory(category).ToArray();

            // Then
            Assert.AreEqual(areaFeatures, retrievedFeatures, "Incorrect features are retrieved by the method GetFeaturesFromCategory");
        }

        private static IEnumerable<TestCaseData> DifferentTestCaseData
        {
            get
            {
                HydroArea area = CreateHydroArea();

                yield return new TestCaseData(area, KnownFeatureCategories.Pumps, area.Pumps);
                yield return new TestCaseData(area, KnownFeatureCategories.Weirs, area.Structures.Where(w => w.Formula is SimpleWeirFormula));
                yield return new TestCaseData(area, KnownFeatureCategories.Gates, area.Structures.Where(w => w.Formula is SimpleGateFormula));
                yield return new TestCaseData(area, KnownFeatureCategories.GeneralStructures, area.Structures.Where(w => w.Formula is GeneralStructureFormula));
                yield return new TestCaseData(area, KnownFeatureCategories.ObservationPoints, area.ObservationPoints);
                yield return new TestCaseData(area, KnownFeatureCategories.ObservationCrossSections, area.ObservationCrossSections);
            }
        }

        private static HydroArea CreateHydroArea()
        {
            var rand = new Random();
            int k = rand.Next(0, 5);

            var area = new HydroArea();

            for (var i = 0; i < k; i++)
            {
                var pump = new Pump();
                var simpleWeir = new Structure() { Formula = new SimpleWeirFormula() };
                var gate = new Structure { Formula = new SimpleGateFormula() };
                var generalStructure = new Structure() { Formula = new GeneralStructureFormula() };
                var observationPoint = new GroupableFeature2DPoint();
                var observationCrossSection = new ObservationCrossSection2D();

                area.ObservationCrossSections.Add(observationCrossSection);
                area.ObservationPoints.Add(observationPoint);
                area.Pumps.Add(pump);
                area.Structures.Add(simpleWeir);
                area.Structures.Add(gate);
                area.Structures.Add(generalStructure);
            }

            return area;
        }

        private void AssertUpdatedGroupNameForStructures(string newGroupName, string expectedGroupName)
        {
            AssertUpdatedGroupName<Structure>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<Pump>(newGroupName, expectedGroupName);
        }

        private void AssertUpdatedGroupNameForHydroAreaFeatures(string newGroupName, string expectedGroupName)
        {
            AssertUpdatedGroupName<LandBoundary2D>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<GroupableFeature2DPolygon>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<ThinDam2D>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<FixedWeir>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<GroupableFeature2DPoint>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<ObservationCrossSection2D>(newGroupName, expectedGroupName);
        }

        private void AssertUpdatedGroupName<TFeature>(string newGroupName, string expectedGroupName)
            where TFeature : IGroupableFeature, new()
        {
            var structure = new TFeature { GroupName = newGroupName };
            structure.UpdateGroupName(model);

            Assert.That(structure.GroupName, Is.EqualTo(expectedGroupName));
        }
    }
}