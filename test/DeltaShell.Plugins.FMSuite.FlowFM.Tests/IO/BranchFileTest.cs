using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.IO;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class BranchFileTest
    {
        private string filePath;

        [SetUp]
        public void Setup()
        {
            filePath = Path.Combine(FileUtils.CreateTempDirectory(), UGridToNetworkAdapter.BranchGuiFileName);
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(filePath);
        }

        [Test]
        public void GivenTwoSewerConnections_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var sewerConnections = new List<IBranch>
            {
                new SewerConnection("sc_1"), new SewerConnection("sc_2")
            };
            
            BranchFile.Write(sewerConnections, filePath);
            CheckBranchTypeFileContent(sewerConnections);
        }

        [Test]
        public void GivenTwoPipes_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var pipes = new List<IBranch>
            {
                new Pipe { Name = "pipe_1" }, new Pipe { Name = "pipe_2" }
            };

            BranchFile.Write(pipes, filePath);
            CheckBranchTypeFileContent(pipes);
        }

        [Test]
        public void GivenTwoChannels_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var channels = new List<IBranch>
            {
                new Channel { Name = "channel_1" }, new Channel { Name = "channel_2" }
            };

            BranchFile.Write(channels, filePath);
            CheckBranchTypeFileContent(channels);
        }

        [Test]
        public void GivenDifferentTypesOfBranches_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var branches = new List<IBranch>
            {
                new Channel { Name = "myChannel" }, new Pipe { Name = "myPipe" }, new SewerConnection("mySewerConnection")
            };

            BranchFile.Write(branches, filePath);
            CheckBranchTypeFileContent(branches);
        }

        [Test]
        public void GivenBranchTypeFile_WhenReadingFile_ThenCorrectLookupDictionaryIsReturned()
        {
            var branches = new List<IBranch>
            {
                new Channel { Name = "myChannel" }, new Pipe { Name = "myPipe" }, new SewerConnection("mySewerConnection")
            };
            BranchFile.Write(branches, filePath);
            var branchToTypeDict = BranchFile.Read(filePath);

            Assert.That(branchToTypeDict.Count, Is.EqualTo(3));
            Assert.That(branchToTypeDict["myChannel"], Is.EqualTo((int)BranchFile.BranchTypes.Channel));
            Assert.That(branchToTypeDict["myPipe"], Is.EqualTo((int)BranchFile.BranchTypes.Pipe));
            Assert.That(branchToTypeDict["mySewerConnection"], Is.EqualTo((int)BranchFile.BranchTypes.SewerConnection));
        }


        private void CheckBranchTypeFileContent(List<IBranch> branches)
        {
            var fileContent = File.ReadAllLines(filePath);
            Assert.That(fileContent.Length, Is.EqualTo(branches.Count + 1));
            CheckHeaderLine(fileContent);
            CheckDataContent(fileContent, branches);
        }

        private static void CheckDataContent(string[] fileContent, List<IBranch> branches)
        {
            for (var n = 0; n < branches.Count; n++)
                Assert.That(fileContent[n+1], Is.EqualTo(branches[n].Name + ";" + GetBranchType(branches[n])));
        }

        private static int GetBranchType(IBranch branch)
        {
            if (branch is IChannel)
            {
                return (int)BranchFile.BranchTypes.Channel;
            }
            if (branch is IPipe)
            {
                return (int)BranchFile.BranchTypes.Pipe;
            }
            if (branch is ISewerConnection)
            {
                return (int)BranchFile.BranchTypes.SewerConnection;
            }

            return (int)BranchFile.BranchTypes.Unkown;
        }

        private static void CheckHeaderLine(string[] fileContent)
        {
            Assert.That(fileContent[0], Is.EqualTo("BranchId;Type"));
        }
    }
}