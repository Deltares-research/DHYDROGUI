using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekOrificeReaderTest
    {
        private SobekOrificeReader reader;

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
            reader = new SobekOrificeReader();
        }
        [Test]
        public void Read()
        {
            Assert.AreEqual(7, reader.Type);

            string source = @"cl 0.7 cw 2 gh 0.9 mu 0.63 sc 1 rt 0";


            var orifice = (SobekOrifice) reader.GetStructure(source);

            Assert.AreEqual(0.7f, orifice.CrestLevel);
            Assert.AreEqual(2.0f, orifice.CrestWidth);
            Assert.AreEqual(0.9f, orifice.GateHeight);
            Assert.AreEqual(0.63f, orifice.ContractionCoefficient);
            Assert.AreEqual(1.0f, orifice.LateralContractionCoefficient);
            Assert.AreEqual(0, orifice.FlowDirection);
            Assert.AreEqual(0, orifice.MaximumFlowPos);
            Assert.AreEqual(0, orifice.MaximumFlowNeg);
            Assert.IsFalse(orifice.UseMaximumFlowNeg);
            Assert.IsFalse(orifice.UseMaximumFlowPos);
        }

        [Test]
        public void ReadWithDefinedMaximumFlow()
        {
            string source = @"cl 0.7 cw 2 gh 0.9 mu 0.63 sc 1 rt 0 mp 1 0.55 mn 0 0.2";
            var orifice = (SobekOrifice)reader.GetStructure(source);
            Assert.IsTrue(orifice.UseMaximumFlowPos);
            Assert.AreEqual(0.55d, orifice.MaximumFlowPos);
            Assert.IsFalse(orifice.UseMaximumFlowNeg);
            Assert.AreEqual(0.2d, orifice.MaximumFlowNeg);
        }
    }
}