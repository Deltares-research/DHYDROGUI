using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Reader;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.CrossSections
{
    [TestFixture]
    public class CrossSectionFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(@"CrossSectionDefinitions_YZ.ini", "CrossSectionLocations_YZ.ini", CrossSectionType.YZ)]
        [TestCase(@"CrossSectionDefinitions_ZW.ini", "CrossSectionLocations_ZW.ini", CrossSectionType.ZW)]
        [TestCase(@"CrossSectionDefinitions_Standard.ini", "CrossSectionLocations_Standard.ini", CrossSectionType.Standard)]
        public void GiveAValidCrossSectionFiles_WhenReading_ThenCrossSectionsAreSetOnNetworkWithoutErrors(string definitionFileName, string locationFileName, CrossSectionType type)
        {
            var testdataDir = "ImportCrossSections";

            var definitionFilePath = TestHelper.GetTestFilePath(Path.Combine(testdataDir, definitionFileName));
            var locationFilePath = TestHelper.GetTestFilePath(Path.Combine(testdataDir, locationFileName));

            Assert.That(File.Exists(definitionFilePath));
            Assert.That(File.Exists(locationFilePath));

            var errorReport = new List<string>();

            Action<string, IList<string>> createAndAddErrorReport =
                (header, errorMessages) => errorReport.AddRange(errorMessages);

            var reader = new CrossSectionFileReader(createAndAddErrorReport);

            var network = new HydroNetwork();
            var branchName = "Channel1";
            network.Branches.Add(new Branch { Name = branchName });

            reader.Read(definitionFilePath, locationFilePath, network);

            var crossSections = network.CrossSections.ToList();

            Assert.NotNull(crossSections);
            Assert.AreEqual(2, crossSections.Count);

            var crossSectionName1 = "CrossSection1";
            var crossSectionName2 = "CrossSection2";

            Assert.That(crossSections.Exists(cs => cs.Name == crossSectionName1));
            Assert.That(crossSections.Exists(cs => cs.Name == crossSectionName2));

            var crossSection = network.CrossSections.First();

            Assert.AreEqual(0, errorReport.Count);

            Assert.AreEqual(1, network.SharedCrossSectionDefinitions.Count);
            Assert.AreEqual(crossSectionName1, network.SharedCrossSectionDefinitions.First().Name);
            Assert.AreEqual(crossSectionName1, crossSection.Definition.Name);
            Assert.AreEqual(crossSectionName1, crossSections.Last().Definition.Name);

            Assert.AreEqual(branchName, crossSection.Branch.Name);
            Assert.AreEqual(5.0, crossSection.Chainage);
            Assert.AreEqual(type, crossSection.CrossSectionType);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GiveAValidCrossSectionFilesOfGeomtreyBasedCrossSections_WhenReading_ThenCrossSectionsAreSetOnNetworkWithoutErrors()
        {
            var testdataDir = "ImportCrossSections";

            var definitionFilePath = TestHelper.GetTestFilePath(Path.Combine(testdataDir, "CrossSectionDefinitions_XYZ.ini"));
            var locationFilePath = TestHelper.GetTestFilePath(Path.Combine(testdataDir, "CrossSectionLocations_XYZ.ini"));

            Assert.That(File.Exists(definitionFilePath));
            Assert.That(File.Exists(locationFilePath));

            var errorReport = new List<string>();

            Action<string, IList<string>> createAndAddErrorReport =
                (header, errorMessages) => errorReport.AddRange(errorMessages);

            var reader = new CrossSectionFileReader(createAndAddErrorReport);

            var network = new HydroNetwork();
            var branchName = "Channel1";
            network.Branches.Add(new Branch { Name = branchName });

            reader.Read(definitionFilePath, locationFilePath, network);

            var crossSections = network.CrossSections.ToList();

            Assert.NotNull(crossSections);
            Assert.AreEqual(2, crossSections.Count);

            var crossSectionName1 = "CrossSection1";
            var crossSectionName2 = "CrossSection2";

            Assert.That(crossSections.Exists(cs => cs.Name == crossSectionName1));
            Assert.That(crossSections.Exists(cs => cs.Name == crossSectionName2));

            var crossSection = network.CrossSections.First();

            Assert.AreEqual(0, errorReport.Count);

            Assert.AreEqual(0, network.SharedCrossSectionDefinitions.Count);
            Assert.AreEqual(crossSectionName1, crossSection.Definition.Name);
            Assert.AreEqual(crossSectionName2, crossSections.Last().Definition.Name);

            Assert.AreEqual(branchName, crossSection.Branch.Name);
            Assert.AreEqual(5.0, crossSection.Chainage);
            Assert.AreEqual(CrossSectionType.GeometryBased, crossSection.CrossSectionType);
        }

        [Test]
        [Category(TestCategory.DataAccess)]

        public void GivenLocationReferencingNonExistingDefinitionInCrossSectionFiles_WhenReading_ThenErrorIsGiven()
        {
            var testdataDir = "ImportCrossSections";

            var definitionFilePath = TestHelper.GetTestFilePath(Path.Combine(testdataDir, "CrossSectionDefinitions_YZ_DefinitionMissing.ini"));
            var locationFilePath = TestHelper.GetTestFilePath(Path.Combine(testdataDir, "CrossSectionLocations_YZ_DefinitionMissing.ini"));

            Assert.That(File.Exists(definitionFilePath));
            Assert.That(File.Exists(locationFilePath));

            var errorReport = new List<string>();

            Action<string, IList<string>> createAndAddErrorReport =
                (header, errorMessages) => errorReport.AddRange(errorMessages);

            var reader = new CrossSectionFileReader(createAndAddErrorReport);

            var network = new HydroNetwork();
            var branchName = "Channel1";
            network.Branches.Add(new Branch { Name = branchName });

            reader.Read(definitionFilePath, locationFilePath, network);

            Assert.AreEqual(2, errorReport.Count);
            Assert.AreEqual(0, network.CrossSections.Count());
            Assert.That(errorReport.All(e => e.Contains($"has no definition in the definition file: {definitionFilePath}")));
        }

        [Test]
        [Category(TestCategory.DataAccess)]

        public void GivenCrossSectionFilesReferencingBranchThatDoesNotExist_WhenReading_ThenErrorIsGiven()
        {
            var testdataDir = "ImportCrossSections";

            var definitionFilePath = TestHelper.GetTestFilePath(Path.Combine(testdataDir, "CrossSectionDefinitions_YZ.ini"));
            var locationFilePath = TestHelper.GetTestFilePath(Path.Combine(testdataDir, "CrossSectionLocations_YZ.ini"));

            Assert.That(File.Exists(definitionFilePath));
            Assert.That(File.Exists(locationFilePath));

            var errorReport = new List<string>();

            Action<string, IList<string>> createAndAddErrorReport =
                (header, errorMessages) => errorReport.AddRange(errorMessages);

            var reader = new CrossSectionFileReader(createAndAddErrorReport);

            var network = new HydroNetwork();

            var branchNameInFile = "Channel1";

            network.Branches.Add(new Branch { Name = "SomeOtherChannel" });

            reader.Read(definitionFilePath, locationFilePath, network);

            Assert.AreEqual(2, errorReport.Count);
            Assert.AreEqual(0, network.CrossSections.Count());
            Assert.That(errorReport.All(e => e.Contains($"has a branch ID ({branchNameInFile}) which is not available in the model.")));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidCrossSectionDefinitionFile_WhenReadingGroundLayerData_ThenGroundLayerDataObjectShouldBeReturnedWithoutErrors()
        {
            // Given
            var filePath = TestHelper.GetTestFilePath(@"ImportCrossSections\CrossSectionDefinitions_Standard.ini");
            Assert.That(File.Exists(filePath));

            // When
            var groundLayerDataObjects = CrossSectionDefinitionFileReader.ReadGroundLayerData(filePath).ToArray();

            // Then
            Assert.That(groundLayerDataObjects.Length, Is.EqualTo(1));
            var groundLayerData = groundLayerDataObjects.FirstOrDefault();
            Assert.IsNotNull(groundLayerData);
            Assert.That(groundLayerData.CrossSectionDefinitionId, Is.EqualTo("CrossSection1"));
            Assert.IsTrue(groundLayerData.GroundLayerUsed, "GroundLayer is not used");
            Assert.That(groundLayerData.GroundLayerThickness, Is.EqualTo(2.0));
        }

        [Test]
        public void GivenCrossSectionFilesThatDescribeTwoDefinitionsReferencingTheSameSharedDefinition_WhenReadingCrossSections_ThenNetworkIsAsExpected()
        {
            // Given
            var definitionsFilePath = TestHelper.GetTestFilePath(@"FileReaders\CrossSectionFileReaderTest\CrossSectionDefinitions.ini");
            var locationsFilePath = TestHelper.GetTestFilePath(@"FileReaders\CrossSectionFileReaderTest\CrossSectionLocations.ini");
            
            // When
            void ErrorMessageHandling(string s, IList<string> list)
            {
            }

            var network = new HydroNetwork();
            network.Branches.Add(new Channel {Name = "Channel1"}); // This channel is defined in the file at location locationsFilePath, so we add it

            var reader = new CrossSectionFileReader(ErrorMessageHandling);
            reader.Read(definitionsFilePath, locationsFilePath, network);

            // Then
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(1));
            Assert.That(network.CrossSections.Count, Is.EqualTo(2));

            // Check that all cross sections in the network have definitions that refer to the shared definition in the network
            var sharedDefinitionInNetwork = network.SharedCrossSectionDefinitions.FirstOrDefault();
            var proxyDefinitions = network.CrossSections.Select(cs => cs.Definition as CrossSectionDefinitionProxy).ToArray();
            proxyDefinitions.ForEach(proxy =>
            {
                Assert.IsNotNull(proxy.InnerDefinition);
                Assert.That(proxy.InnerDefinition, Is.EqualTo(sharedDefinitionInNetwork));
            });

            // Check that level shifts have been imported correctly
            Assert.That(proxyDefinitions.Any(proxy => Math.Abs(proxy.LevelShift - 88.0) < double.Epsilon));
            Assert.That(proxyDefinitions.Any(proxy => Math.Abs(proxy.LevelShift - 22.0) < double.Epsilon));
        }
    }
}
