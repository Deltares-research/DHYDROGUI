using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
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

            WriteAndCheckBranchTypeFileContent(sewerConnections);
        }

        [Test]
        public void GivenTwoPipes_WhenWritingBranchTypeFile_ThenBranchTypesAreCorrectlyWritten()
        {
            var pipes = new List<IBranch>
            {
                new Pipe { Name = "pipe_1" }, new Pipe { Name = "pipe_2" }
            };
            
            WriteAndCheckBranchTypeFileContent(pipes);
        }

        [Test]
        [Category("Quarantine")]
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
            
            WriteAndCheckBranchTypeFileContent(pipes);
        }

        [Test]
        public void GivenSewerConnections_WhenWritingBranchTypeFile_ThenMaterialIsWrittenToBranchFile()
        {
            var pipes = new List<IBranch>
            {
                new Pipe { Name = "pipe_1", Material = SewerProfileMapping.SewerProfileMaterial.Masonry },
                new Pipe { Name = "pipe_2", Material = SewerProfileMapping.SewerProfileMaterial.SheetMetal },
                new SewerConnection { Name = "sc_2" },
                new Channel { Name = "channel_1" }
            };
            
            WriteAndCheckBranchTypeFileContent(pipes);
        }

        [Test]
        public void GivenTwoChannels_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var channels = new List<IBranch>
            {
                new Channel { Name = "channel_1" }, new Channel { Name = "channel_2" }
            };
            
            WriteAndCheckBranchTypeFileContent(channels);
        }

        [Test]
        [Category("Quarantine")]
        public void GivenDifferentTypesOfBranches_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var branches = new List<IBranch>
            {
                new Channel { Name = "myChannel" }, new Pipe { Name = "myPipe" }, new SewerConnection("mySewerConnection")
            };
            
            WriteAndCheckBranchTypeFileContent(branches);
        }

        private void WriteAndCheckBranchTypeFileContent(List<IBranch> branches)
        {
            BranchFile.Write(filePath, branches);
            var propertiesPerBranch = BranchFile.Read(filePath, null);
            for (var n = 0; n < propertiesPerBranch.Count; n++)
            {
                Assert.That(propertiesPerBranch[n].Name, Is.EqualTo(branches[n].Name));
                Assert.That(propertiesPerBranch[n].BranchType, Is.EqualTo(GetBranchType(branches[n])));
                Assert.That(propertiesPerBranch[n].WaterType, Is.EqualTo(GetWaterType(branches[n])));
                Assert.That(propertiesPerBranch[n].Material, Is.EqualTo(GetMaterial(branches[n])));
            }
        }

        private static SewerConnectionWaterType GetWaterType(IBranch branch)
        {
            var sewerConnection = branch as ISewerConnection;
            return sewerConnection?.WaterType ?? SewerConnectionWaterType.None;
        }

        private static SewerProfileMapping.SewerProfileMaterial GetMaterial(IBranch branch)
        {
            var pipe = branch as Pipe;
            return pipe?.Material ?? SewerProfileMapping.SewerProfileMaterial.Unknown;
        }

        private static BranchFile.BranchType GetBranchType(IBranch branch)
        {
            switch (branch)
            {
                case IPipe p:
                    return BranchFile.BranchType.Pipe;
                case ISewerConnection s:
                    return BranchFile.BranchType.SewerConnection;
                default:
                    return BranchFile.BranchType.Channel;

            }

        }
    }
}