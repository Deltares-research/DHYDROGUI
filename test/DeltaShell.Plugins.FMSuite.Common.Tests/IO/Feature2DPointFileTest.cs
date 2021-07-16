using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    public class Feature2DPointFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadGroupableFeatureObsFileAssignsGroupName()
        {
            var groupName = "ObsGroup1_obs.xyn";
            var filePath = TestHelper.GetTestFilePath(Path.Combine(@"HydroAreaCollection", groupName));
            Assert.IsTrue(File.Exists(filePath));
            filePath = TestHelper.CreateLocalCopy(filePath);
            try
            {
                var obsFile = new Feature2DPointFile<GroupableFeature2DPoint>();
                var readObjects = obsFile.Read(filePath);
                var groups = readObjects.GroupBy(g => g.GroupName).ToList();
                Assert.That(groups.Count, Is.EqualTo(1));
                Assert.That(groups.First().Key, Is.EqualTo(filePath.Replace(@"\", "/")));
            }
            finally
            {
                FileUtils.DeleteIfExists(filePath);
            }
        }
    }
}