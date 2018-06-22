using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class StructureFileWriteTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void StructuresFileWriteGeneralStructureGivesExpectedResultTest()
        {
            List<IStructure1D> structs = new List<IStructure1D>();

            var generalStructureWeir = new Weir("weir01")
            {
                WeirFormula = new GeneralStructureWeirFormula
                {
                    WidthLeftSideOfStructure = 1.0,
                    WidthStructureLeftSide = 2.0,
                    WidthStructureCentre = 3.0,
                    WidthStructureRightSide = 4.0,
                    WidthRightSideOfStructure = 5.0,

                    BedLevelLeftSideOfStructure = 6.0,
                    BedLevelLeftSideStructure = 7.0,
                    BedLevelStructureCentre = 8.0,
                    BedLevelRightSideStructure = 9.0,
                    BedLevelRightSideOfStructure = 10.0,

                    GateOpening = 11.0,

                    PositiveFreeGateFlow = 12.0,
                    PositiveDrownedGateFlow = 13.0,
                    PositiveFreeWeirFlow = 14.0,
                    PositiveDrownedWeirFlow = 15.0,
                    PositiveContractionCoefficient = 16.0,

                    NegativeFreeGateFlow = 17.0,
                    NegativeDrownedGateFlow = 18.0,
                    NegativeFreeWeirFlow = 19.0,
                    NegativeDrownedWeirFlow = 20.0,
                    NegativeContractionCoefficient = 21.0,
                    ExtraResistance = 22.0,
                }
            };
            structs.Add(generalStructureWeir);

            var simpleWeir = new Weir("weir02")
            {
                WeirFormula = new SimpleWeirFormula()
            };
            structs.Add(simpleWeir);

            var schema = new StructureSchemaCsvFile().ReadStructureSchema(StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);

            var structuresFile = new StructuresFile()
            {
                StructureSchema = schema,
            };

            var exportFilePath = TestHelper.GetCurrentMethodName() + ".ini";
            FileUtils.DeleteIfExists(exportFilePath);

            try
            {
                // write
                structuresFile.Write(exportFilePath, structs);

                // read
                var fileContents = File.ReadAllText(exportFilePath);

                // compare
                Assert.AreEqual(
                "[structure]" + Environment.NewLine +
                "    type                  = generalstructure    # Type of structure" + Environment.NewLine +
                "    id                    = weir01              # Name of the structure" + Environment.NewLine +
                "    widthleftW1           = 1.000               # Width left side of structure (m)" + Environment.NewLine +
                "    widthleftWsdl         = 2.000               # Width structure left side (m)" + Environment.NewLine +
                "    widthcenter           = 3.000               # Width structure centre (m)" + Environment.NewLine +
                "    widthrightWsdr        = 4.000               # Width structure right side (m)" + Environment.NewLine +
                "    widthrightW2          = 5.000               # Width right side of structure (m)" + Environment.NewLine +
                "    levelleftZb1          = 6.000               # Bed level left side of structure (m AD)" + Environment.NewLine +
                "    levelleftZbsl         = 7.000               # Bed level left side structure (m AD)" + Environment.NewLine +
                "    levelcenter           = 8.000               # Bed level at centre of structure (m AD)" + Environment.NewLine +
                "    levelrightZbsr        = 9.000               # Bed level right side structure (m AD)" + Environment.NewLine +
                "    levelrightZb2         = 10.000              # Bed level right side of structure (m AD)" + Environment.NewLine +
                "    gateheight            = 19.000              # Gate lower edge level (m AD)" + Environment.NewLine +
                "    pos_freegateflowcoeff = 12.000              # Positive free gate flow (-)" + Environment.NewLine +
                "    pos_drowngateflowcoeff= 13.000              # Positive drowned gate flow (-)" + Environment.NewLine +
                "    pos_freeweirflowcoeff = 14.000              # Positive free weir flow (-)" + Environment.NewLine +
                "    pos_drownweirflowcoeff= 15.000              # Positive drowned weir flow (-)" + Environment.NewLine +
                "    pos_contrcoeffreegate = 16.000              # Positive flow contraction coefficient (-)" + Environment.NewLine +
                "    neg_freegateflowcoeff = 17.000              # Negative free gate flow (-)" + Environment.NewLine +
                "    neg_drowngateflowcoeff= 18.000              # Negative drowned gate flow (-)" + Environment.NewLine +
                "    neg_freeweirflowcoeff = 19.000              # Negative free weir flow (-)" + Environment.NewLine +
                "    neg_drownweirflowcoeff= 20.000              # Negative drowned weir flow (-)" + Environment.NewLine +
                "    neg_contrcoeffreegate = 21.000              # Negative flow contraction coefficient (-)" + Environment.NewLine +
                "    extraresistance       = 22.000              # Extra resistance (-)" + Environment.NewLine +
                "    gatedoorheight        = 11.000              # Gate opening height (m)" + Environment.NewLine +
                "[structure]" + Environment.NewLine +
                "    type                  = weir                # Type of structure" + Environment.NewLine +
                "    id                    = weir02              # Name of the structure" + Environment.NewLine +
                "    crest_level           = 1                   # Weir crest height (in [m])" + Environment.NewLine +
                "    crest_width           = 5                   # Weir crest width (in [m])" + Environment.NewLine +
                "    lat_contr_coeff       = 1                   # Lateral contraction coefficient" + Environment.NewLine, fileContents);
            }
            finally
            {
                FileUtils.DeleteIfExists(exportFilePath);
            }
        }
    }
}
