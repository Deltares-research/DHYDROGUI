using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Plugins.ImportExport.Sobek.Properties;
using DeltaShell.Sobek.Readers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekCrossSectionsImporterTest
    {
        private readonly SobekFileNames sobekFileNames = new SobekFileNames();

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportCrossSections()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\Network.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork,hydroNetwork, new IPartialSobekImporter[] {new SobekBranchesImporter(),new SobekCrossSectionsImporter()});

            importer.Import();

            Assert.AreEqual(52, hydroNetwork.CrossSections.Count());
            Assert.AreEqual("P_CoqR_21730", hydroNetwork.CrossSections.First().Name);
            Assert.AreEqual(0, hydroNetwork.SharedCrossSectionDefinitions.Count());
            Assert.IsTrue(hydroNetwork.CrossSections.All(cs => !cs.Definition.IsProxy));
            Assert.AreEqual(0, hydroNetwork.CrossSectionSectionTypes.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportCrossSectionsWithScientificNotationTools9637()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekCrossSectionsImporterTest).Assembly, @"TOOLS963.lit\1\NETWORK.TP");
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();

            Assert.AreEqual(2, hydroNetwork.CrossSections.Count());
            var crossSectionWithScientificNotation = hydroNetwork.CrossSections.First(cs => cs.Name == "3");
            var crossSectionWithNormalNotation = hydroNetwork.CrossSections.First(cs => cs.Name == "11");

            Assert.AreEqual(7.69938720957774E-05, crossSectionWithScientificNotation.Chainage, 1e-9);
            Assert.AreEqual(5437.30926751243, crossSectionWithNormalNotation.Chainage, 1e-9);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportCrossSectionsWithStrangeSymbolsInNames()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\FHM2011F.lit\1\Network.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();

            Assert.AreEqual(2551, hydroNetwork.CrossSections.Count());
            Assert.AreEqual(38, hydroNetwork.SharedCrossSectionDefinitions.Count());
            Assert.AreEqual("KA45", hydroNetwork.CrossSections.First().Name);
            Assert.IsTrue(hydroNetwork.CrossSections.Any(cs => cs.Name == "bPlu_11.5*_14"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportCrossSectionsOfPo()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\POup_GV.lit\7\Network.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();

            Assert.AreEqual(4153, hydroNetwork.CrossSections.Count());
            Assert.AreEqual(15, hydroNetwork.SharedCrossSectionDefinitions.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportCrossSectionsToExistingNetwork()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\Network.TP";
            var hydroNetwork = PartialSobekImporterTestHelper.GetTestNetwork();

            var csName = hydroNetwork.CrossSections.First().Name;

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();


            Assert.IsNotNull(hydroNetwork.CrossSections.FirstOrDefault(cs => cs.Name == csName));

            Assert.AreEqual(53, hydroNetwork.CrossSections.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateExistingCrossSectionOnAnotherBranch()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\Network.TP";

            var hydroNetwork = PartialSobekImporterTestHelper.GetTestNetwork();
            var branch = hydroNetwork.Branches.First();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();

            Assert.AreEqual(53, hydroNetwork.CrossSections.Count());

            var firstCrossSection = hydroNetwork.CrossSections.Last(); //the first cs is part of  GetTestNetwork

            var branchOfCS = firstCrossSection.Branch;
            firstCrossSection.Branch = branch;

            Assert.AreNotSame(branchOfCS, firstCrossSection.Branch);

            importer.Import();

            Assert.AreEqual(53, hydroNetwork.CrossSections.Count());
            Assert.AreSame(branchOfCS, hydroNetwork.CrossSections.Last().Branch);
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateExistingCrossSection()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\Network.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();

            Assert.AreEqual(52, hydroNetwork.CrossSections.Count());

            var firstCrossSection = hydroNetwork.CrossSections.First();

            var longName = firstCrossSection.LongName;

            firstCrossSection.LongName = "changedCHANGED";

            importer.Import();

            Assert.AreEqual(52, hydroNetwork.CrossSections.Count());
            Assert.AreEqual(longName, hydroNetwork.CrossSections.First().LongName);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportAllCrossSectionTypes()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\StandardCrossSections.lit\10\Network.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();

            Assert.AreEqual(22, hydroNetwork.CrossSections.Count()); 
            //needs to be 12 now instead of 11 because we will retrieve height and width of a closed rectangular (id=19) from the table (5x10).
            Assert.AreEqual(12, hydroNetwork.CrossSections.Where(t => t.CrossSectionType == CrossSectionType.Standard).Count());


            var definition = hydroNetwork.CrossSections.Where(
                cs => cs.CrossSectionType == CrossSectionType.ZW && !cs.Definition.IsProxy).Select(
                    cs => cs.Definition as CrossSectionDefinitionZW).First();

            Assert.AreEqual(0.0, definition.Sections[0].MinY);
            Assert.AreEqual(2.0, definition.Sections[0].MaxY);
        }

        [Test]
        public void GivenSOBEK2CrossSectionLocationMappingAndDefinition_WhenSobekCrossSectionsImporterImport_ThanCrossSectionSetOnSewerConnection()
        {
            SobekCrossSectionsImporter sobekCrossSectionsImporter = new SobekCrossSectionsImporter();
            
            ISewerConnection sewerConnection = new SewerConnection();
            sewerConnection.Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 10) });
            sewerConnection.Name = "1";

            using (var tempDirectory = new TemporaryDirectory())
            {
                GenerateCustomCrossSectionDefinitionLocationFile(tempDirectory);
                GenerateCustomCrossSectionDefinitionMappingFile(tempDirectory);

                const string crossSectionDefinitionIdName = "MyCrossSectionIdName";
                GenerateCustomCrossSectionDefinitionsFile(tempDirectory, crossSectionDefinitionIdName);

                using (WaterFlowFMModel model = new WaterFlowFMModel())
                {
                    model.Network.Branches.Add(sewerConnection);
                    sobekCrossSectionsImporter.TargetObject = model;
                    TypeUtils.SetField(sobekCrossSectionsImporter, "baseDir", tempDirectory.Path);

                    sobekCrossSectionsImporter.Import();
                    Assert.That(sewerConnection.CrossSectionDefinitionName, Is.EqualTo(crossSectionDefinitionIdName));
                    Assert.That(sewerConnection.CrossSection.Name, Is.EqualTo("cross section"));
                }
            }
        }
        

        [Test]
        public void GivenSOBEK2CrossSectionLocationMappingAndDefinition_WhenSobekCrossSectionsImporterImport_ThanCrossSectionSetOnPipe()
        {
            SobekCrossSectionsImporter sobekCrossSectionsImporter = new SobekCrossSectionsImporter();

            IPipe pipe = new Pipe();
            pipe.Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 10) });
            pipe.Name = "1";

            using (var tempDirectory = new TemporaryDirectory())
            {
                GenerateCustomCrossSectionDefinitionLocationFile(tempDirectory);
                GenerateCustomCrossSectionDefinitionMappingFile(tempDirectory);

                const string crossSectionDefinitionIdName = "MyCrossSectionIdName";
                GenerateCustomCrossSectionDefinitionsFile(tempDirectory, crossSectionDefinitionIdName);

                using (WaterFlowFMModel model = new WaterFlowFMModel())
                {
                    model.Network.Branches.Add(pipe);
                    sobekCrossSectionsImporter.TargetObject = model;
                    TypeUtils.SetField(sobekCrossSectionsImporter, "baseDir", tempDirectory.Path);
                    sobekCrossSectionsImporter.Import();
                    Assert.That(pipe.CrossSectionDefinitionName, Is.EqualTo(crossSectionDefinitionIdName));
                    Assert.That(pipe.CrossSection.Name, Is.EqualTo("cross section"));
                }
            }
        }

        [Test]
        public void GivenSOBEK2CrossSectionLocationMappingWithoutDefinition_WhenSobekCrossSectionsImporterImport_ThanDefaultCrossSectionSetOnPumpSewerConnection()
        {
            SobekCrossSectionsImporter sobekCrossSectionsImporter = new SobekCrossSectionsImporter();
            ISewerConnection sewerConnection = new SewerConnection();
            sewerConnection.Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 10) });
            sewerConnection.Name = "1";
            using (var tempDirectory = new TemporaryDirectory())
            {
                GenerateCustomCrossSectionDefinitionLocationFile(tempDirectory);
                GenerateCustomCrossSectionDefinitionMappingFile(tempDirectory);

                using (WaterFlowFMModel model = new WaterFlowFMModel())
                {
                    model.Network.Branches.Add(sewerConnection);
                    sobekCrossSectionsImporter.TargetObject = model;
                    TypeUtils.SetField(sobekCrossSectionsImporter, "baseDir", tempDirectory.Path);
                    TestHelper.AssertAtLeastOneLogMessagesContains(() =>
                    {
                        sobekCrossSectionsImporter.Import();
                        Assert.That(sewerConnection.CrossSectionDefinitionName, Is.EqualTo(SewerCrossSectionDefinitionFactory.DefaultPumpSewerStructureProfileName));
                        Assert.That(sewerConnection.CrossSection.Definition.Name, Is.EqualTo(SewerCrossSectionDefinitionFactory.DefaultPumpSewerStructureProfileName));
                        Assert.That(sewerConnection.CrossSection.Name, Is.EqualTo("SewerProfile_1"));
                    }, Resources.SobekCrossSectionsImporter_AddCrossSections_Definition_with_the_following_ids_were_not_found__ignored__Using_default);
                }
            }
        }

        [Test]
        public void GivenSOBEK2CrossSectionLocationMappingWithoutKnownDefinitionType_WhenSobekCrossSectionsImporterImport_ThanDefaultCrossSectionSetOnWeirSewerConnection()
        {
            SobekCrossSectionsImporter sobekCrossSectionsImporter = new SobekCrossSectionsImporter();
            SewerConnection sewerConnection = new SewerConnection();
            sewerConnection.Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 10) });
            sewerConnection.Name = "1";
            TypeUtils.SetField(sewerConnection, "specialConnectionType", SewerConnectionSpecialConnectionType.Weir);
            using (var tempDirectory = new TemporaryDirectory())
            {
                GenerateCustomCrossSectionDefinitionLocationFile(tempDirectory);
                GenerateCustomCrossSectionDefinitionMappingFile(tempDirectory);

                const string crossSectionDefinitionIdName = "MyCrossSectionIdName";
                GenerateCustomCrossSectionDefinitionsFile(tempDirectory, crossSectionDefinitionIdName, 801);

                using (WaterFlowFMModel model = new WaterFlowFMModel())
                {
                    model.Network.Branches.Add(sewerConnection);
                    sobekCrossSectionsImporter.TargetObject = model;
                    TypeUtils.SetField(sobekCrossSectionsImporter, "baseDir", tempDirectory.Path);
                    sobekCrossSectionsImporter.Import();
                    Assert.That(sewerConnection.CrossSection.Definition.Name, Is.EqualTo(SewerCrossSectionDefinitionFactory.DefaultWeirSewerStructureProfileName));
                    Assert.That(sewerConnection.CrossSectionDefinitionName, Is.EqualTo(SewerCrossSectionDefinitionFactory.DefaultWeirSewerStructureProfileName));
                    Assert.That(sewerConnection.CrossSection.Name, Is.Not.EqualTo(crossSectionDefinitionIdName));
                    Assert.That(sewerConnection.CrossSection.Name, Is.EqualTo("SewerProfile_1"));
                }
            }
        }

        private void GenerateCustomCrossSectionDefinitionsFile(TemporaryDirectory tempDirectory, string crossSectionDefinitionIdName, int crossSectionDefinitionType = 1)
        {
            string profile = $"CRDS id 'DefinitionId' nm '{crossSectionDefinitionIdName}' ty {crossSectionDefinitionType} wm 45 w1 30 w2 0 sw 9.9999e+009 bl -3 lt lw dk 0 dc 9.9999e+009 db 9.9999e+009 df 9.9999e+009 dt 9.9999e+009 bw 45 bs 3 aw 75 rd 9.9999e+009 ll 9.9999e+009 rl 9.9999e+009 lw 9.9999e+009 rw 9.9999e+009 crds\r\n";
            var customCrsDefFile = Path.Combine(tempDirectory.Path, sobekFileNames.SobekProfileDefinitionsFileName);
            File.WriteAllText(customCrsDefFile, profile);
        }

        private void GenerateCustomCrossSectionDefinitionMappingFile(TemporaryDirectory tempDirectory)
        {
            var customCrsDefMappingFile = Path.Combine(tempDirectory.Path, sobekFileNames.SobekProfileDataFileName);
            File.WriteAllText(customCrsDefMappingFile, "CRSN id 'LocationId' di 'DefinitionId' rl 0 us 9.9999e+009 ds 9.9999e+009 crsn\r\n");
        }

        private void GenerateCustomCrossSectionDefinitionLocationFile(TemporaryDirectory tempDirectory)
        {
            var customCrsDefLocFile = Path.Combine(tempDirectory.Path, sobekFileNames.SobekNetworkLocationsFileName);
            File.WriteAllText(customCrsDefLocFile, "CRSN id 'LocationId' nm 'LocationName' ci '1' lc 671 crsn\r\n");
        }
    }
}
