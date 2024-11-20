using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekUniversalWeirReaderTest
    {
        [Test]
        public void Read()
        {
            var reader = new SobekUniversalWeirReader();
            Assert.AreEqual(11, reader.Type);

            string source = @"cl 1.1 si '1' ce 1.05 sv 0.667 rt 0";

            var weir = (SobekUniversalWeir) reader.GetStructure(source);

            Assert.AreEqual(1.1f, weir.CrestLevelShift);
            Assert.AreEqual("1", weir.CrossSectionId);
            Assert.AreEqual(1.05f, weir.DischargeCoefficient);
            Assert.AreEqual(0, weir.FlowDirection);
        }

        [Test]
        public void ReadOtherFormat()
        {
        
            var reader = new SobekUniversalWeirReader();
            Assert.AreEqual(11, reader.Type);

            string source = @"cl 0 si 'stuw_299_vughterstuw' ce 0.93 sv 0.667 rt 0    ";

            var weir = (SobekUniversalWeir) reader.GetStructure(source);

            Assert.AreEqual(0.0f, weir.CrestLevelShift);
            Assert.AreEqual("stuw_299_vughterstuw", weir.CrossSectionId);
            Assert.AreEqual(0.93f, weir.DischargeCoefficient);
            
        }

        [Test]
        public void ReadFormatWithNonStandardAscii()
        {

            var reader = new SobekUniversalWeirReader();
            Assert.AreEqual(11, reader.Type);

            string source = @"STDS id 'S_DoraBaltea_8' nm 'Mazzè' ty 11 cl 0 si 'strS_DoraBaltea_8' ce 1 sv 0.667 rt 0 stds ";

            var weir = (SobekUniversalWeir)reader.GetStructure(source);

            Assert.AreEqual(0.0f, weir.CrestLevelShift);
            Assert.AreEqual("strS_DoraBaltea_8", weir.CrossSectionId);
            Assert.AreEqual(1.0f, weir.DischargeCoefficient);
        }
        
    }
}