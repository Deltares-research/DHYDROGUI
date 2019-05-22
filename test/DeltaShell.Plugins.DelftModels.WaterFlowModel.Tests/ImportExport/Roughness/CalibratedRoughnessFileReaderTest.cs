using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class CalibratedRoughnessFileReaderTest : RoughnessFileReaderTestHelper
    {
        [Test]
        public void TestRoughnessDataFileReader_With_Calibrated_RoughnessSectionFile()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(3);
            var crossSectionSectionType = new CrossSectionSectionType { Name = "Main" };
            var roughnessSection = new RoughnessSection(crossSectionSectionType, network);

            var roughnessFile = TestHelper.GetTestFilePath(@"FileReaders/roughness-Main.ini");

            //check original defaults:
            roughnessSection.SetDefaults(RoughnessType.DeBosAndBijkerk, 801.0d);
            Assert.That(roughnessSection.RoughnessNetworkCoverage.DefaultRoughnessType, Is.EqualTo(RoughnessType.DeBosAndBijkerk));
            Assert.That(roughnessSection.RoughnessNetworkCoverage.DefaultValue, Is.EqualTo(801.0).Within(0.0001));

            //check constant values:
            Assert.That(roughnessSection.RoughnessNetworkCoverage.Locations.Values.Count, Is.EqualTo(0));

            //no functions are set for the roughness section
            foreach (var branch in network.Branches)
            {
                Assert.That(() => roughnessSection.FunctionOfH(branch), Throws.Exception.TypeOf<KeyNotFoundException>());
                Assert.That(() => roughnessSection.FunctionOfQ(branch), Throws.Exception.TypeOf<KeyNotFoundException>());
            }

            new CalibratedRoughnessFileReader().ReadFile(roughnessFile, network, new[] { roughnessSection });
            CheckResults(roughnessSection, network);

            //re-read file & check to see if no duplicates are created
            new CalibratedRoughnessFileReader().ReadFile(roughnessFile, network, new[] { roughnessSection });
            CheckResults(roughnessSection, network);
        }
    }
}