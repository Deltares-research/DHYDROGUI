using System.IO;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.FileWriter
{
    [TestFixture]
    public class ManifestRetrieverTest
    {
        [Test]
        public void FixedResources_GetsTheCorrectResources()
        {
            // Setup
            var retriever = new ManifestRetriever();

            // Call
            string[] resourceNames = retriever.FixedResources.ToArray();

            // Assert
            Assert.That(resourceNames, Is.EquivalentTo(new[]
            {
                "Bergcoef.cap",
                "CropData.Dat",
                "CropOwData.Dat",
                "KasGebrData.Dat",
                "KasInitData.Dat",
                "KasKlasData.Dat",
                "SoilData.Dat",
                "DEFAULT.BUI",
                "EVAPOR.GEM",
                "EVAPOR.PLV",
                "ROOT_SIM.INP",
                "UNSA_SIM.INP",
                "SOBEK_3B.LNG"
            }));
        }

        [Test]
        public void GetFixedStream_GetsTheCorrectStream()
        {
            var retriever = new ManifestRetriever();
            using (Stream stream = retriever.GetFixedStream("EVAPOR.GEM"))
            {
                Assert.That(stream.Length, Is.EqualTo(6665));
            }
        }
    }
}