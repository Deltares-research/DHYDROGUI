using System.Collections.Generic;
using System.Data;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class RoughnessSectionToDataTableConverterTest
    {
        [Test]
        public void OrderInTableIsBranchThenSectionTypeAndThenChainage()
        {
            var network = new HydroNetwork();
            //according to spec the csv should be sorted like this branch,sectiontype,chainage,offset...
            var channel1 = new Channel {Network = network, Name = "ch1"};
            var channel2 = new Channel {Network = network, Name = "ch2"};

            network.Branches.AddRange(new[] {channel1, channel2});

            var mainSectionType = new CrossSectionSectionType {Name = "Main"};
            var fp1SectionType = new CrossSectionSectionType {Name = "FP1"};

            var roughnessSectionMain = new RoughnessSection(mainSectionType, network);
            var roughnessSectionFp1 = new RoughnessSection(fp1SectionType, network);
            var sections = new[]
                               {
                                   roughnessSectionMain, roughnessSectionFp1
                               };

            //some constants on both branches
            roughnessSectionMain.RoughnessNetworkCoverage[new NetworkLocation(channel1, 10)] = new[] {1.0d, 1};
            roughnessSectionMain.RoughnessNetworkCoverage[new NetworkLocation(channel1, 20)] = new[] {2.0d, 1};
            roughnessSectionMain.RoughnessNetworkCoverage[new NetworkLocation(channel2, 30)] = new[] {3.0d, 1};
            roughnessSectionMain.RoughnessNetworkCoverage[new NetworkLocation(channel2, 40)] = new[] {4.0d, 1};

            //some constants on both branches
            roughnessSectionFp1.RoughnessNetworkCoverage[new NetworkLocation(channel1, 50)] = new[] {5.0d, 1};
            roughnessSectionFp1.RoughnessNetworkCoverage[new NetworkLocation(channel1, 60)] = new[] {6.0d, 1};
            roughnessSectionFp1.RoughnessNetworkCoverage[new NetworkLocation(channel2, 70)] = new[] {7.0d, 1};
            roughnessSectionFp1.RoughnessNetworkCoverage[new NetworkLocation(channel2, 80)] = new[] {8.0d, 1};

            var converter = new RoughnessSectionToDataTableConverter();
            var table = converter.GetDataTable(sections);

            //TODO: check table contents
            Assert.AreEqual(8, table.Rows.Count);
            //select branchnames 
            var branchNames = new List<string>();
            var offsets = new List<string>();
            var sectionTypes = new List<string>();

            foreach (DataRow row in table.Rows)
            {
                branchNames.Add(row[0].ToString());
                offsets.Add(row[1].ToString());
                sectionTypes.Add(row[3].ToString());
            }
            //table.Rows.
            Assert.AreEqual(new[]{"ch1","ch1","ch1","ch1","ch2","ch2","ch2","ch2"},branchNames);
            Assert.AreEqual(new[] { "Main", "Main", "FP1", "FP1", "Main", "Main", "FP1", "FP1" }, sectionTypes);
            Assert.AreEqual(new[] { "10", "20", "50", "60", "30", "40", "70", "80" }, offsets);

        }


    }
}