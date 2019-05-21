using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class PiProfilesExporterTest : FewsAdapterTestBase
    {
        private const string OutputFolder = "XmlOutput";

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)] // todo: check expected content
        public void Export_NetworkCoverage_XmlFileCreatedAndContentsAreValid()
        {
            FileUtils.CreateDirectoryIfNotExists(OutputFolder, true);
            var path = Path.Combine(OutputFolder, TestHelper.GetCurrentMethodName() + ".xml");
            Network network = CreateNetwork();

            var nc = new NetworkCoverage("WaterLevel", true) {Network = network};
            var branch1 = network.Branches[0];
            var branch2 = network.Branches[1];

            var time0 = new DateTime(1, 1, 1); // take fixed date so we can compare output with expected file
            var time1 = time0.AddDays(1);
            var loc11 = new NetworkLocation(branch1, 0);
            var loc12 = new NetworkLocation(branch1, 50.0);
            var loc21 = new NetworkLocation(branch2, 0);
            var loc22 = new NetworkLocation(branch2, 50.0);

            nc[time0, loc11] = 1.1;
            nc[time0, loc12] = 2.2;
            nc[time0, loc21] = 3.3;
            nc[time0, loc22] = 4.4;
            nc[time1, loc11] = 5.5;
            nc[time1, loc12] = 6.6;
            nc[time1, loc21] = 7.7;
            nc[time1, loc22] = 8.8;

            var exporter = new PiProfilesExporter();
            exporter.Export(nc, path);
            var actual =  File.ReadLines(path).ToList();

            string file = TestHelper.GetTestDataDirectory() + @"pi_multipleprofiles.xml";
            var expected = File.ReadLines(file).ToList();

            string errors = "";
            for (int index = 0; index < expected.Count; index++)
            {
                if (!expected[index].Equals(actual[index]))
                {
                    errors += actual[index] + "\n";
                }
            }
            Assert.IsEmpty(errors, "differences in " + path);
        }
    }
}
