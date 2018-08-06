using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class BranchTypeFileTest
    {
        private string filePath;

        [SetUp]
        public void Setup()
        {
            filePath = Path.Combine(FileUtils.CreateTempDirectory(), "branchGui.csv");
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
            
            BranchTypeFile.Write(sewerConnections, filePath);
            CheckBranchTypeFileContent(sewerConnections);
        }

        [Test]
        public void GivenTwoPipes_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var pipes = new List<IBranch>
            {
                new Pipe { Name = "pipe_1" }, new Pipe { Name = "pipe_2" }
            };

            BranchTypeFile.Write(pipes, filePath);
            CheckBranchTypeFileContent(pipes);
        }

        [Test]
        public void GivenTwoChannels_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var channels = new List<IBranch>
            {
                new Channel { Name = "channel_1" }, new Channel { Name = "channel_2" }
            };

            BranchTypeFile.Write(channels, filePath);
            CheckBranchTypeFileContent(channels);
        }

        [Test]
        public void GivenDifferentTypesOfBranches_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var branches = new List<IBranch>
            {
                new Channel { Name = "myChannel" }, new Pipe { Name = "myPipe" }, new SewerConnection("mySewerConnection")
            };

            BranchTypeFile.Write(branches, filePath);
            CheckBranchTypeFileContent(branches);
        }

        [Test]
        public void GivenBranchTypeFile_WhenReadingFile_ThenCorrectLookupDictionaryIsReturned()
        {
            var branches = new List<IBranch>
            {
                new Channel { Name = "myChannel" }, new Pipe { Name = "myPipe" }, new SewerConnection("mySewerConnection")
            };
            BranchTypeFile.Write(branches, filePath);
            var branchToTypeDict = BranchTypeFile.Read(filePath);

            Assert.That(branchToTypeDict.Count, Is.EqualTo(3));
            Assert.That(branchToTypeDict["myChannel"], Is.EqualTo(typeof(Channel).Name));
            Assert.That(branchToTypeDict["myPipe"], Is.EqualTo(typeof(Pipe).Name));
            Assert.That(branchToTypeDict["mySewerConnection"], Is.EqualTo(typeof(SewerConnection).Name));
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
                Assert.That(fileContent[n+1], Is.EqualTo(branches[n].Name + ";" + branches[n].GetType().Name));
        }

        private static void CheckHeaderLine(string[] fileContent)
        {
            Assert.That(fileContent[0], Is.EqualTo("BranchId;Type"));
        }
    }
}