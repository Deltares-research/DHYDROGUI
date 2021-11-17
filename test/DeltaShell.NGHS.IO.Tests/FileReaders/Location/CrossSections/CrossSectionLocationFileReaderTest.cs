using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders.Location.CrossSections;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Location.CrossSections
{
    [TestFixture]
    public class CrossSectionLocationFileReaderTest
    {
        [Test]
        public void Constructor_DelftIniReaderNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new CrossSectionLocationFileReader(null);
            
            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("delftIniReader"));
        }
        
        [Test]
        public void Read_FileDoesNotExist_ReturnsEmptyCollection()
        {
            // Setup
            var delftIniReader = new DelftIniReader();
            var reader = new CrossSectionLocationFileReader(delftIniReader);

            // Call
            IEnumerable<CrossSectionLocation> locations = reader.Read(@"does\not\exist.def");

            // Assert
            Assert.That(locations, Is.Empty);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_ReadFileCorrectly()
        {
            // Setup
            var delftIniReader = new DelftIniReader();
            var reader = new CrossSectionLocationFileReader(delftIniReader);

            string fileContent = string.Join(
                Environment.NewLine,
                "[General]",
                "    fileVersion           = 1.01",
                "    fileType              = crossLoc",
                "",
                "[CrossSection]",
                "    Id                    = some_id_1",
                "    branchId              = some_branch_id_1",
                "    chainage              = 1.23",
                "    shift                 = 2.34",
                "    definitionId          = some_definition_id_1",
                "",
                "[Unsupported]",
                "    Id                    = some_id_2",
                "    branchId              = some_branch_id_2",
                "    chainage              = 3.45",
                "    shift                 = 4.56",
                "    definitionId          = some_definition_id_2",
                "",
                "[CrossSection]",
                "    Id                    = some_id_3",
                "    name                  = some_long_name_3",
                "    branchId              = some_branch_id_3",
                "    chainage              = 5.67",
                "    shift                 = 6.78",
                "    definitionId          = some_definition_id_3"
            );

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CreateFile("crsloc.ini", fileContent);

                // Call
                CrossSectionLocation[] locations = reader.Read(filePath).ToArray();

                // Assert
                Assert.That(locations, Has.Length.EqualTo(2));

                Assert.That(locations[0].Id, Is.EqualTo("some_id_1"));
                Assert.That(locations[0].LongName, Is.Null);
                Assert.That(locations[0].BranchId, Is.EqualTo("some_branch_id_1"));
                Assert.That(locations[0].Chainage, Is.EqualTo(1.23));
                Assert.That(locations[0].Shift, Is.EqualTo(2.34));
                Assert.That(locations[0].DefinitionId, Is.EqualTo("some_definition_id_1"));

                Assert.That(locations[1].Id, Is.EqualTo("some_id_3"));
                Assert.That(locations[1].LongName, Is.EqualTo("some_long_name_3"));
                Assert.That(locations[1].BranchId, Is.EqualTo("some_branch_id_3"));
                Assert.That(locations[1].Chainage, Is.EqualTo(5.67));
                Assert.That(locations[1].Shift, Is.EqualTo(6.78));
                Assert.That(locations[1].DefinitionId, Is.EqualTo("some_definition_id_3"));
            }
        }

        [Test]
        [TestCaseSource(nameof(GetPropertyNotFoundCases))]
        public void Read_PropertyNotFound_LogsErrorAndDoesNotCreateCrossSectionLocation(string fileContent, string property)
        {
            // Setup
            var delftIniReader = new DelftIniReader();
            var reader = new CrossSectionLocationFileReader(delftIniReader);

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CreateFile("crsloc.ini", fileContent);

                // Call
                CrossSectionLocation[] locations = null;

                void Call()
                {
                    locations = reader.Read(filePath).ToArray();
                }

                // Assert
                string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
                Assert.That(error, Is.EqualTo($"Property '{property}' is not found in the file for category 'CrossSection' on line 1"));
                Assert.That(locations, Is.Empty);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetPropertyEmptyValueCases))]
        public void Read_PropertyEmptyValue_LogsErrorAndDoesNotCreateCrossSectionLocation(string fileContent, string property, int lineNumber)
        {
            // Setup
            var delftIniReader = new DelftIniReader();
            var reader = new CrossSectionLocationFileReader(delftIniReader);

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CreateFile("crsloc.ini", fileContent);

                // Call
                CrossSectionLocation[] locations = null;

                void Call()
                {
                    locations = reader.Read(filePath).ToArray();
                }

                // Assert
                string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
                Assert.That(error, Is.EqualTo($"Property '{property}' does not contain a value in the file for category 'CrossSection' on line {lineNumber}"));
                Assert.That(locations, Is.Empty);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetPropertyInvalidDoubleCases))]
        public void Read_PropertyInvalidDouble_LogsErrorAndDoesNotCreateCrossSectionLocation(string fileContent, string property, int lineNumber)
        {
            // Setup
            var delftIniReader = new DelftIniReader();
            var reader = new CrossSectionLocationFileReader(delftIniReader);

            using (var temp = new TemporaryDirectory())
            {
                string filePath = temp.CreateFile("crsloc.ini", fileContent);

                // Call
                CrossSectionLocation[] locations = null;

                void Call()
                {
                    locations = reader.Read(filePath).ToArray();
                }

                // Assert
                string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
                Assert.That(error, Is.EqualTo($"Property '{property}' contains an invalid floating-point number in the file for category 'CrossSection' on line {lineNumber}: a.bc"));
                Assert.That(locations, Is.Empty);
            }
        }

        private static IEnumerable<TestCaseData> GetPropertyNotFoundCases()
        {
            string contentWithoutId = string.Join(
                Environment.NewLine,
                "[CrossSection]",
                "    branchId              = some_branch_id",
                "    chainage              = 1.23",
                "    shift                 = 2.34",
                "    definitionId          = some_definition_id"
            );

            string contentWithoutBranchId = string.Join(
                Environment.NewLine,
                "[CrossSection]",
                "    Id                    = some_id",
                "    chainage              = 1.23",
                "    shift                 = 2.34",
                "    definitionId          = some_definition_id"
            );

            string contentWithoutChainage = string.Join(
                Environment.NewLine,
                "[CrossSection]",
                "    Id                    = some_id",
                "    branchId              = some_branch_id",
                "    shift                 = 2.34",
                "    definitionId          = some_definition_id"
            );

            string contentWithoutShift = string.Join(
                Environment.NewLine,
                "[CrossSection]",
                "    Id                    = some_id",
                "    branchId              = some_branch_id",
                "    chainage              = 1.23",
                "    definitionId          = some_definition_id"
            );

            string contentWithoutDefinitionId = string.Join(
                Environment.NewLine,
                "[CrossSection]",
                "    Id                    = some_id",
                "    branchId              = some_branch_id",
                "    chainage              = 1.23",
                "    shift                 = 2.34"
            );

            yield return new TestCaseData(contentWithoutId, "Id");
            yield return new TestCaseData(contentWithoutBranchId, "branchId");
            yield return new TestCaseData(contentWithoutChainage, "chainage");
            yield return new TestCaseData(contentWithoutShift, "shift");
            yield return new TestCaseData(contentWithoutDefinitionId, "definitionId");
        }

        private static IEnumerable<TestCaseData> GetPropertyEmptyValueCases()
        {
            string contentWithoutId = string.Join(
                Environment.NewLine,
                "[CrossSection]",
                "    Id                    = ",
                "    branchId              = some_branch_id",
                "    chainage              = 1.23",
                "    shift                 = 2.34",
                "    definitionId          = some_definition_id"
            );

            string contentWithoutBranchId = string.Join(
                Environment.NewLine,
                "[CrossSection]",
                "    Id                    = some_id",
                "    branchId              = ",
                "    chainage              = 1.23",
                "    shift                 = 2.34",
                "    definitionId          = some_definition_id"
            );

            string contentWithoutChainage = string.Join(
                Environment.NewLine,
                "[CrossSection]",
                "    Id                    = some_id",
                "    branchId              = some_branch_id",
                "    chainage              = ",
                "    shift                 = 2.34",
                "    definitionId          = some_definition_id"
            );

            string contentWithoutShift = string.Join(
                Environment.NewLine,
                "[CrossSection]",
                "    Id                    = some_id",
                "    branchId              = some_branch_id",
                "    chainage              = 1.23",
                "    shift                 = ",
                "    definitionId          = some_definition_id"
            );

            string contentWithoutDefinitionId = string.Join(
                Environment.NewLine,
                "[CrossSection]",
                "    Id                    = some_id",
                "    branchId              = some_branch_id",
                "    chainage              = 1.23",
                "    shift                 = 2.34",
                "    definitionId          = "
            );

            yield return new TestCaseData(contentWithoutId, "Id", 2);
            yield return new TestCaseData(contentWithoutBranchId, "branchId", 3);
            yield return new TestCaseData(contentWithoutChainage, "chainage", 4);
            yield return new TestCaseData(contentWithoutShift, "shift", 5);
            yield return new TestCaseData(contentWithoutDefinitionId, "definitionId", 6);
        }

        private static IEnumerable<TestCaseData> GetPropertyInvalidDoubleCases()
        {
            string contentInvalidChainage = string.Join(
                Environment.NewLine,
                "[CrossSection]",
                "    Id                    = some_id",
                "    branchId              = some_branch_id",
                "    chainage              = a.bc",
                "    shift                 = 2.34",
                "    definitionId          = some_definition_id"
            );

            string contentInvalidShift = string.Join(
                Environment.NewLine,
                "[CrossSection]",
                "    Id                    = some_id",
                "    branchId              = some_branch_id",
                "    chainage              = 1.23",
                "    shift                 = a.bc",
                "    definitionId          = some_definition_id"
            );

            yield return new TestCaseData(contentInvalidChainage, "chainage", 4);
            yield return new TestCaseData(contentInvalidShift, "shift", 5);
        }
    }
}