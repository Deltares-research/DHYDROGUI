using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
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
            Structure simpleWeir = GetSimpleWeir(simpleWeirName);

            const string gatedWeirName = "Open-the-gate-a-little";
            Structure gatedWeir = GetGatedWeir(gatedWeirName);

            const string generalStructureName = "general-structure-sir";
            Structure generalStructure = GetGeneralStructure(generalStructureName);

            var writtenStructures = new List<IStructureObject>()
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

                IList<IStructureObject> readStructures = null;
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
        private static void AssertThatReadStructuresAreEquivalentToWrittenStructures(IEnumerable<IStructureObject> readStructures, IEnumerable<IStructureObject> writtenStructures)
        {
            List<IStructureObject> readStructuresList = readStructures.ToList();
            foreach (IStructureObject writtenStructure in writtenStructures)
            {
                IStructureObject readStructure = readStructuresList.FirstOrDefault(s => s.Name == writtenStructure.Name);
                Assert.That(readStructure, Is.Not.Null, $"Read structures does not contain structure with name {writtenStructure.Name}");

                Assert.That(readStructure, Is.TypeOf<Structure>(), $"Expected structure {readStructure.Name} of type <Weir2D>");
                var readWeir = (Structure) readStructure;
                var writtenWeir = (Structure) writtenStructure;

                Assert.That(readWeir.Formula, Is.Not.Null, $"Expected the weir formula of {readStructure.Name} not to be null.");
                Assert.That(readWeir.Formula, Is.TypeOf(writtenWeir.Formula.GetType()));

                Assert.That(readWeir.CrestWidth, Is.EqualTo(writtenWeir.CrestWidth), "Expected read crest width to be equal to written crest width:");
                Assert.That(readWeir.CrestLevel, Is.EqualTo(writtenWeir.CrestLevel), "Expected read crest level to be equal to written crest level:");

                // Extra values of general structure.
                if (writtenWeir.Formula is GeneralStructureFormula writtenGSFormula)
                {
                    var readGSFormula = readWeir.Formula as GeneralStructureFormula;
                    Assert.That(readGSFormula.Upstream1Level, Is.EqualTo(writtenGSFormula.Upstream1Level));
                    Assert.That(readGSFormula.Downstream2Level, Is.EqualTo(writtenGSFormula.Downstream2Level));
                    Assert.That(readGSFormula.Upstream2Level, Is.EqualTo(writtenGSFormula.Upstream2Level));
                    Assert.That(readGSFormula.Downstream1Level, Is.EqualTo(writtenGSFormula.Downstream1Level));

                    Assert.That(readGSFormula.Upstream1Width, Is.EqualTo(writtenGSFormula.Upstream1Width));
                    Assert.That(readGSFormula.Downstream2Width, Is.EqualTo(writtenGSFormula.Downstream2Width));
                    Assert.That(readGSFormula.Upstream2Width, Is.EqualTo(writtenGSFormula.Upstream2Width));
                    Assert.That(readGSFormula.Downstream1Width, Is.EqualTo(writtenGSFormula.Downstream1Width));
                }
            }
        }

        private static Structure GetGeneralStructure(string weirName)
        {
            var generalStructureFormula = new GeneralStructureFormula()
            {
                Upstream1Width = double.NaN,
                Downstream2Width = double.NaN,
                CrestWidth = double.NaN,
                Upstream2Width = double.NaN,
                Downstream1Width = double.NaN
            };

            return new Structure()
            {
                Name = weirName,
                Formula = generalStructureFormula,
                CrestWidth = double.NaN,
                Geometry = new Point(0.0, 0.0)
            };
        }

        private static Structure GetGatedWeir(string weirName) =>
            new Structure()
            {
                Name = weirName,
                Formula = new SimpleGateFormula(true),
                CrestWidth = double.NaN,
                Geometry = new Point(0.0, 0.0)
            };

        private static Structure GetSimpleWeir(string weirName) =>
            new Structure()
            {
                Name = weirName,
                Formula = new SimpleWeirFormula(),
                CrestWidth = double.NaN,
                Geometry = new Point(0.0, 0.0)
            };

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

        #endregion
    }
}