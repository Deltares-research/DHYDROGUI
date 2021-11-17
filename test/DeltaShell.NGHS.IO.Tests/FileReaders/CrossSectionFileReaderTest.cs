using System;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class CrossSectionFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFile_TwoLocationShareDefinition_DefinitionIsAddedToSharedCrossSectionDefinitions()
        {
            // Setup
            string fileContentLocations = string.Join(
                Environment.NewLine,
                "[General]",
                "    fileVersion           = 1.01",
                "    fileType              = crossLoc",
                "",
                "[CrossSection]",
                "    id                    = some_id_",
                "    branchId              = some_branch_id",
                "    chainage              = 1.23",
                "    shift                 = 2.34",
                "    definitionId          = some_definition_id",
                "",
                "[CrossSection]",
                "    id                    = some_id_2",
                "    branchId              = some_branch_id",
                "    chainage              = 5.67",
                "    shift                 = 6.78",
                "    definitionId          = some_definition_id"
            );

            string fileContentDefinitions = string.Join(
                Environment.NewLine,
                "[General]",
                "    fileVersion           = 3.00",
                "    fileType              = crossDef",
                "",
                "[Definition]",
                "    id                    = some_definition_id",
                "    type                  = yz",
                "    thalweg               = 50.0",
                "    singleValuedZ         = 1",
                "    yzCount               = 4",
                "    yCoordinates          = 0 33 66 100",
                "    zCoordinates          = 0 -10 -10 -0",
                "    sectionCount          = 1",
                "    frictionIds           = Channels"
            );

            using (var temp = new TemporaryDirectory())
            {
                string locationFile = temp.CreateFile("crsloc.ini", fileContentLocations);
                string definitionFile = temp.CreateFile("crsdef.ini", fileContentDefinitions);
                var network = new HydroNetwork();
                network.Branches.Add(new Branch { Name = "some_branch_id" });

                // Call
                CrossSectionFileReader.ReadFile(locationFile, definitionFile, network, null);

                // Assert
                Assert.That(network.SharedCrossSectionDefinitions, Has.Count.EqualTo(1));
                Assert.That(network.SharedCrossSectionDefinitions[0].Name, Is.EqualTo("some_definition_id"));
            }
        }
    }
}