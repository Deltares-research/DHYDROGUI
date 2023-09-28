using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class StructuresFileWriterTest : TemporaryDirectoryBaseFixture
    {
        private static readonly Random random = new Random();

        private static IEnumerable<TestCaseData> WriteFileData()
        {
            IniSection GenerateSection(int index)
            {
                var result = new IniSection("Structure");
                result.AddPropertyWithOptionalComment("a", random.NextDouble(), $"comment {index * 3 + 0}");
                result.AddPropertyWithOptionalComment("b", random.Next(), $"comment {index * 3 + 1}");
                result.AddPropertyWithOptionalComment("c", random.Next().ToString("X"), $"comment {index * 3 + 2}");

                return result;
            }

            IniSection[] noIniSections = {};
            yield return new TestCaseData((IList<IniSection>)noIniSections).SetName("No Sections");

            IniSection[] someIniSections = 
                Enumerable.Range(0, 5).Select(GenerateSection).ToArray();
            yield return new TestCaseData((IList<IniSection>)someIniSections).SetName("Multiple Sections");
        }

        [Test]
        [TestCaseSource(nameof(WriteFileData))]
        public void WriteFile_ExpectedResult(IList<IniSection> iniSections)
        {
            // Setup
            DateTime referenceTime = DateTime.Today;
            string filePath = Path.Combine(TempDir.Path, "structures.ini");

            bool isCalled = false;
            IHydroRegion[] hydroRegions = {};

            IEnumerable<IniSection> CreateStructureSections(IEnumerable<IHydroRegion> regions,
                                                                    DateTime refTime)
            {
                Assert.That(regions, Is.EqualTo(hydroRegions));
                Assert.That(refTime, Is.EqualTo(referenceTime));

                isCalled = true;
                return iniSections;
            }

            // Call
            StructureFileWriter.WriteFile(filePath, 
                                          hydroRegions, 
                                          referenceTime, 
                                          CreateStructureSections);

            // Assert
            Assert.That(isCalled, Is.True);
            Assert.That(File.Exists(filePath), Is.True);

            IList<IniSection> result = 
                new IniReader().ReadIniFile(filePath);

            IList<IniSection> expectedSections = 
                GetExpectedSections(iniSections);

            Assert.That(result.Count, Is.EqualTo(expectedSections.Count));

            IEnumerable<(IniSection, IniSection)> resultSections = result.Zip(expectedSections, (c1, c2) => (c1, c2));
            foreach ((IniSection resultCat, IniSection expectedCat) in resultSections)
                AssertSameSection(resultCat, expectedCat);
        }

        private static void AssertSameSection(IniSection resultCat, IniSection expectedCat)
        {
            Assert.That(resultCat.Name, Is.EqualTo(expectedCat.Name));
            Assert.That(resultCat.Properties.Count, Is.EqualTo(expectedCat.Properties.Count()));

            IEnumerable<(IniProperty, IniProperty)> resultProps = resultCat.Properties.Zip(expectedCat.Properties, (p1, p2) => (p1, p2));
            foreach ((IniProperty resProp, IniProperty expectedProp) in resultProps)
                AssertSameProperty(resProp, expectedProp);
        }

        private static void AssertSameProperty(IniProperty resProp, 
                                               IniProperty expectedProp)
        {
            Assert.That(resProp.Key, Is.EqualTo(expectedProp.Key));
            Assert.That(resProp.Value, Is.EqualTo(expectedProp.Value));
        }

        private static IList<IniSection> GetExpectedSections(
            IEnumerable<IniSection> providedSections)
        {
            List<IniSection> result = new List<IniSection>()
            {
                GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.StructureDefinitionsMajorVersion,
                    GeneralRegion.StructureDefinitionsMinorVersion,
                    GeneralRegion.FileTypeName.StructureDefinition)
            };

            result.AddRange(providedSections);
            return result;
        }
    }
}