using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Tests.TestObjects;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CrossSectionDefinitionExtensionsTest
    {
        private const string Section1Name = "section1";
        private const string Section2Name = "section2";
        private const string Section3Name = "section3";

        #region SectionsTotalWidth

        [Test]
        public void GivenCrossSectionDefinitionZwWithOneSection_WhenGettingTotalSectionsWidth_ThenResultIsCorrect()
        {
            var length = 20.0;
            var csDef = new CrossSectionDefinitionZW();
            csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, length);
            Assert.That(csDef.SectionsTotalWidth(), Is.EqualTo(length));
        }

        [Test]
        public void GivenCrossSectionDefinitionZwWithMultipleSections_WhenGettingTotalSectionsWidth_ThenResultIsCorrect()
        {
            var length1 = 20.0;
            var length2 = 55.0;
            var length3 = 89.0;
            var csDef = new CrossSectionDefinitionZW();
            csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, length1);
            csDef.AddSection(new CrossSectionSectionType { Name = Section2Name }, length2);
            csDef.AddSection(new CrossSectionSectionType { Name = Section3Name }, length3);
            Assert.That(csDef.SectionsTotalWidth(), Is.EqualTo(length1 + length2 + length3));
        }

        #endregion

        #region AddSection

        [Test]
        public void GivenCrossSectionDefinition_WhenTryingToAddSectionsWithNegativeWidth_ThenLogMessageAndSectionIsNotAdded()
        {
            var csDef = new TestCrossSectionDefinition() { Name = "myCrossSectionDefinition" };
            var expectedMessage =
                "Could not add CrossSectionSection with negative length -2 to cross section definition 'myCrossSectionDefinition'";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, -2.0), expectedMessage);
            Assert.False(csDef.Sections.Any());
        }

        [Test]
        public void GivenCrossSectionDefinition_WhenTryingToAddSectionWithDuplicateName_ThenLogMessageAndSectionIsNotAdded()
        {
            var csDef = new TestCrossSectionDefinition() { Name = "myCrossSectionDefinition" };
            csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, 20.0);
            Assert.That(csDef.Sections.Count, Is.EqualTo(1));

            var expectedMessage = string.Format("Could not add CrossSectionSection with duplicate name '{0}'", Section1Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, 13.0), expectedMessage);
            Assert.That(csDef.Sections.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenCrossSectionDefinition_WhenAddingMultipleSections_ThenSectionsAreAddedCorrectly()
        {
            var csDef = new TestCrossSectionDefinition();
            csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, 20.0);
            csDef.AddSection(new CrossSectionSectionType { Name = Section2Name }, 55.0);
            csDef.AddSection(new CrossSectionSectionType { Name = Section3Name }, 5.0);
            Assert.That(csDef.Sections.Count, Is.EqualTo(3));

            var section1 = csDef.Sections.FirstOrDefault(s => s.SectionType.Name == Section1Name);
            Assert.IsNotNull(section1);
            Assert.AreEqual(0.0, section1.MinY, double.Epsilon);
            Assert.AreEqual(20.0, section1.MaxY, double.Epsilon);

            var section2 = csDef.Sections.FirstOrDefault(s => s.SectionType.Name == Section2Name);
            Assert.IsNotNull(section2);
            Assert.AreEqual(20.0, section2.MinY, double.Epsilon);
            Assert.AreEqual(75.0, section2.MaxY, double.Epsilon);

            var section3 = csDef.Sections.FirstOrDefault(s => s.SectionType.Name == Section3Name);
            Assert.IsNotNull(section3);
            Assert.AreEqual(75.0, section3.MinY, double.Epsilon);
            Assert.AreEqual(80.0, section3.MaxY, double.Epsilon);
        }

        [Test]
        public void GivenCrossSectionDefinitionZw_WhenAddingSection_ThenSectionIsAddedCorrectly()
        {
            //    Explanation of cross section sections
            //       for CrossSectionDefinitionZW 
            //
            //    \                               /
            //     \                             /
            //      \                           /
            //       \                         /
            //        \_______________________/
            //      |         |   |   |         |
            //    -MaxY     -MinY 0  MinY     MaxY
            //       _________         _________
            //      | Section |       | Section |               

            var csDef = new CrossSectionDefinitionZW();
            
            csDef.AddSection(new CrossSectionSectionType { Name = Section1Name }, 20.0);
            csDef.AddSection(new CrossSectionSectionType { Name = Section2Name }, 55.0);
            csDef.AddSection(new CrossSectionSectionType { Name = Section3Name }, 5.0);
            Assert.That(csDef.Sections.Count, Is.EqualTo(3));

            var section1 = csDef.Sections.FirstOrDefault(s => s.SectionType.Name == Section1Name);
            Assert.IsNotNull(section1);
            Assert.AreEqual(0.0, section1.MinY, double.Epsilon);
            Assert.AreEqual(10.0, section1.MaxY, double.Epsilon);

            var section2 = csDef.Sections.FirstOrDefault(s => s.SectionType.Name == Section2Name);
            Assert.IsNotNull(section2);
            Assert.AreEqual(10.0, section2.MinY, double.Epsilon);
            Assert.AreEqual(37.5, section2.MaxY, double.Epsilon);

            var section3 = csDef.Sections.FirstOrDefault(s => s.SectionType.Name == Section3Name);
            Assert.IsNotNull(section3);
            Assert.AreEqual(37.5, section3.MinY, double.Epsilon);
            Assert.AreEqual(40.0, section3.MaxY, double.Epsilon);
        }

        #endregion


        #region AdjustSectionWidths

        private const string MainSectionName = "Main";
        private const string FP1SectionName = "FloodPlain1";
        private const string FP2SectionName = "FloodPlain2";
        private const string CustomSectionName = "Custom";

        [Test]
        public void TestAdjustSectionWidths_WithEqualFlowWidthAndSectionWidths_GivesNoLogMessageOrChangeToSectionWidths()
        {
            var crossSectionDefinition = new TestCrossSectionDefinition {Name = "TestCrossSectionDefinition"};

            TypeUtils.SetField(crossSectionDefinition, "profile", new List<Coordinate>()
            {
                new Coordinate(0, 0),
                new Coordinate(40, -10.0),
                new Coordinate(60, -10.0),
                new Coordinate(100, 0)
            });

            Assert.AreEqual(100.0, crossSectionDefinition.Width, double.Epsilon);

            var mainSection = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = MainSectionName },
                MinY = 0.0,
                MaxY = 80.0
            };

            var fp1Section = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = FP1SectionName },
                MinY = 80.0,
                MaxY = 95.0
            };

            var fp2Section = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = FP2SectionName },
                MinY = 95.0,
                MaxY = 100.0
            };

            crossSectionDefinition.Sections.Add(mainSection);
            crossSectionDefinition.Sections.Add(fp1Section);
            crossSectionDefinition.Sections.Add(fp2Section);
            
            // adjustSectionWidths & check results
            TestHelper.AssertLogMessagesCount(() => crossSectionDefinition.AdjustSectionWidths(), 0);

            Assert.AreEqual(100.0, crossSectionDefinition.Width, double.Epsilon,
                "CrossSectionDefinition Width should not have changed");

            Assert.AreEqual(80.0, mainSection.MaxY - mainSection.MinY, double.Epsilon,
               "Custom section width should not have changed");

            Assert.AreEqual(15.0, fp1Section.MaxY - fp1Section.MinY, double.Epsilon,
                "FloodPlain1 section width should not have changed");

            Assert.AreEqual(5.0, fp2Section.MaxY - fp2Section.MinY, double.Epsilon,
               "FloodPlain2 section width should not have changed");
            
            Assert.IsTrue(CheckCrossSectionDefinitionSectionsAreCorrectlyFitted(crossSectionDefinition));
        }

        [Test]
        public void TestAdjustSectionWidths_CrossSectionDefinitionYZ()
        {
            var crossSectionDefinition = CrossSectionDefinitionYZ.CreateDefault("DefaultCrossSectionDefinitionYZ");
            Assert.AreEqual(100.0, crossSectionDefinition.Width, double.Epsilon);

            var mainSection = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() {Name = MainSectionName},
                MinY = 10.0,
                MaxY = 50.0
            };

            var fp1Section = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = FP1SectionName },
                MinY = 60.0,
                MaxY = 70.0
            };

            var fp2Section = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = FP2SectionName },
                MinY = 85.0,
                MaxY = 90.0
            };

            crossSectionDefinition.Sections.Add(mainSection);
            crossSectionDefinition.Sections.Add(fp1Section);
            crossSectionDefinition.Sections.Add(fp2Section);
            
            // adjustSectionWidths & check results
            TestHelper.AssertAtLeastOneLogMessagesContains(() => crossSectionDefinition.AdjustSectionWidths(),
                "The Main section width of cross section DefaultCrossSectionDefinitionYZ has been changed from 40m to 85m");

            Assert.AreEqual(100.0, crossSectionDefinition.Width, double.Epsilon,
                "CrossSectionDefinition Width should not have changed");

            Assert.AreEqual(85.0, mainSection.MaxY - mainSection.MinY, double.Epsilon,
                "Main section width should have been expanded to fill the remaining width of the crossSection");

            Assert.AreEqual(10.0, fp1Section.MaxY - fp1Section.MinY, double.Epsilon,
                "FloodPlain1 section width should not have changed");

            Assert.AreEqual(5.0, fp2Section.MaxY - fp2Section.MinY, double.Epsilon,
                "FloodPlain2 section width should not have changed");
            
            Assert.IsTrue(CheckCrossSectionDefinitionSectionsAreCorrectlyFitted(crossSectionDefinition));
        }

        [Test]
        public void TestAdjustSectionWidths_CrossSectionDefinitionXYZ()
        {
            var crossSectionDefinition = new CrossSectionDefinitionXYZ("DefaultCrossSectionDefinitionXYZ")
            {
                Geometry = new LineString(new []
                {
                    new Coordinate(0, 0, 0),
                    new Coordinate(0, 40, -10.0),
                    new Coordinate(0, 60, -10.0),
                    new Coordinate(0, 100, 0)
                })
            };
            
            Assert.AreEqual(100.0, crossSectionDefinition.Width, double.Epsilon);

            var customSection = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = CustomSectionName },
                MinY = 10.0,
                MaxY = 20.0
            };

            var mainSection = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = MainSectionName },
                MinY = 25.0,
                MaxY = 75.0
            };

            var fp1Section = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = FP1SectionName },
                MinY = 85.0,
                MaxY = 90.0
            };

            crossSectionDefinition.Sections.Add(customSection);
            crossSectionDefinition.Sections.Add(mainSection);
            crossSectionDefinition.Sections.Add(fp1Section);
            
            // adjustSectionWidths & check results
            TestHelper.AssertAtLeastOneLogMessagesContains(() => crossSectionDefinition.AdjustSectionWidths(),
                "The Main section width of cross section DefaultCrossSectionDefinitionXYZ has been changed from 50m to 85m");

            Assert.AreEqual(100.0, crossSectionDefinition.Width, double.Epsilon,
                "CrossSectionDefinition Width should not have changed");

            Assert.AreEqual(10.0, customSection.MaxY - customSection.MinY, double.Epsilon,
               "Custom section width should not have changed");

            Assert.AreEqual(85.0, mainSection.MaxY - mainSection.MinY, double.Epsilon,
                "Main section width should have been expanded to fill the remaining width of the crossSection");

            Assert.AreEqual(5.0, fp1Section.MaxY - fp1Section.MinY, double.Epsilon,
                "FloodPlain1 section width should not have changed");
            
            Assert.IsTrue(CheckCrossSectionDefinitionSectionsAreCorrectlyFitted(crossSectionDefinition));
        }

        [Test]
        public void TestAdjustSectionWidths_CrossSectionDefinitionZW()
        {
            var crossSectionDefinition = CrossSectionDefinitionZW.CreateDefault();
            crossSectionDefinition.Name = "DefaultCrossSectionDefinitionZW";
            Assert.AreEqual(100.0, crossSectionDefinition.Width, double.Epsilon);

            var mainSection = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = MainSectionName },
                MinY = 5.0,
                MaxY = 25.0
            };

            var customSection = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = CustomSectionName },
                MinY = 35.0,
                MaxY = 45.0
            };

            crossSectionDefinition.Sections.Add(mainSection);
            crossSectionDefinition.Sections.Add(customSection);

            // adjustSectionWidths & check results
            TestHelper.AssertAtLeastOneLogMessagesContains(() => crossSectionDefinition.AdjustSectionWidths(),
                "The Main section width of cross section DefaultCrossSectionDefinitionZW has been changed from 40m to 80m");

            Assert.AreEqual(100.0, crossSectionDefinition.Width, double.Epsilon,
                "CrossSectionDefinition Width should not have changed");

            Assert.AreEqual(40.0, mainSection.MaxY - mainSection.MinY, double.Epsilon,
                "Main section width should have been expanded to fill the remaining width of the crossSection");

            Assert.AreEqual(10.0, customSection.MaxY - customSection.MinY, double.Epsilon,
               "Custom section width should not have changed");

            Assert.IsTrue(CheckCrossSectionDefinitionSectionsAreCorrectlyFitted(crossSectionDefinition));
        }

        [Test]
        public void TestAdjustSectionWidths_CrossSectionDefinitionStandard()
        {
            var crossSectionDefinition = CrossSectionDefinitionStandard.CreateDefault() as CrossSectionDefinition;
            Assert.NotNull(crossSectionDefinition);

            crossSectionDefinition.Name = "DefaultCrossSectionDefinitionStandard";
            Assert.AreEqual(1.0, crossSectionDefinition.Width, double.Epsilon);

            var customSection = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = CustomSectionName },
                MinY = -0.35,
                MaxY = 0.35
            };

            crossSectionDefinition.Sections.Add(customSection);

            // adjustSectionWidths & check results
            TestHelper.AssertAtLeastOneLogMessagesContains(() => crossSectionDefinition.AdjustSectionWidths(),
                "The Custom section width of cross section DefaultCrossSectionDefinitionStandard has been changed from 0.7m to 1m");

            Assert.AreEqual(1.0, crossSectionDefinition.Width, double.Epsilon,
                "CrossSectionDefinition Width should not have changed");

            Assert.AreEqual(1.0, customSection.MaxY - customSection.MinY, double.Epsilon,
                "Custom section width should have been expanded to fill the remaining width of the crossSection");

            Assert.IsTrue(CheckCrossSectionDefinitionSectionsAreCorrectlyFitted(crossSectionDefinition));
        }

        [Test]
        public void TestAdjustSectionWidths_UpdatesMainSectionEvenIfNotFirstSection()
        {
            var crossSectionDefinition = new TestCrossSectionDefinition();
            crossSectionDefinition.Name = "TestCrossSectionDefinition";

            TypeUtils.SetField(crossSectionDefinition, "profile", new List<Coordinate>()
            {
                new Coordinate(0, 0),
                new Coordinate(40, -10.0),
                new Coordinate(60, -10.0),
                new Coordinate(100, 0)
            });
            
            Assert.AreEqual(100.0, crossSectionDefinition.Width, double.Epsilon);

            var customSection = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = CustomSectionName },
                MinY = 10.0,
                MaxY = 20.0
            };

            var mainSection = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = MainSectionName },
                MinY = 25.0,
                MaxY = 75.0
            };
            
            crossSectionDefinition.Sections.Add(customSection);
            crossSectionDefinition.Sections.Add(mainSection);

            // adjustSectionWidths & check results
            TestHelper.AssertAtLeastOneLogMessagesContains(() => crossSectionDefinition.AdjustSectionWidths(),
                "The Main section width of cross section TestCrossSectionDefinition has been changed from 50m to 90m");

            Assert.AreEqual(100.0, crossSectionDefinition.Width, double.Epsilon,
                "CrossSectionDefinition Width should not have changed");

            Assert.AreEqual(10.0, customSection.MaxY - customSection.MinY, double.Epsilon,
               "Custom section width should not have changed");

            Assert.AreEqual(90.0, mainSection.MaxY - mainSection.MinY, double.Epsilon,
                "Main section width should have been expanded to fill the remaining width of the crossSection");

            Assert.IsTrue(CheckCrossSectionDefinitionSectionsAreCorrectlyFitted(crossSectionDefinition));
        }

        [Test]
        public void TestAdjustSectionWidths_UpdatesFirstSectionIfMainSectionDoesNotExist()
        {
            var crossSectionDefinition = new TestCrossSectionDefinition {Name = "TestCrossSectionDefinition"};

            TypeUtils.SetField(crossSectionDefinition, "profile", new List<Coordinate>()
            {
                new Coordinate(0, 0),
                new Coordinate(40, -10.0),
                new Coordinate(60, -10.0),
                new Coordinate(100, 0)
            });

            Assert.AreEqual(100.0, crossSectionDefinition.Width, double.Epsilon);

            var fp1Section = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = FP1SectionName },
                MinY = 10.0,
                MaxY = 20.0
            };

            var fp2Section = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = FP2SectionName },
                MinY = 20.0,
                MaxY = 80.0
            };

            var customSection = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = CustomSectionName },
                MinY = 80.0,
                MaxY = 90.0
            };

            crossSectionDefinition.Sections.Add(fp1Section);
            crossSectionDefinition.Sections.Add(fp2Section);
            crossSectionDefinition.Sections.Add(customSection);
            
            // adjustSectionWidths & check results
            TestHelper.AssertAtLeastOneLogMessagesContains(() => crossSectionDefinition.AdjustSectionWidths(),
                "The FloodPlain1 section width of cross section TestCrossSectionDefinition has been changed from 10m to 30m");

            Assert.AreEqual(100.0, crossSectionDefinition.Width, double.Epsilon,
                "CrossSectionDefinition Width should not have changed");

            Assert.AreEqual(30.0, fp1Section.MaxY - fp1Section.MinY, double.Epsilon,
                "FloodPlain1 section width should have been expanded to fill the remaining width of the crossSection");

            Assert.AreEqual(60.0, fp2Section.MaxY - fp2Section.MinY, double.Epsilon,
               "FloodPlain2 section width should not have changed");

            Assert.AreEqual(10.0, customSection.MaxY - customSection.MinY, double.Epsilon,
               "Custom section width should not have changed");

            Assert.IsTrue(CheckCrossSectionDefinitionSectionsAreCorrectlyFitted(crossSectionDefinition));
        }

        private bool CheckCrossSectionDefinitionSectionsAreCorrectlyFitted(CrossSectionDefinition crossSectionDefinition)
        {
            if (!crossSectionDefinition.Sections.Any()) return false;

            double expectedMinY;
            double endMaxY;
            crossSectionDefinition.GetCrossSectionDefinitionSectionBounds(out expectedMinY, out endMaxY);
            
            foreach (var crossSectionSection in crossSectionDefinition.Sections)
            {
                if (crossSectionSection.MinY < expectedMinY ||
                    crossSectionSection.MaxY > endMaxY)
                    return false;

                expectedMinY = crossSectionSection.MaxY;
            }
            
            return true;
        }

        #endregion

        [Test]
        public void TestGetCrossSectionDefinitionSectionBounds()
        {
            var csDefYZ = CrossSectionDefinitionYZ.CreateDefault();
            csDefYZ.YZDataTable.Rows[0].DeltaZStorage = 25.0;

            double minY;
            double maxY;
            csDefYZ.GetCrossSectionDefinitionSectionBounds(out minY, out maxY);

            Assert.AreEqual(0.0, minY);
            Assert.AreEqual(csDefYZ.Width, maxY);
        }

        [Test]
        public void TestGetCrossSectionDefinitionSectionBounds_WithStorageOnZWCrossSections()
        {
            var csDefZW = CrossSectionDefinitionZW.CreateDefault();
            csDefZW.ZWDataTable.Rows[0].StorageWidth = 25.0;
            
            double minY;
            double maxY;
            csDefZW.GetCrossSectionDefinitionSectionBounds(out minY, out maxY);

            Assert.AreEqual(0.0, minY);
            Assert.AreEqual((csDefZW.Width - 25.0) / 2, maxY);
        }
    }
}