using System.Collections.Generic;
using System.IO;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using NSubstitute;
using NUnit.Framework;
using IStructure = DelftTools.Hydro.IStructure;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class StructureTimFileNameGeneratorTest
    {
        private static IEnumerable<TestCaseData> GenerateArgumentNullData()
        {
            yield return new TestCaseData(null, Substitute.For<ITimeSeries>(), "structure");
            yield return new TestCaseData(Substitute.For<IStructure>(), null, "timeSeries");
        }

        [Test]
        [TestCaseSource(nameof(GenerateArgumentNullData))]
        public void Generate_ArgumentNull_ThrowsArgumentNullArgument(IStructure structure,
                                                                     ITimeSeries timeSeries,
                                                                     string paramName)
        {
            void Call() => StructureTimFileNameGenerator.Generate(structure, timeSeries);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(paramName));
        }

        private static IEnumerable<TestCaseData> GenerateValidArgumentsData()
        {
            TestCaseData GenerateTestCaseData(string structureName,
                                              string seriesName,
                                              string expectedName,
                                              string testName)
            {
                var structure = Substitute.For<IStructure>();
                structure.Name = structureName;
                var timeSeries = Substitute.For<ITimeSeries>();
                timeSeries.Name = seriesName;
                return new TestCaseData(structure, timeSeries, expectedName)
                    .SetName(testName);
            }

            const string validStructureName = "Structure";
            const string validSeriesName = "series";
            string validExpectedTimeFile = $"{validStructureName}_{validSeriesName}.tim";

            yield return GenerateTestCaseData(validStructureName,
                                              validSeriesName,
                                              validExpectedTimeFile,
                                              "lowercase | no invalid characters");
            yield return GenerateTestCaseData(validStructureName,
                                              validSeriesName.ToUpperInvariant(),
                                              validExpectedTimeFile,
                                              "capitalized | no invalid characters");

            const string spacesStructureName = "Str ct re";
            const string spacesSeriesName = "s ri s";
            const string spacesExpectedTimeFile = "Str_ct_re_s_ri_s.tim";
            yield return GenerateTestCaseData(spacesStructureName,
                                              spacesSeriesName,
                                              spacesExpectedTimeFile,
                                              "lowercase | spaces");
            yield return GenerateTestCaseData(spacesStructureName,
                                              spacesSeriesName.ToUpperInvariant(),
                                              spacesExpectedTimeFile,
                                              "capitalized | spaces");

            const string invalidStructureNameFormat = "Str{0}cture";
            const string invalidSeriesNameFormat = "s{0}ries";
            const string invalidExpectedTimeFile = "Str_cture_s_ries.tim";

            foreach (char c in Path.GetInvalidFileNameChars())
            { 
                yield return GenerateTestCaseData(string.Format(invalidStructureNameFormat, c),
                                                string.Format(invalidSeriesNameFormat, c),
                                                invalidExpectedTimeFile,
                                                $"lowercase | invalid character: {c}"); 
                yield return GenerateTestCaseData(string.Format(invalidStructureNameFormat, c),
                                                  string.Format(invalidSeriesNameFormat, c).ToUpperInvariant(),
                                                  invalidExpectedTimeFile,
                                                  $"uppercase | invalid character: {c}");
            }
        }

        [Test]
        [TestCaseSource(nameof(GenerateValidArgumentsData))]
        public void Generate_ValidArguments_ExpectedResults(IStructure structure,
                                             ITimeSeries timeSeries,
                                             string expectedTimeFileName)
        {
            string result = StructureTimFileNameGenerator.Generate(structure, timeSeries);
            Assert.That(result, Is.EqualTo(expectedTimeFileName));
        }
    }
}