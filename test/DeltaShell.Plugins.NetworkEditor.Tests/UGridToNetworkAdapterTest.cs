using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.NetworkEditor.IO;
using DeltaShell.Plugins.NetworkEditor.Tests.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class UGridToNetworkAdapterTest
    {
        private string testDirectory;
        private string netFilePath;
        private string branchGuiFilePath;

        [SetUp]
        public void Setup()
        {
            testDirectory = FileUtils.CreateTempDirectory();
            netFilePath = Path.Combine(testDirectory, "myNetFile.nc");
            branchGuiFilePath = Path.Combine(testDirectory, UGridToNetworkAdapter.BranchGuiFileName);
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(testDirectory);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenSimpleSewerNewtwork_WhenSavingNetwork_ThenBranchTypeFileIsCorrectlyWritten()
        {
            const string pipeName = "myPipe";
            var sewerNetwork = TestNetworkAndDiscretisationProvider.CreateSimpleSewerNetwork(pipeName);

            var metaData = new UGridGlobalMetaData(sewerNetwork.Name, "PluginName", "PluginVersion");
            UGridToNetworkAdapter.SaveNetwork(sewerNetwork, netFilePath, metaData);

            // Check file existence
            Assert.IsTrue(File.Exists(netFilePath));
            Assert.IsTrue(File.Exists(branchGuiFilePath));

            // Check file content
            var iniCategories = BranchFile.Read(branchGuiFilePath);
            Assert.That(iniCategories.Count, Is.EqualTo(1));
            var branchInfo = iniCategories.First();

            Assert.That(branchInfo.GetPropertyValue(BranchFile.KnownPropertyNames.Name), Is.EqualTo(pipeName));
            Assert.That(branchInfo.GetPropertyValue(BranchFile.KnownPropertyNames.BranchType), Is.EqualTo("3"));
            Assert.That(branchInfo.GetPropertyValue(BranchFile.KnownPropertyNames.WaterType), Is.EqualTo("DWA"));
        }
    }
}