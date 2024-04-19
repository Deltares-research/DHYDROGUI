using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders.CrossSectionDefinition;
using log4net.Core;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class CrossSectionFileReaderTest
    {
        private readonly string fileContentLocations = string.Join(
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

        private readonly string fileContentDefinitions = string.Join(
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
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFile_TwoLocationShareDefinition_DefinitionIsAddedToSharedCrossSectionDefinitions()
        {
            // Setup
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

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFileCrossSectionDefinitions_AddsInfoMessageToLogIndicatingWhichFileIsBeingWritten()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string locationFile = temp.CreateFile("crsloc.ini", fileContentLocations);
                string definitionFile = temp.CreateFile("crsdef.ini", fileContentDefinitions);
                var network = new HydroNetwork();
                network.Branches.Add(new Branch { Name = "some_branch_id" });

                // Call
                void Call() => CrossSectionFileReader.ReadFile(locationFile, definitionFile, network, null);
                IEnumerable<string> infoMessages = TestHelper.GetAllRenderedMessages(Call, Level.Info);

                // Assert
                var expectedMessage = $"Reading cross section definitions from {definitionFile}.";
                Assert.That(infoMessages.Any(m => m.Equals(expectedMessage)));
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFileCrossSectionDefinitions_WithoutBranchToPlaceOn_ThrowsException()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string locationFile = temp.CreateFile("crsloc.ini", fileContentLocations);
                string definitionFile = temp.CreateFile("crsdef.ini", fileContentDefinitions);
                var network = new HydroNetwork();
                
                // Call
                void Call() => CrossSectionFileReader.ReadFile(locationFile, definitionFile, network, null);
                

                // Assert
                var ex = Assert.Throws<IO.FileReaders.FileReadingException>(Call);
                Assert.That(ex.Message, Contains.Substring("some_id_").IgnoreCase);
                Assert.That(ex.Message, Contains.Substring("some_branch_id").IgnoreCase);
            }
        }
    }
}