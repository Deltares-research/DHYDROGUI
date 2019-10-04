using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
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
            var localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection/MduFileProjects"));
            mduFilePath = Path.Combine(localPath, @"MduFileWithoutFeatureFileReferences/FlowFM.mdu");
            fmModel = new WaterFlowFMModel(mduFilePath);
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
            CheckIfUpdateGroupNameGivesTheDesiredResult<Weir2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<Pump2D>(groupName, expectedGroupName);

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
            var parentDir = Directory.GetParent(Directory.GetParent(mduFilePath).FullName).FullName;
            CheckUpdatingNamesForStructures(fileName, expectedGroupName, parentDir);
        }

        [Test]
        [TestCase("myFile.ext", "myFile.ext")]
        [TestCase("FeatureFiles/myFile.ext", "myFile.ext")]
        public void GivenHydroAreaFeatureWithGroupNameThatIsNotInSubfolderOfMduFolder_WhenUpdatingGroupName_ThenGroupNameWillBeTheFileName(string fileName, string expectedGroupName)
        {
            var parentDir = Directory.GetParent(Directory.GetParent(mduFilePath).FullName).FullName;
            CheckUpdatingNamesForHydroAreaFeatures(fileName, expectedGroupName, parentDir);
        }

        [Test]
        [TestCase("myFile.pli", "FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate01.pli", "FeatureFiles/FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate02.pli", "FeatureFiles/FlowFM_structures.ini")]
        public void GivenStructureWithGroupNameThatIsInSubfolderOfMduFolder_WhenUpdatingGroupName_ThenGroupNameWillBeTheStructureFileName(string fileName, string expectedGroupName)
        {
            var parentDir = Directory.GetParent(mduFilePath).FullName;
            CheckUpdatingNamesForStructures(fileName, expectedGroupName, parentDir);
        }

        [Test]
        [TestCase("myFile.ext", "myFile.ext")]
        [TestCase("FeatureFiles/myFile.ext", "FeatureFiles/myFile.ext")]
        public void GivenHydroAreaFeatureWithGroupNameThatIsInSubfolderOfMduFolder_WhenUpdatingGroupName_ThenGroupNameWillBeTheRelativePath(string fileName, string expectedGroupName)
        {
            var parentDir = Directory.GetParent(mduFilePath).FullName;
            CheckUpdatingNamesForHydroAreaFeatures(fileName, expectedGroupName, parentDir);
        }

        [Test]
        public void GetFeaturesFromCategory_Pumps_ThenReturnOnlyPumpsOfTheArea()
        {
            // Given
            var area = new HydroArea();

            var pump = new Pump2D();
            var observationPoint = new GroupableFeature2DPoint();

            area.Pumps.Add(pump);
            area.ObservationPoints.Add(observationPoint);
            
            // When
            List<IFeature> features = area.GetFeaturesFromCategory(KnownFeatureCategories.Pumps).ToList();

            // Then
            Assert.AreEqual(1, features.Count, "Only one feature should have been returned");
            Assert.AreSame(pump, features.First(), "The pump of the area should have been returned");
        }

        [Test]
        public void GetFeaturesFromCategory_SimpleWeirs_ThenReturnOnlyWeirsWithSimpleWeirFormulaOfTheArea()
        {
            // Given
            var area = new HydroArea();

            var simpleWeir = new Weir2D {WeirFormula = new SimpleWeirFormula()};
            var observationPoint = new GroupableFeature2DPoint();

            area.Weirs.Add(simpleWeir);
            area.ObservationPoints.Add(observationPoint);

            // When
            List<IFeature> features = area.GetFeaturesFromCategory(KnownFeatureCategories.Weirs).ToList();

            // Then
            Assert.AreEqual(1, features.Count, "Only one feature should have been returned");
            Assert.AreSame(simpleWeir, features.First(), "The simple weir of the area should have been returned");
        }

        [Test]
        public void GetFeaturesFromCategoryGates_Gates_ThenReturnOnlyWeirsWithGatedWeirFormulaOfTheArea()
        {
            // Given
            var area = new HydroArea();

            var gate = new Weir2D { WeirFormula = new GatedWeirFormula() };
            var observationPoint = new GroupableFeature2DPoint();

            area.Weirs.Add(gate);
            area.ObservationPoints.Add(observationPoint);

            // When
            List<IFeature> features = area.GetFeaturesFromCategory(KnownFeatureCategories.Gates).ToList();

            // Then
            Assert.AreEqual(1, features.Count, "Only one feature should have been returned");
            Assert.AreSame(gate, features.First(), "The gate of the area should have been returned");
        }

        [Test]
        public void GetFeaturesFromCategory_GeneralStructures_ThenReturnOnlyWeirsWithGeneralStructureWeirFormulaOfTheArea()
        {
            // Given
            var area = new HydroArea();

            var generalStructure = new Weir2D { WeirFormula = new GeneralStructureWeirFormula() };
            var observationPoint = new GroupableFeature2DPoint();

            area.Weirs.Add(generalStructure);
            area.ObservationPoints.Add(observationPoint);

            // When
            List<IFeature> features = area.GetFeaturesFromCategory(KnownFeatureCategories.GeneralStructures).ToList();

            // Then
            Assert.AreEqual(1, features.Count, "Only one feature should have been returned");
            Assert.AreSame(generalStructure, features.First(), "The general structure of the area should have been returned");
        }

        [Test]
        public void GetFeaturesFromCategory_ObservationPoints_ThenReturnOnlyObservationPointsOfTheArea()
        {
            // Given
            var area = new HydroArea();

            var observationPoint = new GroupableFeature2DPoint();
            var pump = new Pump2D();

            area.ObservationPoints.Add(observationPoint);
            area.Pumps.Add(pump);

            // When
            List<IFeature> features = area.GetFeaturesFromCategory(KnownFeatureCategories.Observations).ToList();
            
            // Then
            Assert.AreEqual(1, features.Count, "Only one feature should have been returned");
            Assert.AreSame(observationPoint, features.First(), "The observation point of the area should have been returned");
        }

        [Test]
        public void GetFeaturesFromCategory_ObservationCrossSections_ThenReturnOnlyObservationCrossSectionsOfTheArea()
        {
            // Given
            var area = new HydroArea();

            var observationCrossSection = new ObservationCrossSection2D();
            var observationPoint = new GroupableFeature2DPoint();

            area.ObservationCrossSections.Add(observationCrossSection);
            area.ObservationPoints.Add(observationPoint);

            // When
            List<IFeature> features = area.GetFeaturesFromCategory(KnownFeatureCategories.CrossSections).ToList();

            // Then
            Assert.AreEqual(1, features.Count, "Only one feature should have been returned");
            Assert.AreSame(observationCrossSection, features.First(), "The observation cross section of the area should have been returned");
        }
        #region Helper methods

        private void CheckUpdatingNamesForStructures(string fileName, string expectedGroupName, string parentDir)
        {
            var groupName = Path.Combine(parentDir, fileName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<Weir2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<Pump2D>(groupName, expectedGroupName);
        }

        private void CheckUpdatingNamesForHydroAreaFeatures(string fileName, string expectedGroupName, string parentDir)
        {
            var groupName = Path.Combine(parentDir, fileName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<LandBoundary2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<GroupableFeature2DPolygon>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<ThinDam2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<FixedWeir>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<GroupableFeature2DPoint>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<ObservationCrossSection2D>(groupName, expectedGroupName);
        }

        private void CheckIfUpdateGroupNameGivesTheDesiredResult<T>(string groupName, string expectedGroupName) where T : IGroupableFeature
        {
            var gate = mocks.Stub<T>();
            gate.GroupName = groupName;
            mocks.ReplayAll();

            gate.UpdateGroupName(fmModel);

            Assert.That(gate.GroupName, Is.EqualTo(expectedGroupName));
        }
        
        #endregion
    }
}