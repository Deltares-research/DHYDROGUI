using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class StructuresFileReadTest
    {
        /// <summary>
        /// GIVEN a structures file
        /// AND a structures ini file describing a simple weir with an empty crest width
        /// WHEN this structure is read
        /// THEN no exceptions are thrown
        /// AND the weir crest width is NaN
        /// </summary>
        [TestCase(true)]
        [Category(TestCategory.DataAccess)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void GivenAStructuresIniFileDescribingASimpleWeirWithAnEmptyCrestWidth_WhenThisStructureIsRead_ThenNoExceptionsAreThrownAndTheWeirCrestWidthIsNaN(bool hasExplicitField)
        {
            // Given
            // - structures file
            StructuresFile structuresFile = GetStructuresFile();

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                const string structureName = "weir-d";
                string fileIniPath = Path.Combine(tempDir, "structures.ini");

                WritePliFileSingleStructure(tempDir, structureName);

                DelftIniCategory structureCategory = GetBaseStructureCategory(structureName, "weir");
                SetSimpleWeirRequiredProperties(structureCategory);

                if (hasExplicitField)
                {
                    structureCategory.AddProperty(KnownStructureProperties.CrestWidth, " ", "#");
                }

                WriteStructuresIniFile(structureCategory, tempDir);

                // When | Then
                // - Read structures file
                IList<IStructureObject> structures = null;
                Assert.DoesNotThrow(() => { structures = structuresFile.Read(fileIniPath); });

                AssertThatOnlyOneStructureExistsWithin(structures);

                IStructureObject weirStructure = structures[0];
                AssertThatStructureIsCorrect(weirStructure, typeof(SimpleWeirFormula));
            });
        }

        /// <summary>
        /// GIVEN a structures file
        /// AND a structures ini file describing a gated weir with an empty crest width
        /// WHEN this structure is read
        /// THEN no exceptions are thrown
        /// AND the weir crest width is NaN
        /// </summary>
        [TestCase(true)]
        [Category(TestCategory.DataAccess)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileAndAStructuresIniFileDescribingAGatedWeirWithAnEmptyCrestWidth_WhenThisStructureIsRead_ThenNoExceptionsAreThrownAndTheWeirCrestWidthIsNaN(bool hasExplicitField)
        {
            // Given
            // - structures file
            StructuresFile structuresFile = GetStructuresFile();

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                const string structureName = "open-the-gate-a-little";
                string fileIniPath = Path.Combine(tempDir, "structures.ini");

                WritePliFileSingleStructure(tempDir, structureName);

                DelftIniCategory structureCategory = GetBaseStructureCategory(structureName, "gate");
                SetSimpleGateRequiredProperties(structureCategory);

                if (hasExplicitField)
                {
                    structureCategory.AddProperty(KnownStructureProperties.CrestWidth, " ", "#");
                }

                WriteStructuresIniFile(structureCategory, tempDir);

                // When | Then
                // - Read structures file
                IList<IStructureObject> structures = null;
                Assert.DoesNotThrow(() => { structures = structuresFile.Read(fileIniPath); });

                AssertThatOnlyOneStructureExistsWithin(structures);

                IStructureObject weirStructure = structures[0];
                AssertThatStructureIsCorrect(weirStructure, typeof(SimpleGateFormula));
            });
        }

        /// <summary>
        /// GIVEN a structures file
        /// AND a structures ini file describing a general structure with empty fields
        /// WHEN this structure is read
        /// THEN no exceptions are thrown
        /// AND the empty fields contain NaN
        /// </summary>
        [TestCase(true)]
        [Category(TestCategory.DataAccess)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileAndAStructuresIniFileDescribingAGeneralStructureWithEmptyFields_WhenThisStructureIsRead_ThenNoExceptionsAreThrownAndTheEmptyFieldsContainNaN(bool hasExplicitFields)
        {
            // Given
            // - structures file
            StructuresFile structuresFile = GetStructuresFile();

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                const string structureName = "general-structure-sir";
                string fileIniPath = Path.Combine(tempDir, "structures.ini");

                WritePliFileSingleStructure(tempDir, structureName);

                DelftIniCategory structureCategory = GetBaseStructureCategory(structureName, "generalstructure");
                SetGeneralStructureRequiredProperties(structureCategory);

                if (hasExplicitFields)
                {
                    structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.Upstream2Width), " ", "#");
                    structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.Upstream1Width), " ", "#");
                    structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.CrestWidth), " ", "#");
                    structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.Downstream1Width), " ", "#");
                    structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.Downstream2Width), " ", "#");
                }

                WriteStructuresIniFile(structureCategory, tempDir);

                // When | Then
                // - Read structures file
                IList<IStructureObject> structures = null;
                Assert.DoesNotThrow(() => { structures = structuresFile.Read(fileIniPath); });

                AssertThatOnlyOneStructureExistsWithin(structures);

                IStructureObject weirStructure = structures[0];
                AssertThatStructureIsCorrect(weirStructure, typeof(GeneralStructureFormula));
                var generalStructureFormula = ((Structure) weirStructure).Formula as GeneralStructureFormula;
                AssertThatAdditionalGeneralStructureIsCorrect(generalStructureFormula);
            });
        }

        #region TestHelpers

        private static string GetName(KnownGeneralStructureProperties prop)
        {
            return prop.GetDescription();
        }

        /// <summary>
        /// Get the structures file with the default StructureSchema.
        /// </summary>
        /// <returns> A new StructuresFile with the a default schema.</returns>
        private static StructuresFile GetStructuresFile()
        {
            StructureSchema<ModelPropertyDefinition> schema = new StructureSchemaCsvFile().ReadStructureSchema(
                StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);

            var structuresFile = new StructuresFile() {StructureSchema = schema};
            return structuresFile;
        }

        /// <summary>
        /// Write the <paramref name="structureCategory"/> to the structures ini file in the
        /// specified <paramref name="tempDir"/>.
        /// </summary>
        /// <param name="structureCategory">The structure category.</param>
        /// <param name="tempDir">The temporary dir.</param>
        /// <remarks> The newly created file is always called structures.ini </remarks>
        private static void WriteStructuresIniFile(DelftIniCategory structureCategory, string tempDir)
        {
            var categories = new List<DelftIniCategory>() {structureCategory};

            string fileIniPath = Path.Combine(tempDir, "structures.ini");
            new DelftIniWriter().WriteDelftIniFile(categories, fileIniPath);
        }

        /// <summary>
        /// Write the pli file of a single structure in the specified <paramref name="tempDir"/>.
        /// </summary>
        /// <param name="tempDir">The temporary dir.</param>
        /// <param name="structureName">Name of the structure.</param>
        private static void WritePliFileSingleStructure(string tempDir, string structureName)
        {
            var pliFileLines = new List<string>()
            {
                structureName,
                "    2    2",
                "10.0E+001  80.0E+001",
                "10.0E+002  80.0E+002"
            };
            string PliFileContents = string.Join("\n", pliFileLines);
            string filePliPath = Path.Combine(tempDir, $"{structureName}.pli");

            File.WriteAllText(filePliPath, PliFileContents);
        }

        /// <summary>
        /// Get the base structure category.
        /// </summary>
        /// <param name="structureName">Name of the structure.</param>
        /// <param name="structureType">Type of the structure.</param>
        /// <returns> A DelftIniCategory describing the category, type, name and geo file. </returns>
        private static DelftIniCategory GetBaseStructureCategory(string structureName, string structureType)
        {
            var structureCategory = new DelftIniCategory("structure");
            structureCategory.AddProperty(KnownStructureProperties.Type, structureType, "#");
            structureCategory.AddProperty(KnownStructureProperties.Name, structureName, "#");
            structureCategory.AddProperty(KnownStructureProperties.PolylineFile, $"{structureName}.pli", "#");
            return structureCategory;
        }

        private static void SetSimpleWeirRequiredProperties(DelftIniCategory structureCategory)
        {
            structureCategory.AddProperty(KnownStructureProperties.CrestLevel, "0.0", "#");
            structureCategory.AddProperty(KnownStructureProperties.LateralContractionCoefficient, "1.0", "#");
        }

        private static void SetSimpleGateRequiredProperties(DelftIniCategory structureCategory)
        {
            structureCategory.AddProperty(KnownStructureProperties.CrestLevel, "0.0", "#");
            structureCategory.AddProperty(KnownStructureProperties.GateLowerEdgeLevel, "0.0", "#");
            structureCategory.AddProperty(KnownStructureProperties.GateOpeningWidth, "0.0", "#");
            structureCategory.AddProperty(KnownStructureProperties.GateHeight, "0.0", "#");
            structureCategory.AddProperty(KnownStructureProperties.GateOpeningHorizontalDirection, "symmetric", "#");
        }

        private static void SetGeneralStructureRequiredProperties(DelftIniCategory structureCategory)
        {
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.Upstream2Level), "0.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.Upstream1Level), "0.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.CrestLevel), "0.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.Downstream1Level), "0.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.Downstream2Level), "0.0", "#");

            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.GateLowerEdgeLevel), "0.0", "#");

            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.PositiveContractionCoefficientFreeGate), "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient), "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient), "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient), "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient), "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.NegativeContractionCoefficientFreeGate), "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient), "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient), "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient), "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient), "1.0", "#");

            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.ExtraResistance), "0.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.GateHeight), "0.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.GateOpeningWidth), "0.0", "#");

            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.GateOpeningHorizontalDirection), "symmetric", "#");
        }

        private static void AssertThatStructureIsCorrect(IStructureObject structure, Type expectedWeirFormulaType)
        {
            Assert.That(structure, Is.TypeOf(typeof(Structure)), "Expected read structure to be of a different type:");
            var weirdStructure = (Structure) structure;

            Assert.That(weirdStructure.Formula, Is.Not.Null, "Expected weir formula to not be null.");
            Assert.That(weirdStructure.Formula, Is.TypeOf(expectedWeirFormulaType),
                        "Expected weir formula to be of a different type:");
            Assert.That(weirdStructure.CrestWidth, Is.NaN, "Expected weir's crest width to be Empty:");
        }

        private static void AssertThatOnlyOneStructureExistsWithin(IList<IStructureObject> structures)
        {
            Assert.That(structures, Is.Not.Null, "Expected the list of read structures to not be null.");
            Assert.That(structures.Count, Is.EqualTo(1), "Expected a different number of read structures:");
            IStructureObject weirStructure = structures[0];
            Assert.That(weirStructure, Is.Not.Null, "Expected read structure to not be null.");
        }

        private static void AssertThatAdditionalGeneralStructureIsCorrect(GeneralStructureFormula formula)
        {
            Assert.That(formula.Downstream2Width, Is.NaN, "Expected general structure's Downstream 2 width to be Empty:");
            Assert.That(formula.Downstream1Width, Is.NaN, "Expected general structure's Downstream 1 width to be Empty:");

            Assert.That(formula.Upstream1Width, Is.NaN, "Expected general structure's Upstream 1 width to be Empty:");
            Assert.That(formula.Upstream2Width, Is.NaN, "Expected general structure's Upstream 2 width to be Empty:");
        }

        #endregion
    }
}