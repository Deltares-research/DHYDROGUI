using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class AttributesFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAllWhileFileDoesNotExist()
        {
            // setup
            var nonexistentAttributesFile = new FileInfo("I do not exist");

            // call
            TestDelegate call = () => AttributesFileReader.ReadAll(1, 2, nonexistentAttributesFile);

            // assert
            var exception = Assert.Throws<InvalidOperationException>(call);
            Assert.AreEqual($"Cannot find attributes file ({nonexistentAttributesFile.FullName}).",
                            exception.Message);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAllFromSquareModel()
        {
            // setup
            string filePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "square", "square.atr");
            var atrFile = new FileInfo(filePath);

            // call
            AttributesFileData attributesFileData = AttributesFileReader.ReadAll(2500, 1, atrFile);

            // assert
            Assert.AreEqual(2500, attributesFileData.IndexCount);
            IEnumerable<int> t = Enumerable.Range(1, 2500);
            foreach (int segmentIndex in t)
            {
                Assert.IsTrue(attributesFileData.IsSegmentActive(segmentIndex));
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void ReadAllFromRealModelPerformance()
        {
            // setup
            string filePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "real", "uni3d.atr");
            var atrFile = new FileInfo(filePath);

            AttributesFileData data = null;

            // call & assert
            TestHelper.AssertIsFasterThan(450, () => { data = AttributesFileReader.ReadAll(63814, 7, atrFile); });
            Assert.AreEqual(63814 * 7, data.IndexCount);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAllFromRandom3Layer5SegmentsPerLayerFile()
        {
            // setup
            string filePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "attribute files", "random_3x5.atr");
            var atrFile = new FileInfo(filePath);

            // call
            AttributesFileData attributesFileData = AttributesFileReader.ReadAll(5, 3, atrFile);

            // assert
            Assert.AreEqual(15, attributesFileData.IndexCount);
            IEnumerable<int> t = Enumerable.Range(1, 15);
            var expectedArray = new[]
            {
                true,
                false,
                false,
                true,
                false,
                false,
                true,
                true,
                true,
                true,
                false,
                true,
                false,
                true,
                false
            };
            foreach (int segmentIndex in t)
            {
                Assert.AreEqual(expectedArray[segmentIndex - 1], attributesFileData.IsSegmentActive(segmentIndex));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAllFromRandom2Layer2SegmentsPerLayerFileWithTopBottomFirst()
        {
            // setup
            string filePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "attribute files", "random_2x2_WithBottomTopFirst.atr");
            var atrFile = new FileInfo(filePath);

            // call
            AttributesFileData attributesFileData = AttributesFileReader.ReadAll(2, 2, atrFile);

            // assert
            Assert.AreEqual(4, attributesFileData.IndexCount);
            IEnumerable<int> t = Enumerable.Range(1, 4);
            var expectedArray = new[]
            {
                false,
                true,
                true,
                false
            };

            foreach (int segmentIndex in t)
            {
                Assert.AreEqual(expectedArray[segmentIndex - 1], attributesFileData.IsSegmentActive(segmentIndex));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAllFromRandom2Layer2SegmentsPerLayerFileWithOnlyTopBottomFirst()
        {
            // setup
            string filePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "attribute files", "random_2x2_OnlyBottomTopFirst.atr");
            var atrFile = new FileInfo(filePath);

            // call
            TestDelegate call = () => AttributesFileReader.ReadAll(2, 2, atrFile);

            // assert
            var exception = Assert.Throws<FormatException>(call);
            Assert.AreEqual($"Attributes file ({filePath}) does not contain data block for enabled state of segments.",
                            exception.Message);
        }
    }
}