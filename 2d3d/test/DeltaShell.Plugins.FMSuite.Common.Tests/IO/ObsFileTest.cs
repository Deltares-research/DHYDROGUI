using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    public class ObsFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadGroupableFeatureObsFileAssignsGroupName()
        {
            var groupName = "ObsGroup1_obs.xyn";
            string filePath = TestHelper.GetTestFilePath(Path.Combine(@"HydroAreaCollection", groupName));
            Assert.IsTrue(File.Exists(filePath));
            filePath = TestHelper.CreateLocalCopy(filePath);
            try
            {
                var obsFile = new ObsFile<GroupableFeature2DPoint>();
                IList<GroupableFeature2DPoint> readObjects = obsFile.Read(filePath);
                List<IGrouping<string, GroupableFeature2DPoint>> groups = readObjects.GroupBy(g => g.GroupName).ToList();
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