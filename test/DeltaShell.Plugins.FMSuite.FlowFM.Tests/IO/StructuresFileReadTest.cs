using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO;
using NUnit.Framework;


namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class StructuresFileReadTest
    {
        /// <summary>
        /// GIVEN a structures file
        ///   AND a structures ini file describing a simple weir with an empty crest width
        /// WHEN this structure is read
        /// THEN no exceptions are thrown
        ///  AND the weir crest width is NaN
        /// </summary>
        [TestCase(true),  Category(TestCategory.DataAccess)]
        [TestCase(false), Category(TestCategory.DataAccess)]
        public void GivenAStructuresIniFileDescribingASimpleWeirWithAnEmptyCrestWidth_WhenThisStructureIsRead_ThenNoExceptionsAreThrownAndTheWeirCrestWidthIsNaN(bool hasExplicitField)
        {
            // Given
            // - structures file
            var structuresFile = GetStructuresFile();

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                const string structureName = "weir-d";
                var fileIniPath = Path.Combine(tempDir, "structures.ini");

                WritePliFileSingleStructure(tempDir, structureName);

                var structureCategory = GetBaseStructureCategory(structureName, "weir");
                SetSimpleWeirRequiredProperties(structureCategory);

                if (hasExplicitField)
                    structureCategory.AddProperty(KnownStructureProperties.CrestWidth, " ", "#");

                WriteStructuresIniFile(structureCategory, tempDir);

                // When | Then
                // - Read structures file
                IList<IStructure> structures = null;
                Assert.DoesNotThrow(() =>
                {
                    structures = structuresFile.Read(fileIniPath);
                });

                AssertThatOnlyOneStructureExistsWithin(structures);

                var weirStructure = structures[0];
                AssertThatStructureIsCorrect(weirStructure, typeof(SimpleWeirFormula));
            });
        }

        /// <summary>
        /// GIVEN a structures file
        ///   AND a structures ini file describing a gated weir with an empty crest width
        /// WHEN this structure is read
        /// THEN no exceptions are thrown
        ///  AND the weir crest width is NaN
        /// </summary>
        [TestCase(true),  Category(TestCategory.DataAccess)]
        [TestCase(false), Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileAndAStructuresIniFileDescribingAGatedWeirWithAnEmptyCrestWidth_WhenThisStructureIsRead_ThenNoExceptionsAreThrownAndTheWeirCrestWidthIsNaN(bool hasExplicitField)
        {
            // Given
            // - structures file
            var structuresFile = GetStructuresFile();

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                const string structureName = "open-the-gate-a-little";
                var fileIniPath = Path.Combine(tempDir, "structures.ini");

                WritePliFileSingleStructure(tempDir, structureName);

                var structureCategory = GetBaseStructureCategory(structureName, "gate");
                SetSimpleGateRequiredProperties(structureCategory);

                if (hasExplicitField)
                    structureCategory.AddProperty(KnownStructureProperties.GateSillWidth, " ", "#");

                WriteStructuresIniFile(structureCategory, tempDir);

                // When | Then
                // - Read structures file
                IList<IStructure> structures = null;
                Assert.DoesNotThrow(() =>
                {
                    structures = structuresFile.Read(fileIniPath);
                });

                AssertThatOnlyOneStructureExistsWithin(structures);

                var weirStructure = structures[0];
                AssertThatStructureIsCorrect(weirStructure, typeof(GatedWeirFormula));
            });
        }

        /// <summary>
        /// GIVEN a structures file
        ///   AND a structures ini file describing a general structure with empty fields
        /// WHEN this structure is read
        /// THEN no exceptions are thrown
        ///  AND the empty fields contain NaN
        /// </summary>
        [TestCase(true),  Category(TestCategory.DataAccess)]
        [TestCase(false), Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileAndAStructuresIniFileDescribingAGeneralStructureWithEmptyFields_WhenThisStructureIsRead_ThenNoExceptionsAreThrownAndTheEmptyFieldsContainNaN(bool hasExplicitFields)
        {
            // Given
            // - structures file
            var structuresFile = GetStructuresFile();

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                const string structureName = "general-structure-sir";
                var fileIniPath = Path.Combine(tempDir, "structures.ini");

                WritePliFileSingleStructure(tempDir, structureName);

                var structureCategory = GetBaseStructureCategory(structureName, "generalstructure");
                SetGeneralStructureRequiredProperties(structureCategory);

                if (hasExplicitFields)
                {
                    structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.WidthLeftW1),    " ", "#");
                    structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.WidthLeftWsdl),  " ", "#");
                    structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.WidthCenter),    " ", "#");
                    structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.WidthRightWsdr), " ", "#");
                    structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.WidthRightW2),   " ", "#");
                }

                WriteStructuresIniFile(structureCategory, tempDir);

                // When | Then
                // - Read structures file
                IList<IStructure> structures = null;
                Assert.DoesNotThrow(() =>
                {
                    structures = structuresFile.Read(fileIniPath);
                });

                AssertThatOnlyOneStructureExistsWithin(structures);

                var weirStructure = structures[0];
                AssertThatStructureIsCorrect(weirStructure, typeof(GeneralStructureWeirFormula));
                var generalStructureFormula = ((Weir2D) weirStructure).WeirFormula as GeneralStructureWeirFormula;
                AssertThatAdditionalGeneralStructureIsCorrect(generalStructureFormula);
            });
        }

        #region TestHelpers
        private static string GetName(KnownGeneralStructureProperties prop)
        {
            return EnumDescriptionAttributeTypeConverter.GetEnumDescription(prop);
        }

        /// <summary>
        /// Get the structures file with the default StructureSchema.
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
        /// Write the <paramref name="structureCategory"/> to the structures ini file in the
        /// specified <paramref name="tempDir"/>.
        /// </summary>
        /// <param name="structureCategory">The structure category.</param>
        /// <param name="tempDir">The temporary dir.</param>
        /// <remarks> The newly created file is always called structures.ini </remarks>
        private static void WriteStructuresIniFile(IDelftIniCategory structureCategory, string tempDir)
        {
            var categories = new List<IDelftIniCategory>() { structureCategory };

            var fileIniPath = Path.Combine(tempDir, "structures.ini");
            (new DelftIniWriter()).WriteDelftIniFile(categories, fileIniPath);
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
                "10.0E+002  80.0E+002",
            };
            var PliFileContents = string.Join("\n", pliFileLines);
            var filePliPath = Path.Combine(tempDir, $"{structureName}.pli");

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

        private static void SetSimpleWeirRequiredProperties(IDelftIniCategory structureCategory)
        {
            structureCategory.AddProperty(KnownStructureProperties.CrestLevel, "0.0", "#");
            structureCategory.AddProperty(KnownStructureProperties.LateralContractionCoefficient, "1.0", "#");
        }

        private static void SetSimpleGateRequiredProperties(IDelftIniCategory structureCategory)
        {
            structureCategory.AddProperty(KnownStructureProperties.GateSillLevel,      "0.0", "#");
            structureCategory.AddProperty(KnownStructureProperties.GateLowerEdgeLevel, "0.0", "#");
            structureCategory.AddProperty(KnownStructureProperties.GateOpeningWidth,   "0.0", "#");
            structureCategory.AddProperty(KnownStructureProperties.GateDoorHeight,     "0.0", "#");
            structureCategory.AddProperty(KnownStructureProperties.GateHorizontalOpeningDirection, "symmetric", "#");
        }

        private static void SetGeneralStructureRequiredProperties(IDelftIniCategory structureCategory)
        {
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.LevelLeftZb1),   "0.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.LevelLeftZbsl),  "0.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.LevelCenter),    "0.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.LevelRightZbsr), "0.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.LevelRightZb2),  "0.0", "#");

            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.GateHeight),     "0.0", "#");

            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.PositiveContractionCoefficientFreeGate), "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient),       "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient),       "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient),        "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient),        "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.NegativeContractionCoefficientFreeGate), "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient),       "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient),        "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient),       "1.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient),        "1.0", "#");

            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.ExtraResistance), "0.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.GateDoorHeightGeneralStructure), "0.0", "#");
            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.HorizontalDoorOpeningWidth), "0.0", "#");

            structureCategory.AddProperty(GetName(KnownGeneralStructureProperties.HorizontalDoorOpeningDirection), "symmetric", "#");
        }

        private static void AssertThatStructureIsCorrect(IStructure structure, Type expectedWeirFormulaType)
        {
            Assert.That(structure, Is.TypeOf(typeof(Weir2D)), "Expected read structure to be of a different type:");
            var weirdStructure = (Weir2D)structure;

            Assert.That(weirdStructure.WeirFormula, Is.Not.Null, "Expected weir formula to not be null.");
            Assert.That(weirdStructure.WeirFormula, Is.TypeOf(expectedWeirFormulaType),
                        "Expected weir formula to be of a different type:");
            Assert.That(weirdStructure.CrestWidth, Is.NaN, "Expected weir's crest width to be Empty:");
        }

        private static void AssertThatOnlyOneStructureExistsWithin(IList<IStructure> structures)
        {
            Assert.That(structures, Is.Not.Null, "Expected the list of read structures to not be null.");
            Assert.That(structures.Count, Is.EqualTo(1), "Expected a different number of read structures:");
            var weirStructure = structures[0];
            Assert.That(weirStructure, Is.Not.Null, "Expected read structure to not be null.");
        }

        private static void AssertThatAdditionalGeneralStructureIsCorrect(GeneralStructureWeirFormula formula)
        {
            Assert.That(formula.WidthRightSideOfStructure, Is.NaN, "Expected general structure's Downstream 2 width to be Empty:");
            Assert.That(formula.WidthStructureRightSide,   Is.NaN, "Expected general structure's Downstream 1 width to be Empty:");

            Assert.That(formula.WidthLeftSideOfStructure, Is.NaN, "Expected general structure's Upstream 1 width to be Empty:");
            Assert.That(formula.WidthStructureLeftSide,   Is.NaN, "Expected general structure's Upstream 2 width to be Empty:");
        }
        #endregion
    }
}
