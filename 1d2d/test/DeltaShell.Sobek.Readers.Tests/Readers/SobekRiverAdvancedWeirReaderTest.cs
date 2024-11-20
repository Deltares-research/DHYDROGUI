using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekRiverAdvancedWeirReaderTest
    {
        [Test]
        public void Read()
        {
            var reader = new SobekRiverAdvancedWeirReader();
            Assert.AreEqual(1, reader.Type);

            string source = @"cl 5 sw 5 ni 2 ph 11 nh 11 pw 1 nw 1 pp 0.5 np 0.5 pa 0.6 na 0.7 ";

            var weir = (SobekRiverAdvancedWeir)reader.GetStructure(source);

            Assert.AreEqual(5.0f, weir.CrestLevel);
            Assert.AreEqual(5.0f, weir.SillWidth);
            Assert.AreEqual(2, weir.NumberOfPiers);
            Assert.AreEqual(11.0f, weir.PositiveUpstreamFaceHeight);
            Assert.AreEqual(11.0f, weir.NegativeUpstreamHeight);
            Assert.AreEqual(1.0f, weir.PositiveWeirDesignHead);
            Assert.AreEqual(1.0f, weir.NegativeWeirDesignHead);
            Assert.AreEqual(0.5f, weir.PositivePierContractionCoefficient);
            Assert.AreEqual(0.5f, weir.NegativePierContractionCoefficient);
            Assert.AreEqual(0.6f, weir.PositiveAbutmentContractionCoefficient);
            Assert.AreEqual(0.7f, weir.NegativeAbutmentContractionCoefficient);
        }
    }
}