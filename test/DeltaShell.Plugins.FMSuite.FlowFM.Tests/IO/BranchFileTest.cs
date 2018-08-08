using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils;
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
            var iniCategories = BranchFile.Read(filePath);
            for (var n = 0; n < iniCategories.Count; n++)
            {
                Assert.That(iniCategories[n].GetPropertyValue(BranchFile.KnownPropertyNames.Name), Is.EqualTo(branches[n].Name));
                Assert.That(iniCategories[n].GetPropertyValue(BranchFile.KnownPropertyNames.BranchType), Is.EqualTo(GetBranchType(branches[n])));
                Assert.That(iniCategories[n].GetPropertyValue(BranchFile.KnownPropertyNames.WaterType), Is.EqualTo(GetWaterTypeDescription(branches[n])));
            }
        }

        private static string GetWaterTypeDescription(IBranch branch)
        {
            var sewerConnection = branch as ISewerConnection;
            var waterType = sewerConnection?.WaterType ?? SewerConnectionWaterType.None;
            return EnumDescriptionAttributeTypeConverter.GetEnumDescription(waterType);
        }

        private static string GetBranchType(IBranch branch)
        {
            var value = BranchFile.BranchTypes.Unkown;
            if (branch is IChannel)
            {
                value = BranchFile.BranchTypes.Channel;
            }
            if (branch is IPipe)
            {
                value = BranchFile.BranchTypes.Pipe;
            }
            if (branch is ISewerConnection)
            {
                value = BranchFile.BranchTypes.SewerConnection;
            }

            return ((int)value).ToString();
        }
    }
}