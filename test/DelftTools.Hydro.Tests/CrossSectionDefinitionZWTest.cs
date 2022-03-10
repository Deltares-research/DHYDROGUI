using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CrossSectionDefinitionZWTest
    {
        [Test]
        public void EmptyProfiles()
        {
            var crossSection = new CrossSectionDefinitionZW();

            var profile = crossSection.GetProfile();
            var flowProfile = crossSection.FlowProfile;

            Assert.AreEqual(0, profile.Count());
            Assert.AreEqual(0, flowProfile.Count());
        }

        [Test]
        public void CheckEvents()
        {
            int callCount = 0;
            var crossSection = new CrossSectionDefinitionZW();

            ((INotifyPropertyChanged)crossSection).PropertyChanged += (s, e) => callCount++;

            crossSection.ZWDataTable.AddCrossSectionZWRow(0.0, 0.0, 0.0);

            Assert.Greater(callCount, 0);
        }

        [Test]
        public void LevelShiftDoesNotCauseUniqueException()
        {
            var crossSection = new CrossSectionDefinitionZW();
            //simple V profile
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(6, 50, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);

            //level shift it by -4...this makes two rows 6 causing a unique constraint exception
         
            crossSection.ShiftLevel(-4);
        }

        [Test]
        public void ZConstraintWorksAfterLevelShift()
        {
            var crossSection = new CrossSectionDefinitionZW();
            //simple V profile
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(6, 50, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);

            //level shift it by 0
            crossSection.ShiftLevel(0);

            //change the first row to 6..this should cause a constraint exception
            var error = Assert.Throws<ArgumentException>(()=> crossSection.ZWDataTable[0].Z = 6);
            Assert.AreEqual("Z must be unique.", error.Message);
        }

        [Test]
        public void TestProfileMatchesDataTable()
        {
            var crossSection = new CrossSectionDefinitionZW();
            //simple V profile
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(6, 50, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);

            var profileY = new double[] { -50, -25, 0, 25, 50 };
            var profileZ = new double[] {10, 6, 0, 6, 10};

            var flowProfileY = new double[] { -30, -5, 0, 5, 30 };
            var flowProfileZ = new double[] { 10, 6, 0, 6, 10 };

            Assert.AreEqual(profileY, crossSection.GetProfile().Select(c => c.X).ToArray());
            Assert.AreEqual(profileZ, crossSection.GetProfile().Select(c => c.Y).ToArray());
            Assert.AreEqual(flowProfileY, crossSection.FlowProfile.Select(c => c.X).ToArray());
            Assert.AreEqual(flowProfileZ, crossSection.FlowProfile.Select(c => c.Y).ToArray());
        }
        
        [Test]
        public void StorageWidthMustBeLessThanNormalWidthAdd()
        {
            var crossSection = new CrossSectionDefinitionZW();

            var error = Assert.Throws<ArgumentException>(() => crossSection.ZWDataTable.AddCrossSectionZWRow(20, 200, 400));
            Assert.AreEqual("Storage Width cannot exceed Total Width.", error.Message);
        }

        [Test]
        public void StorageWidthMustBeLessThanNormalWidthEdit()
        {
            var crossSection = new CrossSectionDefinitionZW();

            //simple \_/ profile
            crossSection.ZWDataTable.AddCrossSectionZWRow(20, 200, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(15, 150, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);

            var error = Assert.Throws<ArgumentException>(() => crossSection.ZWDataTable[1].StorageWidth = 300.0);
            Assert.AreEqual("Storage Width cannot exceed Total Width.", error.Message);
        }

        [Test]
        public void SetReferenceLevelHeightWidthWidthTest()
        {
            var crossSection = new CrossSectionDefinitionZW();
            crossSection.SetWithHfswData(new[]
                                             {
                                                 new HeightFlowStorageWidth(0, 10.0, 10.0),
                                                 new HeightFlowStorageWidth(10, 100.0, 100.0)
                                             });


            Assert.AreEqual(0.0, crossSection.LowestPoint, 1.0e-6);
            Assert.AreEqual(10.0, crossSection.HighestPoint, 1.0e-6);

            crossSection.ShiftLevel(111);

            Assert.AreEqual(121.0, crossSection.ZWDataTable[0].Z, 1.0e-6);
            Assert.AreEqual(111.0, crossSection.ZWDataTable[1].Z, 1.0e-6);

            Assert.AreEqual(111.0, crossSection.LowestPoint, 1.0e-6);
            Assert.AreEqual(121.0, crossSection.HighestPoint, 1.0e-6);
        }

        [Test]
        public void Clone()
        {
            var crossSection = new CrossSectionDefinitionZW
            {
                SummerDike = new SummerDike
                {
                    CrestLevel = 1,
                    FloodSurface = 2,
                    TotalSurface = 3,
                    FloodPlainLevel = 4
                }
            };
            crossSection.ZWDataTable.AddCrossSectionZWRow(4, 5, 2);

            var clone = (CrossSectionDefinitionZW)crossSection.Clone();

            ReflectionTestHelper.AssertPublicPropertiesAreEqual(crossSection.SummerDike, clone.SummerDike);
            Assert.AreNotSame(crossSection.SummerDike, clone.SummerDike);

            Assert.IsTrue(crossSection.ZWDataTable.ContentEquals(clone.ZWDataTable));
        }

        [Test]
        public void CopyFrom()
        {
            var crossSection = new CrossSectionDefinitionZW
            {
                Thalweg = 5.0,
                SummerDike = new SummerDike
                                {
                                    CrestLevel = 1,
                                    FloodSurface = 2,
                                    TotalSurface = 3,
                                    FloodPlainLevel = 4
                                }
            };
            crossSection.ZWDataTable.AddCrossSectionZWRow(4, 5, 2);

            var copyFrom = new CrossSectionDefinitionZW
            {
                Thalweg = 1.0,
                SummerDike = new SummerDike
                {
                    CrestLevel = 4,
                    FloodSurface = 3,
                    TotalSurface = 2,
                    FloodPlainLevel = 1
                }
            };

            copyFrom.CopyFrom(crossSection);

            Assert.AreEqual(crossSection.Thalweg, copyFrom.Thalweg);
            ReflectionTestHelper.AssertPublicPropertiesAreEqual(crossSection.SummerDike, copyFrom.SummerDike);
            Assert.AreNotSame(crossSection.SummerDike, copyFrom.SummerDike);

            Assert.IsTrue(crossSection.ZWDataTable.ContentEquals(copyFrom.ZWDataTable));
        }

        [Test]
        public void RemoveInvalidSections()
        {
            var mainType = new CrossSectionSectionType
                               {
                                   Name = "Main"
                               };

            var crossSection = new CrossSectionDefinitionZW();
            crossSection.Sections.Add(new CrossSectionSection {SectionType = mainType});

            Assert.AreEqual(1,crossSection.Sections.Count);

            //now rename the type and call 
            mainType.Name = "newName";
            crossSection.RemoveInvalidSections();

            Assert.AreEqual(0,crossSection.Sections.Count);
        }

        [Test]
        public void TestValidateCellValue()
        {
            var crossSection = new CrossSectionDefinitionZW();
            //simple V profile
            crossSection.ZWDataTable.AddCrossSectionZWRow(10, 100, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(6, 50, 40);
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);

            // Setting Z:
            var validationResult = crossSection.ValidateCellValue(0, 0, 0.0);
            Assert.IsFalse(validationResult.Second,
                "Z=0 already defined");
            Assert.AreEqual("Z must be unique.", validationResult.First);

            // NaN not allowed:
            validationResult = crossSection.ValidateCellValue(0, 0, double.NaN);
            Assert.IsFalse(validationResult.Second,
                "NaN values should not accepted");
            Assert.AreEqual("Value must be a number.", validationResult.First);

            // Values using strings:
            validationResult = crossSection.ValidateCellValue(0, 0, "1.0");
            Assert.IsTrue(validationResult.Second,
                "Using strings representing numbers should be accepted");
            Assert.AreEqual("", validationResult.First);

            // Non-number strings:
            validationResult = crossSection.ValidateCellValue(0, 0, "test");
            Assert.IsFalse(validationResult.Second,
                "Using strings representing non-numbers should not be accepted");
            Assert.AreEqual("Value must be a number.", validationResult.First);

            // Setting Width (valid):
            validationResult = crossSection.ValidateCellValue(0, 1, 0.0);
            Assert.IsTrue(validationResult.Second);
            Assert.AreEqual("", validationResult.First);

            // Setting Width (invalid):
            validationResult = crossSection.ValidateCellValue(0, 1, -1.0);
            Assert.IsFalse(validationResult.Second);
            Assert.AreEqual("Total Width cannot be negative.", validationResult.First);

            // Setting Storage Width (valid):
            validationResult = crossSection.ValidateCellValue(0, 2, 1.0);
            Assert.IsTrue(validationResult.Second);
            Assert.AreEqual("", validationResult.First);

            // Setting Storage Width (invalid - negative):
            validationResult = crossSection.ValidateCellValue(0, 2, -1.0);
            Assert.IsFalse(validationResult.Second);
            Assert.AreEqual("Storage Width cannot be negative.", validationResult.First);

            // Setting Storage Width (invalid - too large):
            validationResult = crossSection.ValidateCellValue(1, 2, 51.0);
            Assert.IsFalse(validationResult.Second);
            Assert.AreEqual("Storage Width cannot exceed Total Width.", validationResult.First);
        }

    }
}