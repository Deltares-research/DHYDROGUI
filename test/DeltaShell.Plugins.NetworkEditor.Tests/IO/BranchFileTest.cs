using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.NetworkEditor.IO;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.IO
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
        public void GivenSewerConnections_WhenWritingBranchTypeFile_ThenWaterTypeIsWrittenToBranchFile()
        {
            var pipes = new List<IBranch>
            {
                new Pipe { Name = "pipe_1", WaterType = SewerConnectionWaterType.Combined },
                new SewerConnection { Name = "sc_1", WaterType = SewerConnectionWaterType.DryWater },
                new Pipe { Name = "pipe_2", WaterType = SewerConnectionWaterType.None },
                new SewerConnection { Name = "sc_2", WaterType = SewerConnectionWaterType.StormWater },
                new Channel { Name = "channel_1" }
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

        private void CheckBranchTypeFileContent(List<IBranch> branches)
        {
            BranchFile.Write(branches, filePath);
            var propertiesPerBranch = BranchFile.Read(filePath);
            for (var n = 0; n < propertiesPerBranch.Count; n++)
            {
                Assert.That(propertiesPerBranch[n].Name, Is.EqualTo(branches[n].Name));
                Assert.That(propertiesPerBranch[n].BranchType, Is.EqualTo(GetBranchType(branches[n])));
                Assert.That(propertiesPerBranch[n].WaterType, Is.EqualTo(GetWaterTypeDescription(branches[n])));
            }
        }

        private static SewerConnectionWaterType GetWaterTypeDescription(IBranch branch)
        {
            var sewerConnection = branch as ISewerConnection;
            return sewerConnection?.WaterType ?? SewerConnectionWaterType.None;
        }

        private static BranchFile.BranchType GetBranchType(IBranch branch)
        {
            var value = BranchFile.BranchType.Unkown;
            if (branch is IChannel)
            {
                value = BranchFile.BranchType.Channel;
            }
            else if (branch is IPipe)
            {
                value = BranchFile.BranchType.Pipe;
            }
            else if (branch is ISewerConnection)
            {
                value = BranchFile.BranchType.SewerConnection;
            }

            return value;
        }
    }
}