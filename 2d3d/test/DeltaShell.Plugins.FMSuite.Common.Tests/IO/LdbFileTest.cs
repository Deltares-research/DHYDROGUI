using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
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
            string filePath = TestHelper.GetTestFilePath(Path.Combine(@"HydroAreaCollection", groupName));
            Assert.IsTrue(File.Exists(filePath));
            filePath = TestHelper.CreateLocalCopy(filePath);
            try
            {
                var ldbFile = new LdbFile();
                IList<LandBoundary2D> readObjects = ldbFile.Read(filePath);
                List<IGrouping<string, LandBoundary2D>> groups = readObjects.GroupBy(g => g.GroupName).ToList();
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