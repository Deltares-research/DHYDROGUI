using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class StructuresFileReaderTest
    {
        private IHydroNetwork originalNetwork;
        private IList<IChannel> channels;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch("node1", "node2", "branch");
            channels = originalNetwork.Channels.ToList();
        }

        [Test, Category(TestCategory.DataAccess)]
        public void GivenAStructuresFile_WhenReadingIt_ThenAllCompositeBranchStructuresShouldBeCreated()
        {
            var testFile = TestHelper.GetTestFilePath(@"Structures.ini");

            try
            {
                var categories = new List<DelftIniCategory>();
                var category = new DelftIniCategory(StructureRegion.Header);

                category.AddProperty(StructureRegion.Id.Key, "Weir1");
                category.AddProperty(StructureRegion.Chainage.Key, "50");
                category.AddProperty(StructureRegion.BranchId.Key, "branch");
                category.AddProperty(StructureRegion.Name.Key, "Weir1");
                category.AddProperty(StructureRegion.Compound.Key, "1");
                category.AddProperty(StructureRegion.CompoundName.Key, "Bla");
                category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.Weir);

                category.AddProperty(StructureRegion.CrestLevel.Key, "1.3");
                category.AddProperty(StructureRegion.CrestWidth.Key, "100");
                category.AddProperty(StructureRegion.DischargeCoeff.Key, "1.1");
                category.AddProperty(StructureRegion.LatDisCoeff.Key, "1.2");
                category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0" );
                
                categories.Add(category);

                new IniFileWriter().WriteIniFile(categories, testFile);

                var errorReport = new List<string>();

                Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                    errorReport.Add(
                        $"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

                var reader = new StructuresFileReader(CreateAndAddErrorReport);
                var allCompositeBranchStructures = reader.ReadStructures(testFile, channels, new List<ICrossSectionDefinition>(), new GroundLayerDataTransferObject[] { });

                Assert.AreEqual(1, allCompositeBranchStructures.Count);
                Assert.AreEqual(1, allCompositeBranchStructures[0].Structures.Count);

                Assert.AreEqual("Bla", allCompositeBranchStructures[0].Name);
                Assert.AreEqual("branch", allCompositeBranchStructures[0].Branch.Name);
                Assert.AreEqual(50, allCompositeBranchStructures[0].Chainage);
                Assert.AreEqual("Weir1", allCompositeBranchStructures[0].Structures[0].Name);
                Assert.AreEqual(50, allCompositeBranchStructures[0].Structures[0].Chainage);
                Assert.AreSame(allCompositeBranchStructures[0].Branch, allCompositeBranchStructures[0].Structures[0].Branch);
                Assert.AreEqual("Weir1", allCompositeBranchStructures[0].Structures[0].LongName);

                Assert.AreEqual(0, errorReport.Count);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFile);
            }
        }

        [Test, Category(TestCategory.DataAccess)]
        public void GivenAnEmptyStructuresFile_WhenReadingIt_ThenAnErrorReportShouldBeCreated()
        {
            var testFile = TestHelper.GetTestFilePath(@"ObservationPoints.ini");

            try
            {
                var categories = new List<DelftIniCategory>();

                new IniFileWriter().WriteIniFile(categories, testFile);

                var errorReport = new List<string>();

                Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                    errorReport.Add(
                        $"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

                var reader = new StructuresFileReader(CreateAndAddErrorReport);
                var allCompositeBranchStructures = reader.ReadStructures(testFile, channels, new List<ICrossSectionDefinition>(), new GroundLayerDataTransferObject[] { });

                Assert.AreEqual(0, allCompositeBranchStructures.Count);
                Assert.AreEqual(1, errorReport.Count);

                var expectedMessage =
                    string.Format(
                        "While reading the structures from file, an error occured:{0} Could not read file {1} properly, it seems empty", Environment.NewLine, testFile);

                Assert.AreEqual(expectedMessage, errorReport[0]);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFile);
            }
        }
    }
}