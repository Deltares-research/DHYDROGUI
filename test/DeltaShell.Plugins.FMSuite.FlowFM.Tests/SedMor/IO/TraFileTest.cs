using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.SedMor.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.SedMor.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.SedMor.IO
{
    [TestFixture]
    public class TraFileTest
    {
        [Test]
        public void CheckNumberOfTransportFormulations()
        {
            Assert.AreEqual(12, TransportFormulation.AvailableTransportFormulations.Count());
            Assert.AreEqual("Van Rijn (1984)", TransportFormulation.AvailableTransportFormulations.Skip(5).First());
        }

        [Test]
        public void ReadTraFile()
        {
            var path = TestHelper.GetTestFilePath("sedmor\\files\\GaeumanEtAl.tra");

            var traFile = new TraFile();
            var traFormulation = traFile.Load(path);

            Assert.AreEqual("Gaeuman et al. Trinity River calibration", traFormulation.Name);
            Assert.AreEqual(2, traFormulation.Properties.Count);
            Assert.AreEqual(0.03, traFormulation.Properties["GaeuTrinityCalibrationCoeffTheta"].Value);
        }

        [Test]
        public void ReadOtherTraFile()
        {
            var path = TestHelper.GetTestFilePath("sedmor\\files\\Bijker.tra");

            var traFile = new TraFile();
            var traFormulation = traFile.Load(path);

            Assert.AreEqual("Bijker", traFormulation.Name);
            Assert.AreEqual(9, traFormulation.Properties.Count);
            Assert.AreEqual(0.016, traFormulation.Properties["BijkerSettlVelocity"].Value);
        }

        [Test]
        public void ReadWriteReadOtherTraFile()
        {
            var path = TestHelper.GetTestFilePath("sedmor\\files\\Bijker.tra");

            var traFile = new TraFile();
            var traFormulation = traFile.Load(path);

            Assert.AreEqual("Bijker", traFormulation.Name);
            Assert.AreEqual(9, traFormulation.Properties.Count);
            Assert.AreEqual(0.016, traFormulation.Properties["BijkerSettlVelocity"].Value);

            traFile.Save("newtra.tra", traFormulation);

            var tra2 = traFile.Load(path);

            Assert.AreEqual("Bijker", tra2.Name);
            Assert.AreEqual(9, tra2.Properties.Count);
            Assert.AreEqual(0.016, tra2.Properties["BijkerSettlVelocity"].Value);
        }
    }
}