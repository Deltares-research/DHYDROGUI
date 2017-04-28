using System;
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
        public void ReadAllWhileFileDoesNotExist()
        {
            // setup
            var nonexistentAttributesFile = new FileInfo("I do not exist");
            var reader = new AttributesFileReader(nonexistentAttributesFile);

            // call
            TestDelegate call = () => reader.ReadAll(1, 2);

            // assert
            var exception = Assert.Throws<InvalidOperationException>(call);
            Assert.AreEqual(string.Format("Cannot find attributes file ({0}).", nonexistentAttributesFile.FullName), 
                exception.Message);
        }

        [Test]
        public void ReadAllFromSquareModel()
        {
            // setup
            var filePath = Path.Combine(TestHelper.GetDataDir(), "IO", "square", "square.atr");
            var atrFile = new FileInfo(filePath);
            var reader = new AttributesFileReader(atrFile);

            // call
            var attributesFileData = reader.ReadAll(2500, 1);

            // assert
            Assert.AreEqual(2500, attributesFileData.IndexCount);
            var t = Enumerable.Range(1, 2500);
            foreach (var segmentIndex in t)
            {
                Assert.IsTrue(attributesFileData.IsSegmentActive(segmentIndex));
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void ReadAllFromRealModelPerformance()
        {
            // setup
            var filePath = Path.Combine(TestHelper.GetDataDir(), "IO", "real", "uni3d.atr");
            var atrFile = new FileInfo(filePath);
            var reader = new AttributesFileReader(atrFile);

            AttributesFileData data = null;

            // call & assert
            TestHelper.AssertIsFasterThan(200, () =>
            {
                data = reader.ReadAll(63814, 7);
            });
            Assert.AreEqual(63814 * 7, data.IndexCount);
        }
        
        [Test]
        public void ReadAllFromRandom3Layer5SegmentsPerLayerFile()
        {
            // setup
            var filePath = Path.Combine(TestHelper.GetDataDir(), "IO", "attribute files", "random_3x5.atr");
            var atrFile = new FileInfo(filePath);
            var reader = new AttributesFileReader(atrFile);

            // call
            var attributesFileData = reader.ReadAll(5, 3);

            // assert
            Assert.AreEqual(15, attributesFileData.IndexCount);
            var t = Enumerable.Range(1, 15);
            var expectedArray = new[]
            {
                true, false, false, true, false,
                false, true, true, true, true,
                false, true, false, true, false
            };
            foreach (var segmentIndex in t)
            {
                Assert.AreEqual(expectedArray[segmentIndex-1], attributesFileData.IsSegmentActive(segmentIndex));
            }
        }

        [Test]
        public void ReadAllFromRandom2Layer2SegmentsPerLayerFileWithTopBottomFirst()
        {
            // setup
            var filePath = Path.Combine(TestHelper.GetDataDir(), "IO", "attribute files", "random_2x2_WithBottomTopFirst.atr");
            var atrFile = new FileInfo(filePath);
            var reader = new AttributesFileReader(atrFile);

            // call
            var attributesFileData = reader.ReadAll(2, 2);

            // assert
            Assert.AreEqual(4, attributesFileData.IndexCount);
            var t = Enumerable.Range(1, 4);
            var expectedArray = new[]
            {
                false, true,
                true, false
            };

            foreach (var segmentIndex in t)
            {
                Assert.AreEqual(expectedArray[segmentIndex-1], attributesFileData.IsSegmentActive(segmentIndex));
            }
        }

        [Test]
        public void ReadAllFromRandom2Layer2SegmentsPerLayerFileWithOnlyTopBottomFirst()
        {
            // setup
            var filePath = Path.Combine(TestHelper.GetDataDir(), "IO", "attribute files", "random_2x2_OnlyBottomTopFirst.atr");
            var atrFile = new FileInfo(filePath);
            var reader = new AttributesFileReader(atrFile);

            // call
            TestDelegate call = () => reader.ReadAll(2, 2);

            // assert
            var exception = Assert.Throws<FormatException>(call);
            Assert.AreEqual(string.Format("Attributes file ({0}) does not contain data block for enabled state of segments.", filePath), 
                exception.Message);
        }
    }
}