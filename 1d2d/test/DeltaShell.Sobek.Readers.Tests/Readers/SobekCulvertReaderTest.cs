using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekCulvertReaderTest
    {
        private static readonly SobekCulvertReader Reader;

        static SobekCulvertReaderTest()
        {
            Reader = new SobekCulvertReader();
        }

        [Test]
        public void Read()
        {
            Assert.AreEqual(10, Reader.Type);

            const string source = @"tc 1 ll 5 rl 10 dl 10 si '1' li 0.7 lo 1 ov 0 tv 0 rt 1";

            var culvert = (SobekCulvert) Reader.GetStructure(source);

            Assert.AreEqual(1, (int) culvert.CulvertType);
            Assert.AreEqual(5.0, culvert.BedLevelLeft);
            Assert.AreEqual(10.0, culvert.BedLevelRight);
            Assert.AreEqual(10.0, culvert.Length);
            Assert.AreEqual("1", culvert.CrossSectionId);
            Assert.AreEqual(0.7f, culvert.InletLossCoefficient);
            Assert.AreEqual(1.0, culvert.OutletLossCoefficient);
            Assert.AreEqual(0, culvert.ValveInitialOpeningLevel);
            Assert.AreEqual(0, culvert.UseTableOffLossCoefficient);
            Assert.AreEqual(1, culvert.Direction);
        }

        [Test]
        public void ReadWithTable()
        {
            Assert.AreEqual(10, Reader.Type);

            const string source = @"tc 1 ll 0 rl 0 dl 10 si '1' li 0.7 lo 1 ov 2 tv 1 '5' rt 0";

            var culvert = (SobekCulvert) Reader.GetStructure(source);

            //only test the table stuff..the rest is done in Read()
            Assert.AreEqual(1, culvert.UseTableOffLossCoefficient);
            Assert.AreEqual("5", culvert.TableOfLossCoefficientId);
        }
        
        [Test]
        public void MixedSequence()
        {
            const string source = @"tc 1 dl 7 ll -1.54 rl -1.57 si '2' li 0.5 lo 1 lb 0 tv 0 '' ov 0 rt 0";

            var culvert = (SobekCulvert)Reader.GetStructure(source);

            Assert.AreEqual(7.0f, culvert.Length);
            Assert.AreEqual(-1.57f, culvert.BedLevelRight);
        }

    }
}
