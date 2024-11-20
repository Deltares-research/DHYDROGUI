using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekBridgeReaderTest
    {
        private static readonly SobekBridgeReader Reader;

        static SobekBridgeReaderTest()
        {
            Reader = new SobekBridgeReader();
        }

        [Test]
        public void Read()
        {
            Assert.AreEqual(12, Reader.Type);

            // Sobek online help has example with undocumented tb 1
            const string source = @"STDS id 'bridge1' nm 'bridge' ty 12 tb 3 si 'trapezoidal1' pw 0.5 vf 1.15 li 0.63 lo 0.64 dl 10.0 rl -1.0 rt 0 stds";

            var bridge = (SobekBridge)Reader.GetStructure(source);

            Assert.AreEqual(BridgeType.Abutment, bridge.BridgeType);
            Assert.AreEqual("trapezoidal1", bridge.CrossSectionId);
            Assert.AreEqual(0.5, bridge.TotalPillarWidth, 1.0e-6);
            Assert.AreEqual(1.15, bridge.FormFactor, 1.0e-6);
            Assert.AreEqual(0.63, bridge.InletLossCoefficient, 1.0e-6);
            Assert.AreEqual(0.64, bridge.OutletLossCoefficient, 1.0e-6);
            Assert.AreEqual(10.0, bridge.Length, 1.0e-6);
            Assert.AreEqual(-1.0, bridge.BedLevel, 1.0e-6);
            Assert.AreEqual(0,bridge.Direction);
        }

        [Test]
        public void ReadSobekReTestPillarBridge()
        {

            const string source = @"STDS id 'S_32070_1' nm '' ty 12 tb 2 si '-1' pw 1 vf 1.5 li 0.25 lo 0.25 dl 20 rl 0 stds";
            var bridge = (SobekBridge)Reader.GetStructure(source);
            Assert.AreEqual(BridgeType.PillarBridge, bridge.BridgeType);
            Assert.AreEqual(1.5, bridge.FormFactor);
            Assert.AreEqual(1.0, bridge.TotalPillarWidth);
        }
    }
}
