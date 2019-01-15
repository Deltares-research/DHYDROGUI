using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class StructuresFileWriteTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void StructuresFileWriteGeneralStructureGivesExpectedResultTest()
        {
            var structs = new List<IStructure1D>();

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

                    DoorHeight = 11.0,

                    HorizontalDoorOpeningWidth = 30.0,
                    LowerEdgeLevel = 31.0,

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

            var simpleWeir = new Weir2D("weir02",true)
            {
                CrestWidth = 5.0,
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
                "    widthleftW1           = 1                   # Width left side of structure (m)" + Environment.NewLine +
                "    widthleftWsdl         = 2                   # Width structure left side (m)" + Environment.NewLine +
                "    widthcenter           = 3                   # Width structure centre (m)" + Environment.NewLine +
                "    widthrightWsdr        = 4                   # Width structure right side (m)" + Environment.NewLine +
                "    widthrightW2          = 5                   # Width right side of structure (m)" + Environment.NewLine +
                "    levelleftZb1          = 6                   # Bed level left side of structure (m AD)" + Environment.NewLine +
                "    levelleftZbsl         = 7                   # Bed level left side structure (m AD)" + Environment.NewLine +
                "    levelcenter           = 8                   # Bed level at centre of structure (m AD)" + Environment.NewLine +
                "    levelrightZbsr        = 9                   # Bed level right side structure (m AD)" + Environment.NewLine +
                "    levelrightZb2         = 10                  # Bed level right side of structure (m AD)" + Environment.NewLine +
                "    gateheight            = 31                  # Gate lower edge level (m AD)" + Environment.NewLine +
                "    pos_freegateflowcoeff = 12                  # Positive free gate flow (-)" + Environment.NewLine +
                "    pos_drowngateflowcoeff= 13                  # Positive drowned gate flow (-)" + Environment.NewLine +
                "    pos_freeweirflowcoeff = 14                  # Positive free weir flow (-)" + Environment.NewLine +
                "    pos_drownweirflowcoeff= 15                  # Positive drowned weir flow (-)" + Environment.NewLine +
                "    pos_contrcoeffreegate = 16                  # Positive flow contraction coefficient (-)" + Environment.NewLine +
                "    neg_freegateflowcoeff = 17                  # Negative free gate flow (-)" + Environment.NewLine +
                "    neg_drowngateflowcoeff= 18                  # Negative drowned gate flow (-)" + Environment.NewLine +
                "    neg_freeweirflowcoeff = 19                  # Negative free weir flow (-)" + Environment.NewLine +
                "    neg_drownweirflowcoeff= 20                  # Negative drowned weir flow (-)" + Environment.NewLine +
                "    neg_contrcoeffreegate = 21                  # Negative flow contraction coefficient (-)" + Environment.NewLine +
                "    extraresistance       = 22                  # Extra resistance (-)" + Environment.NewLine +
                "    gatedoorheight        = 11                  # Vertical gate door height (m)" + Environment.NewLine +
                "    door_opening_width    = 30                  # Horizontal opening width between the doors (m)" + Environment.NewLine +
                "    horizontal_opening_direction= symmetric           # Horizontal direction of the opening doors" + Environment.NewLine +
                "[structure]" + Environment.NewLine +
                "    type                  = weir                # Type of structure" + Environment.NewLine +
                "    id                    = weir02              # Name of the structure" + Environment.NewLine +
                "    crest_level           = 0                   # Weir crest height (in [m])" + Environment.NewLine +
                "    crest_width           = 5                   # Weir crest width (in [m])" + Environment.NewLine +
                "    lat_contr_coeff       = 1                   # Lateral contraction coefficient" + Environment.NewLine, fileContents);
            }
            finally
            {
                FileUtils.DeleteIfExists(exportFilePath);
            }
        }

        /// <summary>
        /// GIVEN a structures file
        ///   AND a simple weir with an empty crest width
        /// WHEN these structures are exported
        /// THEN no exceptions are thrown
        ///  AND the corresponding width fields are empty
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenASimpleWeirWithAnEmptyCrestWidth_WhenTheseStructuresAreExported_ThenNoExceptionsAreThrownAndTheCorrespondingWidthFieldsAreEmpty()
        {
            // Given
            // - structures file
            var structuresFile = GetStructuresFile();

            // - simple weir with an empty crest width
            var simpleWeir = new Weir2D("Its weir-d")
            {
                WeirFormula = new SimpleWeirFormula(),
                CrestWidth = double.NaN
            };

            var structures = new List<IStructure>() {simpleWeir};

            // When | Then
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var exportFilePath = Path.Combine(tempDir, "FlowFM_structures.ini");

                Assert.DoesNotThrow(() =>
                {
                    structuresFile.Write(exportFilePath, structures);
                });

                // Read file with ini reader again.
                AssertThatStructureCategoryExists(exportFilePath, out var category);
                AssertThatPropertyExistsAndIsEmpty(category, KnownStructureProperties.CrestWidth);
            });
        }

        /// <summary>
        /// GIVEN a structures file
        ///   AND a gated weir with an empty crest width
        /// WHEN these structures are exported
        /// THEN no exceptions are thrown
        ///  AND the corresponding width fields are empty
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAGatedWeirWithAnEmptyCrestWidth_WhenTheseStructuresAreExported_ThenNoExceptionsAreThrownAndTheCorrespondingWidthFieldsAreEmpty()
        {
            // Given
            // - structures file
            var structuresFile = GetStructuresFile();

            // - simple weir with an empty crest width
            var gatedWeir = new Weir2D("Its weir-d")
            {
                WeirFormula = new GatedWeirFormula(true),
                CrestWidth = double.NaN
            };

            var structures = new List<IStructure>() { gatedWeir };

            // When | Then
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var exportFilePath = Path.Combine(tempDir, "FlowFM_structures.ini");

                Assert.DoesNotThrow(() =>
                {
                    structuresFile.Write(exportFilePath, structures);
                });

                // Read file with ini reader again.
                AssertThatStructureCategoryExists(exportFilePath, out var category);
                AssertThatPropertyExistsAndIsEmpty(category, KnownStructureProperties.GateSillWidth);
            });
        }

        /// <summary>
        /// GIVEN a structures file
        ///   AND a general structure with empty width fields
        /// WHEN these structures are exported
        /// THEN no exceptions are thrown
        ///  AND the corresponding width fields are empty
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAGeneralStructureWithEmptyWidthFields_WhenTheseStructuresAreExported_ThenNoExceptionsAreThrownAndTheCorrespondingWidthFieldsAreEmpty()
        {
            // Given
            // - structures file
            var structuresFile = GetStructuresFile();

            // - simple weir with an empty crest width
            var generalStructureFormula = new GeneralStructureWeirFormula()
            {
                WidthLeftSideOfStructure = double.NaN,
                WidthRightSideOfStructure = double.NaN,
                WidthStructureCentre = double.NaN,
                WidthStructureLeftSide = double.NaN,
                WidthStructureRightSide = double.NaN,
            };

            var generalWeir = new Weir2D("Weir-d salute")
            {
                WeirFormula = generalStructureFormula,
                CrestWidth = double.NaN
            };

            var structures = new List<IStructure>() { generalWeir };

            // When | Then
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var exportFilePath = Path.Combine(tempDir, "FlowFM_structures.ini");

                Assert.DoesNotThrow(() =>
                {
                    structuresFile.Write(exportFilePath, structures);
                });

                // Read file with ini reader again.
                AssertThatStructureCategoryExists(exportFilePath, out var category);

                AssertThatPropertyExistsAndIsEmpty(category, GetName(KnownGeneralStructureProperties.WidthLeftW1));
                AssertThatPropertyExistsAndIsEmpty(category, GetName(KnownGeneralStructureProperties.WidthLeftWsdl));
                AssertThatPropertyExistsAndIsEmpty(category, GetName(KnownGeneralStructureProperties.WidthCenter));
                AssertThatPropertyExistsAndIsEmpty(category, GetName(KnownGeneralStructureProperties.WidthRightWsdr));
                AssertThatPropertyExistsAndIsEmpty(category, GetName(KnownGeneralStructureProperties.WidthRightW2));
            });
        }

        #region TestHelpers

        private static string GetName(KnownGeneralStructureProperties prop)
        {
            return EnumDescriptionAttributeTypeConverter.GetEnumDescription(prop);
        }

        /// <summary>
        /// Gets the structures file.
        /// </summary>
        /// <returns> A new StructuresFile with the a default schema.</returns>
        private static StructuresFile GetStructuresFile()
        {
            var schema = new StructureSchemaCsvFile().ReadStructureSchema(
                StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);

            var structuresFile = new StructuresFile()
            {
                StructureSchema = schema,
            };
            return structuresFile;
        }

        /// <summary>
        /// Assert the that structure category exists and puts in the out <paramref name="category"/>.
        /// </summary>
        /// <param name="exportFilePath">The export file path.</param>
        /// <param name="category">The category.</param>
        private static void AssertThatStructureCategoryExists(string exportFilePath, out IDelftIniCategory category)
        {
            var categories = DelftIniFileParser.ReadFile(exportFilePath);

            Assert.That(categories.Count, Is.EqualTo(1)
                        , "The number of categories does not match the expectation:");
            category = categories[0];
            Assert.That(category, Is.Not.Null
                        , "The structure category is not expected to be null.");
            Assert.That(category.Name, Is.EqualTo("structure")
                        , "The name of the category does not match the expectation:");
        }

        /// <summary>
        /// Assert the that property with the name <paramref name="propertyName"/> exists and has an empty value.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="propertyName">Name of the property.</param>
        private static void AssertThatPropertyExistsAndIsEmpty(IDelftIniCategory category, string propertyName)
        {
            Assert.That(category.Properties, Is.Not.Null
                        , "The Properties of the structure category should not be null.");
            var obtainedProperties = category.Properties.Where(p => p.Name == propertyName);
            Assert.That(obtainedProperties.Count()
                        , Is.EqualTo(1)
                        , "Expected a single crest_width element in structure properties:");

            var prop = obtainedProperties.First();
            Assert.That(prop.Value, Is.EqualTo(string.Empty), "crest_width value does not meet expectation:");
        }

        #endregion
    }
}
