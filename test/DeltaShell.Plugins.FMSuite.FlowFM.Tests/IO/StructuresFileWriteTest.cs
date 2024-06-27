using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
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
            var structs = new List<IStructureObject>();

            var generalStructureWeir = new Structure()
            {
                Name = "weir01",
                Formula = new GeneralStructureFormula
                {
                    Upstream1Width = 1.0,
                    Upstream2Width = 2.0,
                    CrestWidth = 3.0,
                    Downstream1Width = 4.0,
                    Downstream2Width = 5.0,
                    Upstream1Level = 6.0,
                    Upstream2Level = 7.0,
                    CrestLevel = 8.0,
                    Downstream1Level = 9.0,
                    Downstream2Level = 10.0,
                    GateHeight = 11.0,
                    HorizontalGateOpeningWidth = 30.0,
                    GateLowerEdgeLevel = 31.0,
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
                    ExtraResistance = 22.0
                }
            };
            structs.Add(generalStructureWeir);

            var simpleWeir = new Structure()
            {
                Name = "weir02",
                CrestWidth = 5.0,
                Formula = new SimpleWeirFormula()
            };
            structs.Add(simpleWeir);

            StructureSchema<ModelPropertyDefinition> schema = new StructureSchemaCsvFile().ReadStructureSchema(StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);

            var structuresFile = new StructuresFile() {StructureSchema = schema};

            string exportFilePath = TestHelper.GetCurrentMethodName() + ".ini";
            FileUtils.DeleteIfExists(exportFilePath);

            try
            {
                // write
                structuresFile.Write(exportFilePath, structs);

                // read
                string fileContents = File.ReadAllText(exportFilePath);

                // compare
                Assert.AreEqual(
                    "[structure]" + Environment.NewLine +
                    "    type                  = generalstructure    \t# Type of structure" + Environment.NewLine +
                    "    id                    = weir01              \t# Name of the structure" + Environment.NewLine +
                    "    Upstream1Width        = 1                   \t# Upstream width 1 (m)" + Environment.NewLine +
                    "    Upstream2Width        = 2                   \t# Upstream width 2 (m)" + Environment.NewLine +
                    "    CrestWidth            = 3                   \t# Crest width (m)" + Environment.NewLine +
                    "    Downstream1Width      = 4                   \t# Downstream width 1 (m)" + Environment.NewLine +
                    "    Downstream2Width      = 5                   \t# Downstream width 2 (m)" + Environment.NewLine +
                    "    Upstream1Level        = 6                   \t# Upstream level 1 (m AD)" + Environment.NewLine +
                    "    Upstream2Level        = 7                   \t# Upstream level 2 (m AD)" + Environment.NewLine +
                    "    CrestLevel            = 8                   \t# Crest level (m AD)" + Environment.NewLine +
                    "    Downstream1Level      = 9                   \t# Downstream level 1 (m AD)" + Environment.NewLine +
                    "    Downstream2Level      = 10                  \t# Downstream level 2 (m AD)" + Environment.NewLine +
                    "    GateLowerEdgeLevel    = 31                  \t# Gate lower edge level (m AD)" + Environment.NewLine +
                    "    pos_freegateflowcoeff = 12                  \t# Positive free gate flow (-)" + Environment.NewLine +
                    "    pos_drowngateflowcoeff= 13                  \t# Positive drowned gate flow (-)" + Environment.NewLine +
                    "    pos_freeweirflowcoeff = 14                  \t# Positive free weir flow (-)" + Environment.NewLine +
                    "    pos_drownweirflowcoeff= 15                  \t# Positive drowned weir flow (-)" + Environment.NewLine +
                    "    pos_contrcoeffreegate = 16                  \t# Positive flow contraction coefficient (-)" + Environment.NewLine +
                    "    neg_freegateflowcoeff = 17                  \t# Negative free gate flow (-)" + Environment.NewLine +
                    "    neg_drowngateflowcoeff= 18                  \t# Negative drowned gate flow (-)" + Environment.NewLine +
                    "    neg_freeweirflowcoeff = 19                  \t# Negative free weir flow (-)" + Environment.NewLine +
                    "    neg_drownweirflowcoeff= 20                  \t# Negative drowned weir flow (-)" + Environment.NewLine +
                    "    neg_contrcoeffreegate = 21                  \t# Negative flow contraction coefficient (-)" + Environment.NewLine +
                    "    extraresistance       = 22                  \t# Extra resistance (-)" + Environment.NewLine +
                    "    GateHeight            = 11                  \t# Vertical gate door height (m)" + Environment.NewLine +
                    "    GateOpeningWidth      = 30                  \t# Horizontal opening width between the gates (m)" + Environment.NewLine +
                    "    GateOpeningHorizontalDirection= symmetric           \t# Horizontal direction of the opening gates" + Environment.NewLine +
                    "[structure]" + Environment.NewLine +
                    "    type                  = weir                \t# Type of structure" + Environment.NewLine +
                    "    id                    = weir02              \t# Name of the structure" + Environment.NewLine +
                    "    CrestLevel            = 0                   \t# Weir crest height (in [m])" + Environment.NewLine +
                    "    CrestWidth            = 5                   \t# Weir crest width (in [m])" + Environment.NewLine +
                    "    lat_contr_coeff       = 1                   \t# Lateral contraction coefficient" + Environment.NewLine, fileContents);
            }
            finally
            {
                FileUtils.DeleteIfExists(exportFilePath);
            }
        }

        /// <summary>
        /// GIVEN a structures file
        /// AND a simple weir with an empty crest width
        /// WHEN these structures are exported
        /// THEN no exceptions are thrown
        /// AND the corresponding width fields are empty
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenASimpleWeirWithAnEmptyCrestWidth_WhenTheseStructuresAreExported_ThenNoExceptionsAreThrownAndTheCorrespondingWidthFieldsAreEmpty()
        {
            // Given
            // - structures file
            StructuresFile structuresFile = GetStructuresFile();

            // - simple weir with an empty crest width
            var simpleWeir = new Structure()
            {
                Name = "Its weir-d",
                Formula = new SimpleWeirFormula(),
                CrestWidth = double.NaN
            };

            var structures = new List<IStructureObject>() {simpleWeir};

            // When | Then
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                string exportFilePath = Path.Combine(tempDir, "FlowFM_structures.ini");

                Assert.DoesNotThrow(() => { structuresFile.Write(exportFilePath, structures); });

                // Read file with ini reader again.
                IniSection section = AssertThatStructureSectionExistsInFileAndReturn(exportFilePath);
                AssertThatPropertyExistsAndIsEmpty(section, KnownStructureProperties.CrestWidth);
            });
        }

        /// <summary>
        /// GIVEN a structures file
        /// AND a gated weir with an empty crest width
        /// WHEN these structures are exported
        /// THEN no exceptions are thrown
        /// AND the corresponding width fields are empty
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAGatedWeirWithAnEmptyCrestWidth_WhenTheseStructuresAreExported_ThenNoExceptionsAreThrownAndTheCorrespondingWidthFieldsAreEmpty()
        {
            // Given
            // - structures file
            StructuresFile structuresFile = GetStructuresFile();

            // - simple weir with an empty crest width
            var gatedWeir = new Structure()
            {
                Name = "Its weir-d",
                Formula = new SimpleGateFormula(true),
                CrestWidth = double.NaN
            };

            var structures = new List<IStructureObject>() {gatedWeir};

            // When | Then
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                string exportFilePath = Path.Combine(tempDir, "FlowFM_structures.ini");

                Assert.DoesNotThrow(() => { structuresFile.Write(exportFilePath, structures); });

                // Read file with ini reader again.
                IniSection section = AssertThatStructureSectionExistsInFileAndReturn(exportFilePath);
                AssertThatPropertyExistsAndIsEmpty(section, KnownStructureProperties.CrestWidth);
            });
        }

        /// <summary>
        /// GIVEN a structures file
        /// AND a general structure with empty width fields
        /// WHEN these structures are exported
        /// THEN no exceptions are thrown
        /// AND the corresponding width fields are empty
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAGeneralStructureWithEmptyWidthFields_WhenTheseStructuresAreExported_ThenNoExceptionsAreThrownAndTheCorrespondingWidthFieldsAreEmpty()
        {
            // Given
            // - structures file
            StructuresFile structuresFile = GetStructuresFile();

            // - simple weir with an empty crest width
            var generalStructureFormula = new GeneralStructureFormula()
            {
                Upstream1Width = double.NaN,
                Downstream2Width = double.NaN,
                CrestWidth = double.NaN,
                Upstream2Width = double.NaN,
                Downstream1Width = double.NaN
            };

            var generalWeir = new Structure()
            {
                Name = "Weir-d salute",
                Formula = generalStructureFormula,
                CrestWidth = double.NaN
            };

            var structures = new List<IStructureObject>() {generalWeir};

            // When | Then
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                string exportFilePath = Path.Combine(tempDir, "FlowFM_structures.ini");

                Assert.DoesNotThrow(() => { structuresFile.Write(exportFilePath, structures); });

                // Read file with ini reader again.
                IniSection section = AssertThatStructureSectionExistsInFileAndReturn(exportFilePath);

                AssertThatPropertyExistsAndIsEmpty(section, GetName(KnownGeneralStructureProperties.Upstream2Width));
                AssertThatPropertyExistsAndIsEmpty(section, GetName(KnownGeneralStructureProperties.Upstream1Width));
                AssertThatPropertyExistsAndIsEmpty(section, GetName(KnownGeneralStructureProperties.CrestWidth));
                AssertThatPropertyExistsAndIsEmpty(section, GetName(KnownGeneralStructureProperties.Downstream1Width));
                AssertThatPropertyExistsAndIsEmpty(section, GetName(KnownGeneralStructureProperties.Downstream2Width));
            });
        }

        #region TestHelpers

        private static string GetName(KnownGeneralStructureProperties prop)
        {
            return prop.GetDescription();
        }

        /// <summary>
        /// Gets the structures file.
        /// </summary>
        /// <returns> A new StructuresFile with the a default schema.</returns>
        private static StructuresFile GetStructuresFile()
        {
            StructureSchema<ModelPropertyDefinition> schema = new StructureSchemaCsvFile().ReadStructureSchema(
                StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);

            var structuresFile = new StructuresFile() {StructureSchema = schema};
            return structuresFile;
        }

        private static IniSection AssertThatStructureSectionExistsInFileAndReturn(string filePath)
        {
            IniData iniData;
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                iniData = new IniReader().ReadIniFile(fileStream, filePath);
            }

            IniSection section = iniData.Sections.First();
            
            Assert.That(section.Name, Is.EqualTo("structure")
                        , "The name of the section does not match the expectation.");

            return section;
        }

        /// <summary>
        /// Assert the that property with the key <paramref name="propertyKey"/> exists and has an empty value.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="propertyKey">The key of the property.</param>
        private static void AssertThatPropertyExistsAndIsEmpty(IniSection section, string propertyKey)
        {
            Assert.That(section.Properties, Is.Not.Null
                        , "The Properties of the structure section should not be null.");
            IEnumerable<IniProperty> obtainedProperties = section.GetAllProperties(propertyKey);
            Assert.That(obtainedProperties.Count()
                        , Is.EqualTo(1)
                        , "Expected a single crest_width element in structure properties:");

            IniProperty prop = obtainedProperties.First();
            Assert.That(prop.Value, Is.EqualTo(string.Empty), "crest_width value does not meet expectation:");
        }

        #endregion
    }
}