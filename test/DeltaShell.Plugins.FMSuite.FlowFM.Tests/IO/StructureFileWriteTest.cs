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

                    GateHeight = 11.0,
                    
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
                    LowerEdgeLevel = 19.0
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
                "    type                  = generalstructure    # \"Structure type must read generalStructure.\"" + Environment.NewLine +
                "    id                    = weir01              # Unique structure id." + Environment.NewLine +
                "    upstream1Width        = 1.000               " + Environment.NewLine +
                "    upstream2Width        = 2.000               " + Environment.NewLine +
                "    crestWidth            = 3.000               # Width of weir (m)" + Environment.NewLine +
                "    downstream1Width      = 4.000               " + Environment.NewLine +
                "    downstream2Width      = 5.000               " + Environment.NewLine +
                "    upstream1Level        = 6.000               " + Environment.NewLine +
                "    upstream2Level        = 7.000               " + Environment.NewLine +
                "    crestLevel            = 8.000               # Crest level of weir (m AD)" + Environment.NewLine +
                "    downstream1Level      = 9.000               " + Environment.NewLine +
                "    downstream2Level      = 10.000              " + Environment.NewLine +
                "    gateLowerEdgeLevel    = 19.000              # Gate lower edge level (m AD)" + Environment.NewLine +
                "    posFreeGateFlowCoeff  = 12.000              # Positive free gate flow (-)" + Environment.NewLine +
                "    posDrownGateFlowCoeff = 13.000              # Positive drowned gate flow (-)" + Environment.NewLine +
                "    posFreeWeirFlowCoeff  = 14.000              # Positive free weir flow (-)" + Environment.NewLine +
                "    posDrownWeirFlowCoeff = 15.000              # Positive drowned weir flow (-)" + Environment.NewLine +
                "    posContrCoefFreeGate  = 16.000              # Positive flow contraction coefficient (-)" + Environment.NewLine +
                "    negFreeGateFlowCoeff  = 17.000              # Negative free gate flow (-)" + Environment.NewLine +
                "    negDrownGateFlowCoeff = 18.000              # Negative drowned gate flow (-)" + Environment.NewLine +
                "    negFreeWeirFlowCoeff  = 19.000              # Negative free weir flow (-)" + Environment.NewLine +
                "    negDrownWeirFlowCoeff = 20.000              # Negative drowned weir flow (-)" + Environment.NewLine +
                "    negContrCoefFreeGate  = 21.000              # Negative flow contraction coefficient (-)" + Environment.NewLine +
                "    crestLength           = 0.000               " + Environment.NewLine +
                "    useVelocityHeight     = true                " + Environment.NewLine +
                "    extraResistance       = 22.000              # Extra resistance (-)" + Environment.NewLine +
                "    gateHeight            = 11.000              # Gate height (m)" + Environment.NewLine +
                "    gateOpeningWidth      = 0.000               " + Environment.NewLine +
                "    gateOpeningHorizontalDirection= symmetric           " + Environment.NewLine +
                Environment.NewLine +
                "[structure]" + Environment.NewLine +
                "    type                  = weir                # \"Structure type must read weir.\"" + Environment.NewLine +
                "    id                    = weir02              # Unique structure id." + Environment.NewLine +
                "    crestLevel            = 1                   # Crest level of weir (m AD)." + Environment.NewLine +
                "    crestWidth            = 5                   # (optional) Width of weir (m)." + Environment.NewLine +
                "    corrCoeff             = 1                   # Correction coefficient (-)." + Environment.NewLine, fileContents);
            }
            finally
            {
                FileUtils.DeleteIfExists(exportFilePath);
            }
        }
    }
}
