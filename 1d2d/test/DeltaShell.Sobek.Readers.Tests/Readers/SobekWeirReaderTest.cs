using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekWeirReaderTest
    {
        [Test]
        public void Read()
        {
            var reader = new SobekWeirReader();
            Assert.AreEqual(6,reader.Type);

            string source = "cl 0.7 cw 2 ce 1 sc 1 rt 0  ";
            var weir = (SobekWeir) reader.GetStructure(source);

            Assert.AreEqual(0.7f, weir.CrestLevel);
            Assert.AreEqual(2.0f, weir.CrestWidth);
            Assert.AreEqual(1.0f, weir.DischargeCoefficient);
            Assert.AreEqual(1.0f, weir.LateralContractionCoefficient );
            Assert.AreEqual(0, weir.FlowDirection);
        }
    }
}