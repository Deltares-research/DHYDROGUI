using System;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class AttributesFileDataTest
    {
        [Test]
        public void ConstructorTest()
        {
            // call
            var data = new AttributesFileData(3, 4);

            // assert
            Assert.AreEqual(12, data.IndexCount);
        }

        [Test]
        [TestCase(0)]
        [TestCase(-882)]
        public void ConstructWithInvalidNumberOfSegmentsPerLayer(int invalidNumberOfSegmentsPerLayer)
        {
            // call
            TestDelegate call = () => new AttributesFileData(invalidNumberOfSegmentsPerLayer, 2);

            // assert
            Assert.Throws<ArgumentOutOfRangeException>(call);
        }

        [Test]
        [TestCase(0)]
        [TestCase(-12)]
        public void ConstructWithInvalidNumberOfLayers(int invalidNumberOfSegmentsPerLayer)
        {
            // call
            TestDelegate call = () => new AttributesFileData(3, invalidNumberOfSegmentsPerLayer);

            // assert
            Assert.Throws<ArgumentOutOfRangeException>(call);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetSegmentActive(bool boolean)
        {
            // setup
            var data = new AttributesFileData(1, 1);

            // call
            data.SetSegmentActive(1, boolean);

            // assert
            Assert.AreEqual(boolean, data.IsSegmentActive(1));
        }

        [Test]
        public void SetSegmentActiveWithInvalidIndex()
        {
            // setup
            var data = new AttributesFileData(1, 1);

            // call
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => data.SetSegmentActive(2, true));

            // assert
            Assert.AreEqual("Segment index is out of range (count = 1).\r\nParameter name: segmentIndex\r\nActual value was 2.", exception.Message);
        }

        [Test]
        public void IsSegmentActiveWithInvalidIndex()
        {
            // setup
            var data = new AttributesFileData(1, 1);

            // call
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => data.IsSegmentActive(2));

            // assert
            Assert.AreEqual("Segment index is out of range (count = 1).\r\nParameter name: segmentIndex\r\nActual value was 2.", exception.Message);
        }
    }
}