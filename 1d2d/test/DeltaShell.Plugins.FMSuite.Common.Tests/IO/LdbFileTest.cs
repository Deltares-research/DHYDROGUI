using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    public class LdbFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadGroupableFeatureLdbFileAssignsGroupName()
        {
            var groupName = "LdbGroup1.ldb";
            var filePath = TestHelper.GetTestFilePath(Path.Combine(@"HydroAreaCollection", groupName));
            Assert.IsTrue(File.Exists(filePath));
            filePath = TestHelper.CreateLocalCopy(filePath);
            try
            {
                var ldbFile = new LdbFile();
                var readObjects = ldbFile.Read(filePath);
                var groups = readObjects.GroupBy(g => g.GroupName).ToList();
                Assert.That(groups.Count, Is.EqualTo(1));
                Assert.IsNull(groups.First().Key);
            }
            finally
            {
                FileUtils.DeleteIfExists(filePath);
            }
        }
    }
}