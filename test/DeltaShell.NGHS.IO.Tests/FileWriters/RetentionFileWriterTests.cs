using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters
{
    [TestFixture]
    public class RetentionFileWriterTests
    {
        [Test]
        public void TestRetentionFileWriter()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(50, true);
            
            var data = FunctionHelper.Get1DFunction<double, double>("Storage", "Height", "Storage");
            data.Arguments[0].SetValues(new [] {0.0d, 1.0d, 2.0d});
            data.SetValues(new [] {10.0d, 100.0d, 1000.0d});
            data.Arguments[0].InterpolationType = InterpolationType.Linear;
            
            var data2 = FunctionHelper.Get1DFunction<double, double>("Storage", "Height", "Storage");
            data2.Arguments[0].SetValues(new [] {3.0d, 4.0d, 5.0d});
            data2.SetValues(new [] {20.0d, 200.0d, 2000.0d});
            data2.Arguments[0].InterpolationType = InterpolationType.Constant;
            
            const string aRetentionArea = "A retention area";
            var retentions = new List<IRetention>()
            {
                new Retention(){Name = "1", LongName = aRetentionArea, Branch = network.Branches.ElementAt(0), Chainage = network.Branches.ElementAt(0).Length/2, UseTable = false, BedLevel = 1,StorageArea = 10},
                new Retention(){Name = "2", LongName = aRetentionArea, Branch = network.Branches.ElementAt(1), Chainage = network.Branches.ElementAt(1).Length/3, UseTable = false, BedLevel = 1,StorageArea = 10},
                new Retention(){Name = "3", LongName = aRetentionArea, Branch = network.Branches.ElementAt(2), Chainage = network.Branches.ElementAt(2).Length/4, UseTable = false, BedLevel = 1,StorageArea = 10},
                new Retention(){Name = "4", LongName = aRetentionArea, Branch = network.Branches.ElementAt(3), Chainage = network.Branches.ElementAt(3).Length/5, UseTable = true, Data = data},
                new Retention(){Name = "5", LongName = aRetentionArea, Branch = network.Branches.ElementAt(4), Chainage = network.Branches.ElementAt(4).Length/6, UseTable = true, Data = data2},
            };
            RetentionFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.Retention, retentions);
            var iniSections = new IniReader().ReadIniFile(FileWriterTestHelper.ModelFileNames.Retention);
            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(5, iniSections.Count(c => c.Name == RetentionRegion.Header));
            var retention = iniSections.Where(c => c.Name == RetentionRegion.Header).ElementAt(0);

            var name = retention.ReadProperty<string>(RetentionRegion.Id.Key);
            Assert.AreEqual("1", name);
            
            var longName = retention.ReadProperty<string>(RetentionRegion.Name.Key);
            Assert.AreEqual(aRetentionArea, longName);
            
            var branchId = retention.ReadProperty<string>(RetentionRegion.BranchId.Key);
            Assert.AreEqual(network.Branches.ElementAt(0).Name, branchId);
            
            var chainage = retention.ReadProperty<double>(RetentionRegion.Chainage.Key);
            Assert.AreEqual(network.Branches.ElementAt(0).Length /2, chainage, 0.1); // hmm double compare!!
            
            var useTable = retention.ReadProperty<bool>(RetentionRegion.UseTable.Key);
            Assert.AreEqual(true, useTable);

            var level = retention.ReadProperty<double>(RetentionRegion.Levels.Key);
            Assert.AreEqual(1, level, 0.1); // hmm double compare!!
            
            var storageArea = retention.ReadProperty<double>(RetentionRegion.StorageArea.Key);
            Assert.AreEqual(10, storageArea, 0.1); // hmm double compare!!

            retention = iniSections.Where(c => c.Name == RetentionRegion.Header).ElementAt(1);
            name = retention.ReadProperty<string>(RetentionRegion.Id.Key);
            Assert.AreEqual("2", name);

            longName = retention.ReadProperty<string>(RetentionRegion.Name.Key);
            Assert.AreEqual(aRetentionArea, longName);

            branchId = retention.ReadProperty<string>(RetentionRegion.BranchId.Key);
            Assert.AreEqual(network.Branches.ElementAt(1).Name, branchId);

            chainage = retention.ReadProperty<double>(RetentionRegion.Chainage.Key);
            Assert.AreEqual(network.Branches.ElementAt(1).Length / 3, chainage, 0.1); // hmm double compare!!
            
            useTable = retention.ReadProperty<bool>(RetentionRegion.UseTable.Key);
            Assert.AreEqual(true, useTable);

            level = retention.ReadProperty<double>(RetentionRegion.Levels.Key);
            Assert.AreEqual(1, level, 0.1); // hmm double compare!!

            storageArea = retention.ReadProperty<double>(RetentionRegion.StorageArea.Key);
            Assert.AreEqual(10, storageArea, 0.1); // hmm double compare!!
            
            retention = iniSections.Where(c => c.Name == RetentionRegion.Header).ElementAt(2);
            name = retention.ReadProperty<string>(RetentionRegion.Id.Key);
            Assert.AreEqual("3", name);

            longName = retention.ReadProperty<string>(RetentionRegion.Name.Key);
            Assert.AreEqual(aRetentionArea, longName);

            branchId = retention.ReadProperty<string>(RetentionRegion.BranchId.Key);
            Assert.AreEqual(network.Branches.ElementAt(2).Name, branchId);

            chainage = retention.ReadProperty<double>(RetentionRegion.Chainage.Key);
            Assert.AreEqual(network.Branches.ElementAt(2).Length / 4, chainage, 0.1); // hmm double compare!!
            
            useTable = retention.ReadProperty<bool>(RetentionRegion.UseTable.Key);
            Assert.AreEqual(true, useTable);

            level = retention.ReadProperty<double>(RetentionRegion.Levels.Key);
            Assert.AreEqual(1, level, 0.1); // hmm double compare!!

            storageArea = retention.ReadProperty<double>(RetentionRegion.StorageArea.Key);
            Assert.AreEqual(10, storageArea, 0.1); // hmm double compare!!
            
            retention = iniSections.Where(c => c.Name == RetentionRegion.Header).ElementAt(3);
            name = retention.ReadProperty<string>(RetentionRegion.Id.Key);
            Assert.AreEqual("4", name);

            longName = retention.ReadProperty<string>(RetentionRegion.Name.Key);
            Assert.AreEqual(aRetentionArea, longName);

            branchId = retention.ReadProperty<string>(RetentionRegion.BranchId.Key);
            Assert.AreEqual(network.Branches.ElementAt(3).Name, branchId);

            chainage = retention.ReadProperty<double>(RetentionRegion.Chainage.Key);
            Assert.AreEqual(network.Branches.ElementAt(3).Length / 5, chainage, 0.1); // hmm double compare!!
            
            useTable = retention.ReadProperty<bool>(RetentionRegion.UseTable.Key);
            Assert.AreEqual(true, useTable);
            
            var numLevels = retention.ReadProperty<int>(RetentionRegion.NumLevels.Key);
            Assert.AreEqual(3, numLevels);

            var heights = retention.ReadPropertiesToListOfType<double>(RetentionRegion.Levels.Key);
            Assert.AreEqual(3, heights.Count);
            Assert.AreEqual(0, heights[0], 0.1d);
            Assert.AreEqual(1, heights[1], 0.1d);
            Assert.AreEqual(2, heights[2], 0.1d);

            var storages = retention.ReadPropertiesToListOfType<double>(RetentionRegion.StorageArea.Key);
            Assert.AreEqual(3, storages.Count);
            Assert.AreEqual(10, storages[0], 0.1d);
            Assert.AreEqual(100, storages[1], 0.1d);
            Assert.AreEqual(1000, storages[2], 0.1d);

            var interpolate = retention.ReadProperty<string>(RetentionRegion.Interpolate.Key);
            Assert.AreEqual("linear", interpolate);
            
            retention = iniSections.Where(c => c.Name == RetentionRegion.Header).ElementAt(4);
            name = retention.ReadProperty<string>(RetentionRegion.Id.Key);
            Assert.AreEqual("5", name);

            longName = retention.ReadProperty<string>(RetentionRegion.Name.Key);
            Assert.AreEqual(aRetentionArea, longName);

            branchId = retention.ReadProperty<string>(RetentionRegion.BranchId.Key);
            Assert.AreEqual(network.Branches.ElementAt(4).Name, branchId);

            chainage = retention.ReadProperty<double>(RetentionRegion.Chainage.Key);
            Assert.AreEqual(network.Branches.ElementAt(4).Length / 6, chainage, 0.1); // hmm double compare!!
            
            useTable = retention.ReadProperty<bool>(RetentionRegion.UseTable.Key);
            Assert.AreEqual(true, useTable);
            
            numLevels = retention.ReadProperty<int>(RetentionRegion.NumLevels.Key);
            Assert.AreEqual(3, numLevels);

            heights = retention.ReadPropertiesToListOfType<double>(RetentionRegion.Levels.Key);
            Assert.AreEqual(3, heights.Count);
            Assert.AreEqual(3, heights[0], 0.1d);
            Assert.AreEqual(4, heights[1], 0.1d);
            Assert.AreEqual(5, heights[2], 0.1d);

            storages = retention.ReadPropertiesToListOfType<double>(RetentionRegion.StorageArea.Key);
            Assert.AreEqual(3, storages.Count);
            Assert.AreEqual(20, storages[0], 0.1d);
            Assert.AreEqual(200, storages[1], 0.1d);
            Assert.AreEqual(2000, storages[2], 0.1d);

            interpolate = retention.ReadProperty<string>(RetentionRegion.Interpolate.Key);
            Assert.AreEqual("block", interpolate);
        }

        [Test]
        public void TestRetentionFileWriterThrowsErrorBecauseOfWrongInterpolationType()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1, true);
            Assert.AreEqual(1, network.Branches.Count);
            var data = FunctionHelper.Get1DFunction<double, double>("Storage", "Height", "Storage");
            data.Arguments[0].SetValues(new[] {0.0d, 1.0d, 2.0d});
            data.SetValues(new[] {10.0d, 100.0d, 1000.0d});
            data.Arguments[0].InterpolationType = InterpolationType.None;
            
            const string aRetentionArea = "A retention area";
            var retentions = new List<IRetention>()
            {
                new Retention()
                {
                    Name = "1",
                    LongName = aRetentionArea,
                    Branch = network.Branches.ElementAt(0),
                    Chainage = network.Branches.ElementAt(0).Length/2,
                    Type = RetentionType.Closed,
                    UseTable = true,
                    Data = data
                },
            };

            Assert.Throws<FileWritingException>(() =>
                RetentionFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.Retention, retentions));
        }

        [Test]
        public void TestRetentionFileWriterThrowsErrorBecauseOfWrongLevelsType()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1, true);
            Assert.AreEqual(1, network.Branches.Count);
            var data = FunctionHelper.Get1DFunction<string, double>("Storage", "Height", "Storage");
            data.Arguments[0].SetValues(new[] {"0", "1", "2"});
            data.SetValues(new[] {10.0d, 100.0d, 1000.0d});
            data.Arguments[0].InterpolationType = InterpolationType.Linear;
            
            const string aRetentionArea = "A retention area";
            var retentions = new List<IRetention>()
            {
                new Retention()
                {
                    Name = "1",
                    LongName = aRetentionArea,
                    Branch = network.Branches.ElementAt(0),
                    Chainage = network.Branches.ElementAt(0).Length/2,
                    Type = RetentionType.Closed,
                    UseTable = true,
                    Data = data
                },
            };

            Assert.Throws<FileWritingException>(() =>
                RetentionFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.Retention, retentions));
        }

        [Test]
        public void TestRetentionFileWriterThrowsErrorBecauseOfWrongStorageType()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1, true);
            Assert.AreEqual(1, network.Branches.Count);
            var data = FunctionHelper.Get1DFunction<double, int>("Storage", "Height", "Storage");
            data.Arguments[0].SetValues(new[] {0d, 1d, 2d});
            data.SetValues(new[] {10, 100, 1000});
            data.Arguments[0].InterpolationType = InterpolationType.Linear;
            
            const string aRetentionArea = "A retention area";
            var retentions = new List<IRetention>()
            {
                new Retention()
                {
                    Name = "1",
                    LongName = aRetentionArea,
                    Branch = network.Branches.ElementAt(0),
                    Chainage = network.Branches.ElementAt(0).Length/2,
                    Type = RetentionType.Closed,
                    UseTable = true,
                    Data = data
                },
            };

            Assert.Throws<FileWritingException>(() =>
                RetentionFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.Retention, retentions));
        }


        [Test]
        [TestCase(0, true, true)]
        [TestCase(100, true, false)]
        [TestCase(100 - 1e-12, true, false)]
        [TestCase(50, false, false)]
        public void GivenRetentionFileWriter_WritingARetentionOnANode_ShouldAddNodeId(double chainage, bool expectedNodeId, bool expectSourceNodeId)
        {
            //Arrange
            var branch = Substitute.For<IBranch>();
            var sourceNode = Substitute.For<INode>();
            var targetNode = Substitute.For<INode>();

            branch.Length.Returns(100);
            branch.Name.Returns("Branch1");
            branch.Source.Returns(sourceNode);
            branch.Target.Returns(targetNode);

            sourceNode.Name.Returns("Node1");
            targetNode.Name.Returns("Node2");

            var retention = new Retention
            {
                Branch = branch,
                Chainage = chainage
            };

            // Act
            var iniSection = RetentionFileWriter.GenerateSpatialDataDefinition(retention, "Retention");

            // Assert
            var nodeIdProperty = iniSection.GetProperty(RetentionRegion.NodeId.Key);
            Assert.AreEqual(expectedNodeId, nodeIdProperty != null, $"NodeId tag should {(expectedNodeId ? "" : "not ")}be added");

            if (expectedNodeId)
            {
                Assert.NotNull(nodeIdProperty);
                var expectedNodeName = expectSourceNodeId ? sourceNode.Name : targetNode.Name;
                Assert.AreEqual(expectedNodeName, nodeIdProperty.Value, $"Expected nodeId value to be {expectedNodeName} instead of {nodeIdProperty.Value}");
            }

            var branchProperty = iniSection.GetProperty(RetentionRegion.BranchId.Key);
            Assert.AreEqual(expectedNodeId, branchProperty == null, $"Branch property should {(expectedNodeId ? "not " : "")}be added (because there is a NodeId)");

            var chainageProperty = iniSection.GetProperty(RetentionRegion.Chainage.Key);
            Assert.AreEqual(expectedNodeId, chainageProperty == null, $"Chainage property should {(expectedNodeId ? "not " : "")}be added (because there is a NodeId)");
        }
    }
}