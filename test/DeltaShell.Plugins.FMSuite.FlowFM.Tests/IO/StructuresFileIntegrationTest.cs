using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class StructuresFileIntegrationTest
    {
        /// <summary>
        /// GIVEN a structures file
        /// AND structures containing empty width fields
        /// WHEN these structures are written
        /// AND these structure are read
        /// THEN the obtained structures are equal to the original structures
        /// AND no exceptions are thrown
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void GivenStructuresContainingEmptyWidthFields_WhenTheseStructuresAreWrittenAndTheseStructuresAreRead_ThenTheObtainedStructuresAreEqualToTheOriginalStructuresAndNoExceptionsAreThrown()
        {
            // Given
            StructuresFile structuresFile = GetStructuresFile();

            const string simpleWeirName = "Its-weir-d";
            Weir2D simpleWeir = GetSimpleWeir(simpleWeirName);

            const string gatedWeirName = "Open-the-gate-a-little";
            Weir2D gatedWeir = GetGatedWeir(gatedWeirName);

            const string generalStructureName = "general-structure-sir";
            Weir2D generalStructure = GetGeneralStructure(generalStructureName);

            var writtenStructures = new List<IStructure>()
            {
                simpleWeir,
                gatedWeir,
                generalStructure
            };

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                string exportFilePath = Path.Combine(tempDir, "FlowFM_structures.ini");

                // When | Then
                Assert.DoesNotThrow(() => { structuresFile.Write(exportFilePath, writtenStructures); });

                IList<IStructure> readStructures = null;
                Assert.DoesNotThrow(() => { readStructures = structuresFile.Read(exportFilePath); });

                Assert.That(readStructures, Is.Not.Null, "Expected read structures to not be null.");
                Assert.That(readStructures.Count, Is.EqualTo(3), "Expected a different number of read structures:");

                AssertThatReadStructuresAreEquivalentToWrittenStructures(readStructures, writtenStructures);
            });
        }

        #region TestHelpers

        /// <summary>
        /// Assert that the structural values of the read structures are equivalent to those of the written structures.
        /// </summary>
        /// <param name="readStructures">The read structures.</param>
        /// <param name="writtenStructures">The written structures.</param>
        /// <remarks>
        /// This method checks that each element of the written structures exists
        /// within the read structures, and that the name, crest width, crest level,
        /// and formulas are equivalent.
        /// </remarks>
        private static void AssertThatReadStructuresAreEquivalentToWrittenStructures(IEnumerable<IStructure> readStructures, IEnumerable<IStructure> writtenStructures)
        {
            List<IStructure> readStructuresList = readStructures.ToList();
            foreach (IStructure writtenStructure in writtenStructures)
            {
                IStructure readStructure = readStructuresList.FirstOrDefault(s => s.Name == writtenStructure.Name);
                Assert.That(readStructure, Is.Not.Null, $"Read structures does not contain structure with name {writtenStructure.Name}");

                Assert.That(readStructure, Is.TypeOf<Weir2D>(), $"Expected structure {readStructure.Name} of type <Weir2D>");
                var readWeir = (Weir2D) readStructure;
                var writtenWeir = (Weir2D) writtenStructure;

                Assert.That(readWeir.WeirFormula, Is.Not.Null, $"Expected the weir formula of {readStructure.Name} not to be null.");
                Assert.That(readWeir.WeirFormula, Is.TypeOf(writtenWeir.WeirFormula.GetType()));

                Assert.That(readWeir.CrestWidth, Is.EqualTo(writtenWeir.CrestWidth), "Expected read crest width to be equal to written crest width:");
                Assert.That(readWeir.CrestLevel, Is.EqualTo(writtenWeir.CrestLevel), "Expected read crest level to be equal to written crest level:");

                // Extra values of general structure.
                if (writtenWeir.WeirFormula is GeneralStructureWeirFormula writtenGSFormula)
                {
                    var readGSFormula = readWeir.WeirFormula as GeneralStructureWeirFormula;
                    Assert.That(readGSFormula.BedLevelLeftSideOfStructure, Is.EqualTo(writtenGSFormula.BedLevelLeftSideOfStructure));
                    Assert.That(readGSFormula.BedLevelRightSideOfStructure, Is.EqualTo(writtenGSFormula.BedLevelRightSideOfStructure));
                    Assert.That(readGSFormula.BedLevelLeftSideStructure, Is.EqualTo(writtenGSFormula.BedLevelLeftSideStructure));
                    Assert.That(readGSFormula.BedLevelRightSideStructure, Is.EqualTo(writtenGSFormula.BedLevelRightSideStructure));

                    Assert.That(readGSFormula.WidthLeftSideOfStructure, Is.EqualTo(writtenGSFormula.WidthLeftSideOfStructure));
                    Assert.That(readGSFormula.WidthRightSideOfStructure, Is.EqualTo(writtenGSFormula.WidthRightSideOfStructure));
                    Assert.That(readGSFormula.WidthStructureLeftSide, Is.EqualTo(writtenGSFormula.WidthStructureLeftSide));
                    Assert.That(readGSFormula.WidthStructureRightSide, Is.EqualTo(writtenGSFormula.WidthStructureRightSide));
                }
            }
        }

        private static Weir2D GetGeneralStructure(string weirName)
        {
            var generalStructureFormula = new GeneralStructureWeirFormula()
            {
                WidthLeftSideOfStructure = double.NaN,
                WidthRightSideOfStructure = double.NaN,
                WidthStructureCentre = double.NaN,
                WidthStructureLeftSide = double.NaN,
                WidthStructureRightSide = double.NaN
            };

            var generalStructure = new Weir2D(weirName)
            {
                WeirFormula = generalStructureFormula,
                CrestWidth = double.NaN,
                Geometry = new Point(0.0, 0.0)
            };
            return generalStructure;
        }

        private static Weir2D GetGatedWeir(string weirName)
        {
            var gatedWeir = new Weir2D(weirName)
            {
                WeirFormula = new GatedWeirFormula(true),
                CrestWidth = double.NaN,
                Geometry = new Point(0.0, 0.0)
            };
            return gatedWeir;
        }

        private static Weir2D GetSimpleWeir(string weirName)
        {
            var simpleWeir = new Weir2D(weirName)
            {
                WeirFormula = new SimpleWeirFormula(),
                CrestWidth = double.NaN,
                Geometry = new Point(0.0, 0.0)
            };
            return simpleWeir;
        }

        /// <summary>
        /// Gets the structures file.
        /// </summary>
        /// <returns> A new StructuresFile with the a default schema.</returns>
        private static StructuresFile GetStructuresFile()
        {
            StructureSchema<ModelPropertyDefinition> schema = new StructureSchemaCsvFile().ReadStructureSchema(
                StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);

            var structuresFile = new StructuresFile()
            {
                StructureSchema = schema
            };
            return structuresFile;
        }

        #endregion
    }
}