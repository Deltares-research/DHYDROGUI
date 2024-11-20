using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekCrossSectionsReaderTest
    {
        [Test]
        public void ReadRecord()
        {
            var source = "CRSN id 'c1' nm 'crossdef1' ci '10' lc 250.6 crsn";
            var crossSectionLocation = SobekCrossSectionsReader.GetCrossSectionLocation(source);
            Assert.AreEqual("c1", crossSectionLocation.ID);
            Assert.AreEqual("crossdef1", crossSectionLocation.Name);
            Assert.AreEqual("10", crossSectionLocation.BranchID);
            Assert.AreEqual(250.6, crossSectionLocation.Offset);

            source = "CRSN id 'c1' nm 'crossdef1' ci '10' lc 12E-02 crsn";
            crossSectionLocation = SobekCrossSectionsReader.GetCrossSectionLocation(source);
            Assert.AreEqual("c1", crossSectionLocation.ID);
            Assert.AreEqual("crossdef1", crossSectionLocation.Name);
            Assert.AreEqual("10", crossSectionLocation.BranchID);
            Assert.AreEqual(12e-2, crossSectionLocation.Offset);
        }
    }
}