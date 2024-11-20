using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects
{
    [TestFixture]
    public class FunctionFromHydroDynamicsTest
    {
        [Test]
        public void FilePathTest()
        {
            // setup
            var function = new FunctionFromHydroDynamics();

            const string filePath = "Some filepath";

            // call
            function.FilePath = filePath;

            // assert
            Assert.AreEqual(filePath, function.FilePath);
        }
    }
}