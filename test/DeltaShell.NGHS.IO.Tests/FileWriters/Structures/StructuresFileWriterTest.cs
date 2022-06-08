using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
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
            DelftIniCategory GenerateCategory(int index)
            {
                var result = new DelftIniCategory("Structure");
                result.AddProperty("a", random.NextDouble(), $"comment {index * 3 + 0}");
                result.AddProperty("b", random.Next(), $"comment {index * 3 + 1}");
                result.AddProperty("c", random.Next().ToString("X"), $"comment {index * 3 + 2}");

                return result;
            }

            DelftIniCategory[] noCategories = {};
            yield return new TestCaseData((IList<DelftIniCategory>)noCategories).SetName("No Categories");

            DelftIniCategory[] someCategories = 
                Enumerable.Range(0, 5).Select(GenerateCategory).ToArray();
            yield return new TestCaseData((IList<DelftIniCategory>)someCategories).SetName("Multiple Categories");
        }

        [Test]
        [TestCaseSource(nameof(WriteFileData))]
        public void WriteFile_ExpectedResult(IList<DelftIniCategory> categories)
        {
            // Setup
            DateTime referenceTime = DateTime.Today;
            string filePath = Path.Combine(TempDir.Path, "structures.ini");

            bool isCalled = false;
            IHydroRegion[] hydroRegions = {};

            IEnumerable<DelftIniCategory> CreateStructureCategories(IEnumerable<IHydroRegion> regions,
                                                                    DateTime refTime)
            {
                Assert.That(regions, Is.EqualTo(hydroRegions));
                Assert.That(refTime, Is.EqualTo(referenceTime));

                isCalled = true;
                return categories;
            }

            // Call
            StructureFileWriter.WriteFile(filePath, 
                                          hydroRegions, 
                                          referenceTime, 
                                          CreateStructureCategories);

            // Assert
            Assert.That(isCalled, Is.True);
            Assert.That(File.Exists(filePath), Is.True);

            IList<DelftIniCategory> result = 
                new DelftIniReader().ReadDelftIniFile(filePath);

            IList<DelftIniCategory> expectedCategories = 
                GetExpectedCategories(categories);

            Assert.That(result.Count, Is.EqualTo(expectedCategories.Count));

            IEnumerable<(DelftIniCategory, DelftIniCategory)> resultCategories = result.Zip(expectedCategories, (c1, c2) => (c1, c2));
            foreach ((DelftIniCategory resultCat, DelftIniCategory expectedCat) in resultCategories)
                AssertSameCategory(resultCat, expectedCat);
        }

        private static void AssertSameCategory(IDelftIniCategory resultCat, IDelftIniCategory expectedCat)
        {
            Assert.That(resultCat.Name, Is.EqualTo(expectedCat.Name));
            Assert.That(resultCat.Properties.Count, Is.EqualTo(expectedCat.Properties.Count));

            IEnumerable<(DelftIniProperty, DelftIniProperty)> resultProps = resultCat.Properties.Zip(expectedCat.Properties, (p1, p2) => (p1, p2));
            foreach ((DelftIniProperty resProp, DelftIniProperty expectedProp) in resultProps)
                AssertSameProperty(resProp, expectedProp);
        }

        private static void AssertSameProperty(IDelftIniProperty resProp, 
                                               IDelftIniProperty expectedProp)
        {
            Assert.That(resProp.Name, Is.EqualTo(expectedProp.Name));
            Assert.That(resProp.Value, Is.EqualTo(expectedProp.Value));
        }

        private static IList<DelftIniCategory> GetExpectedCategories(
            IEnumerable<DelftIniCategory> providedCategories)
        {
            List<DelftIniCategory> result = new List<DelftIniCategory>()
            {
                GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.StructureDefinitionsMajorVersion,
                    GeneralRegion.StructureDefinitionsMinorVersion,
                    GeneralRegion.FileTypeName.StructureDefinition)
            };

            result.AddRange(providedCategories);
            return result;
        }
    }
}