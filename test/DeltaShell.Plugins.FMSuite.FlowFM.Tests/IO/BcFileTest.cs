using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class BcFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTwoAstroWaterLevelBoundaryConditions()
        {
            var filePath = TestHelper.GetTestFilePath(@"BcFiles\TwoAstroWaterLevels.bc");
            var fileReader = new BcFile();
            var dataBlocks = fileReader.Read(filePath).ToList();
            Assert.AreEqual(2, dataBlocks.Count());

            var firstBlock = dataBlocks.First();
            Assert.AreEqual("pli1_0001", firstBlock.SupportPoint);
            Assert.AreEqual("astronomic", firstBlock.FunctionType);
            Assert.AreEqual(3, firstBlock.Quantities.Count);

            var quantity = firstBlock.Quantities[0];
            
            Assert.AreEqual("astronomic component", quantity.Quantity);
            Assert.AreEqual("-", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[] {"M2", "S2"}, quantity.Values);

            quantity = firstBlock.Quantities[1];

            Assert.AreEqual("waterlevelbnd amplitude", quantity.Quantity);
            Assert.AreEqual("m", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[] { "0.9", "0.95" }, quantity.Values);

            quantity = firstBlock.Quantities[2];

            Assert.AreEqual("waterlevelbnd phase" , quantity.Quantity);
            Assert.AreEqual("rad/deg/minutes", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[] { "10", "-7.5" }, quantity.Values);


            var secondBlock = dataBlocks.ElementAt(1);
            Assert.AreEqual("pli1_0002", secondBlock.SupportPoint);
            Assert.AreEqual("astronomic", secondBlock.FunctionType);
            Assert.AreEqual(3, secondBlock.Quantities.Count);

            quantity = secondBlock.Quantities[0];

            Assert.AreEqual("astronomic component", quantity.Quantity);
            Assert.AreEqual("-", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[] { "M2", "S2" }, quantity.Values);

            quantity = secondBlock.Quantities[1];

            Assert.AreEqual("waterlevelbnd amplitude", quantity.Quantity);
            Assert.AreEqual("m", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[] { "0.8", "1.1" }, quantity.Values);

            quantity = secondBlock.Quantities[2];

            Assert.AreEqual("waterlevelbnd phase", quantity.Quantity);
            Assert.AreEqual("rad/deg/minutes", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[] { "20", "-11.5" }, quantity.Values);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadWaterLevelAndSalinityLayersConditions()
        {
            var filePath = TestHelper.GetTestFilePath(@"BcFiles\WaterLevelAndSalinityLayers.bc");
            var fileReader = new BcFile();
            var dataBlocks = fileReader.Read(filePath).ToList();
            Assert.AreEqual(1, dataBlocks.Count());

            var firstBlock = dataBlocks.First();
            Assert.AreEqual("pli1_0001", firstBlock.SupportPoint);
            Assert.AreEqual("timeseries", firstBlock.FunctionType);
            Assert.AreEqual(4, firstBlock.Quantities.Count);

            var quantity = firstBlock.Quantities[0];

            Assert.AreEqual("time", quantity.Quantity);
            Assert.AreEqual("minutes since 2013-01-01", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[] { "0", "1440" }, quantity.Values);

            quantity = firstBlock.Quantities[1];

            Assert.AreEqual("waterlevelbnd", quantity.Quantity);
            Assert.AreEqual("m", quantity.Unit);
            Assert.AreEqual(null, quantity.VerticalPosition);
            Assert.AreEqual(new[] { "0.5", "0.65" }, quantity.Values);

            quantity = firstBlock.Quantities[2];

            Assert.AreEqual("salinitybnd", quantity.Quantity);
            Assert.AreEqual("ppt", quantity.Unit);
            Assert.AreEqual("1", quantity.VerticalPosition);
            Assert.AreEqual(new[] { "22", "30" }, quantity.Values);

            quantity = firstBlock.Quantities[3];

            Assert.AreEqual("salinitybnd", quantity.Quantity);
            Assert.AreEqual("ppt", quantity.Unit);
            Assert.AreEqual("2", quantity.VerticalPosition);
            Assert.AreEqual(new[] { "0", "0" }, quantity.Values);
        }
    }
}
