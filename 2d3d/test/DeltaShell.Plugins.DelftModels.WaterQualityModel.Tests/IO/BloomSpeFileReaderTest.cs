using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class BloomSpeFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadSmallSpeFile_AssertAll()
        {
            string dataDir = TestHelper.GetTestDataDirectory();
            string filePath = Path.Combine(dataDir, "IO", "spe", "small.spe");

            BloomInfo result = BloomSpeFileReader.Read(filePath);

            string[] expected = new[]
            {
                "extvlfdi_e",
                "extvlffl_e",
                "extvlgre_e",
                "extvlaph_n",
                "extvlaph_p",
                "extvlapf_e",
                "extvlapf_n",
                "extvlblu_n",
                "extvlblu_p",
                "extvlmdi_e",
                "extvlmdi_n",
                "extvlmdi_p",
                "extvlmfl_e",
                "extvldim_m",
                "extvlulf_n",
                "extvlulf_p",
                "extvlnod_e",

                "extuvfdi_e",
                "extuvffl_e",
                "extuvgre_e",
                "extuvaph_n",
                "extuvaph_p",
                "extuvapf_e",
                "extuvapf_n",
                "extuvblu_n",
                "extuvblu_p",
                "extuvmdi_e",
                "extuvmdi_n",
                "extuvmdi_p",
                "extuvmfl_e",
                "extuvdim_m",
                "extuvulf_n",
                "extuvulf_p",
                "extuvnod_e",

                "ncrfdi_e",
                "ncrffl_e",
                "ncrgre_e",
                "ncraph_n",
                "ncraph_p",
                "ncrapf_e",
                "ncrapf_n",
                "ncrblu_n",
                "ncrblu_p",
                "ncrmdi_e",
                "ncrmdi_n",
                "ncrmdi_p",
                "ncrmfl_e",
                "ncrdim_m",
                "ncrulf_n",
                "ncrulf_p",
                "ncrnod_e"
            };

            var expectedDescriptions = new[]
            {
                "Fresh DIATOMS energy type",
                "Fresh FLAGELAT energy type",
                "GREENS energy type",
                "APHANIZO nitrogen type",
                "APHANIZO phosphorus type",
                "APHANFIX energy type",
                "APHANFIX nitrogen type",
                "BLUEGRN nitrogen type",
                "BLUEGRN phosphorus type",
                "Marine DIATOMS energy type",
                "Marine DIATOMS nitrogen type",
                "Marine DIATOMS phosphorus type",
                "Marine FLAGELAT energy type",
                "Mixotrophic DINOFLAG type",
                "Ulva attached nitrogen type",
                "Ulva attached phosphorus type",
                "NODULARIA energy type"
            };

            CollectionAssert.AreEquivalent(expected, result.AllParameters);
            CollectionAssert.AreEquivalent(expectedDescriptions, result.Descriptions);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadLargeSpeFile_AssertHeuristicsOnly()
        {
            string dataDir = TestHelper.GetTestDataDirectory();
            string filePath = Path.Combine(dataDir, "IO", "spe", "bloom.spe");

            BloomInfo result = BloomSpeFileReader.Read(filePath);

            // There should be 28*50=1400 variables
            Assert.AreEqual(1400, result.AllParameters.Count());

            // There should be 50 that start with SCR
            Assert.AreEqual(50, result.AllParameters.Count(p => p.StartsWith("scr")));

            // All values should be unique
            Assert.AreEqual(result.AllParameters, result.AllParameters.Distinct());

            Assert.AreEqual(result.Descriptions.Count(), result.Korts.Count());
        }
    }
}