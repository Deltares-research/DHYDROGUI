using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class WaterFlowFMModelHydroAreaExtensionsTest
    {
        private MockRepository mocks;
        private WaterFlowFMModel fmModel;
        private string mduFilePath;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection/MduFileProjects"));
            mduFilePath = Path.Combine(localPath, @"MduFileWithoutFeatureFileReferences/FlowFM.mdu");

            fmModel = new WaterFlowFMModel();
            fmModel.ImportFromMdu(mduFilePath);
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
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
            CheckIfUpdateGroupNameGivesTheDesiredResult<Structure>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<Pump>(groupName, expectedGroupName);

            // other features
            CheckIfUpdateGroupNameGivesTheDesiredResult<LandBoundary2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<GroupableFeature2DPolygon>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<ThinDam2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<FixedWeir>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<GroupableFeature2DPoint>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<ObservationCrossSection2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<BridgePillar>(groupName, expectedGroupName);
        }

        [Test]
        [TestCase("myFile.pli", "FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate01.pli", "MyFile_structures.ini")]
        [TestCase("FeatureFiles/gate02.pli", "FlowFM_structures.ini")]
        public void GivenStructureWithGroupNameThatIsNotInSubfolderOfMduFolder_WhenUpdatingGroupName_ThenGroupNameWillBeTheStructureFileName(string fileName, string expectedGroupName)
        {
            string parentDir = Directory.GetParent(Directory.GetParent(mduFilePath).FullName).FullName;
            CheckUpdatingNamesForStructures(fileName, expectedGroupName, parentDir);
        }

        [Test]
        [TestCase("myFile.ext", "myFile.ext")]
        [TestCase("FeatureFiles/myFile.ext", "myFile.ext")]
        public void GivenHydroAreaFeatureWithGroupNameThatIsNotInSubfolderOfMduFolder_WhenUpdatingGroupName_ThenGroupNameWillBeTheFileName(string fileName, string expectedGroupName)
        {
            string parentDir = Directory.GetParent(Directory.GetParent(mduFilePath).FullName).FullName;
            CheckUpdatingNamesForHydroAreaFeatures(fileName, expectedGroupName, parentDir);
        }

        [Test]
        [TestCase("myFile.pli", "FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate01.pli", "FeatureFiles/FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate02.pli", "FeatureFiles/FlowFM_structures.ini")]
        public void GivenStructureWithGroupNameThatIsInSubfolderOfMduFolder_WhenUpdatingGroupName_ThenGroupNameWillBeTheStructureFileName(string fileName, string expectedGroupName)
        {
            string parentDir = Directory.GetParent(mduFilePath).FullName;
            CheckUpdatingNamesForStructures(fileName, expectedGroupName, parentDir);
        }

        [Test]
        [TestCase("myFile.ext", "myFile.ext")]
        [TestCase("FeatureFiles/myFile.ext", "FeatureFiles/myFile.ext")]
        public void GivenHydroAreaFeatureWithGroupNameThatIsInSubfolderOfMduFolder_WhenUpdatingGroupName_ThenGroupNameWillBeTheRelativePath(string fileName, string expectedGroupName)
        {
            string parentDir = Directory.GetParent(mduFilePath).FullName;
            CheckUpdatingNamesForHydroAreaFeatures(fileName, expectedGroupName, parentDir);
        }

        [Test]
        public void GetFeaturesFromCategory_ReturnsExceptionForUnknownCategory()
        {
            // Given
            var area = new HydroArea();
            const string category = "Unknown";

            // When Then
            var ex =
                Assert.Throws<ArgumentException>(() => area.GetFeaturesFromCategory(category));
            Assert.AreEqual($"unknown category {category} used.", ex.Message,
                            "The exception message is different than expected");
        }

        [TestCaseSource(nameof(DifferentTestCaseData))]
        public void GetFeaturesFromCategory_ReturnsExpectedFeatures(HydroArea area, string category, IEnumerable<IFeature> areaFeatures)
        {
            // When
            IFeature[] retrievedFeatures = area.GetFeaturesFromCategory(category).ToArray();

            // Then
            Assert.AreEqual(areaFeatures, retrievedFeatures,
                            "Incorrect features are retrieved by the method GetFeaturesFromCategory");
        }

        #region Helper methods

        private static IEnumerable<TestCaseData> DifferentTestCaseData
        {
            get
            {
                HydroArea area = CreateHydroArea();

                yield return new TestCaseData(area, KnownFeatureCategories.Pumps, area.Pumps);
                yield return new TestCaseData(area, KnownFeatureCategories.Weirs, area.Weirs.Where(w => w.Formula is SimpleWeirFormula));
                yield return new TestCaseData(area, KnownFeatureCategories.Gates, area.Weirs.Where(w => w.Formula is GatedWeirFormula));
                yield return new TestCaseData(area, KnownFeatureCategories.GeneralStructures, area.Weirs.Where(w => w.Formula is GeneralStructureWeirFormula));
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
                var simpleWeir = new Structure() {Formula = new SimpleWeirFormula()};
                var gate = new Structure {Formula = new GatedWeirFormula()};
                var generalStructure = new Structure() {Formula = new GeneralStructureWeirFormula()};
                var observationPoint = new GroupableFeature2DPoint();
                var observationCrossSection = new ObservationCrossSection2D();

                area.ObservationCrossSections.Add(observationCrossSection);
                area.ObservationPoints.Add(observationPoint);
                area.Pumps.Add(pump);
                area.Weirs.Add(simpleWeir);
                area.Weirs.Add(gate);
                area.Weirs.Add(generalStructure);
            }

            return area;
        }

        private void CheckUpdatingNamesForStructures(string fileName, string expectedGroupName, string parentDir)
        {
            string groupName = Path.Combine(parentDir, fileName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<Structure>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<Pump>(groupName, expectedGroupName);
        }

        private void CheckUpdatingNamesForHydroAreaFeatures(string fileName, string expectedGroupName, string parentDir)
        {
            string groupName = Path.Combine(parentDir, fileName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<LandBoundary2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<GroupableFeature2DPolygon>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<ThinDam2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<FixedWeir>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<GroupableFeature2DPoint>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<ObservationCrossSection2D>(groupName, expectedGroupName);
        }

        private void CheckIfUpdateGroupNameGivesTheDesiredResult<T>(string groupName, string expectedGroupName) where T : IGroupableFeature, new()
        {
            var structure = new T {GroupName = groupName};

            structure.UpdateGroupName(fmModel);

            Assert.That(structure.GroupName, Is.EqualTo(expectedGroupName));
        }

        #endregion
    }
}