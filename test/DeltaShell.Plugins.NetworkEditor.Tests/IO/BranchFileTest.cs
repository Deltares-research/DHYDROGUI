using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
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
            filePath = Path.Combine(FileUtils.CreateTempDirectory(), NetworkPropertiesHelper.BranchGuiFileName);
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
        public void GivenSewerConnections_WhenWritingBranchTypeFile_ThenWaterTypeIsWrittenToBranchFile()
        {
            var pipes = new List<IBranch>
            {
                new Pipe { Name = "pipe_1", WaterType = SewerConnectionWaterType.Combined },
                new SewerConnection { Name = "sc_1", WaterType = SewerConnectionWaterType.DryWater },
                new Pipe { Name = "pipe_2", WaterType = SewerConnectionWaterType.None },
                new SewerConnection { Name = "sc_2", WaterType = SewerConnectionWaterType.StormWater }
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
        public void GivenDifferentTypesOfBranches_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var branches = new List<IBranch>
            {
                new Pipe { Name = "myPipe" }, new SewerConnection("mySewerConnection")
            };
            
            WriteAndCheckBranchTypeFileContent(branches);
        }

        [Test]
        public void GivenChannel_GettingBranchProperties_ShouldGiveCorrectPropertiesValues()
        {
            //Arrange
            var node1 = new HydroNode("Node 1"){Geometry = new Point(0,0)};
            var node2 = new HydroNode("Node 2") { Geometry = new Point(10, 10) };

            var branch = new Channel("channel",node1, node2)
            {
                Geometry = new LineString(new []{new Coordinate(0,0), new Coordinate(10,10)}),
                Length = 100,
                IsLengthCustom = true
            };

            // Act
            var properties = branch.GetBranchProperties();

            // Assert
            Assert.AreEqual(BranchFile.BranchType.Channel, properties.BranchType);
            Assert.AreEqual(true, properties.IsCustomLength);
        }

        [Test]
        public void GivenPipe_GettingBranchProperties_ShouldGiveCorrectPropertiesValues()
        {
            //Arrange
            var manhole1 = new Manhole("Node 1") { Geometry = new Point(0, 0) };
            var manhole2 = new Manhole("Node 2") { Geometry = new Point(10, 10) };

            var compartment1 = new Compartment("Compartment 1");
            var compartment2 = new Compartment("Compartment 2");

            manhole1.Compartments.Add(compartment1);
            manhole2.Compartments.Add(compartment2);

            var pipe = new Pipe
            {
                Name = "Pipe 1",
                Source = manhole1,
                Target = manhole2,
                SourceCompartment = compartment1,
                TargetCompartment = compartment2,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(10, 10) }),
                Length = 100,
                IsLengthCustom = true
            };

            // Act
            var properties = pipe.GetBranchProperties();

            // Assert
            Assert.AreEqual(BranchFile.BranchType.Pipe, properties.BranchType);
            Assert.AreEqual(pipe.Name, properties.Name);
            Assert.AreEqual(true, properties.IsCustomLength);
            Assert.AreEqual(compartment1.Name ,properties.SourceCompartmentName);
            Assert.AreEqual(compartment2.Name , properties.TargetCompartmentName);
            Assert.AreEqual(pipe.WaterType, properties.WaterType);
            Assert.AreEqual(pipe.Material, properties.Material);
        }

        [Test]
        public void GivenSewerConnection_GettingBranchProperties_ShouldGiveCorrectPropertiesValues()
        {
            //Arrange
            var manhole1 = new Manhole("Node 1") { Geometry = new Point(0, 0) };
            var manhole2 = new Manhole("Node 2") { Geometry = new Point(10, 10) };

            var compartment1 = new Compartment("Compartment 1");
            var compartment2 = new Compartment("Compartment 2");

            manhole1.Compartments.Add(compartment1);
            manhole2.Compartments.Add(compartment2);

            var sewerConnection = new SewerConnection("SewerConnection 1")
            {
                Source = manhole1,
                Target = manhole2,
                SourceCompartment = compartment1,
                TargetCompartment = compartment2,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(10, 10) }),
                Length = 100,
                IsLengthCustom = true
            };

            // Act
            var properties = sewerConnection.GetBranchProperties();

            // Assert
            Assert.AreEqual(BranchFile.BranchType.SewerConnection, properties.BranchType);
            Assert.AreEqual(sewerConnection.Name, properties.Name);
            Assert.AreEqual(true, properties.IsCustomLength);
            Assert.AreEqual(compartment1.Name, properties.SourceCompartmentName);
            Assert.AreEqual(compartment2.Name, properties.TargetCompartmentName);
            Assert.AreEqual(sewerConnection.WaterType, properties.WaterType);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Read_FilePathNullOrWhiteSpace_ThrowsArgumentException(string filePath)
        {
            // Call
            void Call() => BranchFile.Read(filePath, "FlowFM_net.nc", Substitute.For<IIniReader>(), Substitute.For<ILogHandler>());

            // Assert
            Assert.That(Call, Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("filePath"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Read_NetFilePathNullOrWhiteSpace_ThrowsArgumentException(string netFilePath)
        {
            // Call
            void Call() => BranchFile.Read("branches.gui", netFilePath, Substitute.For<IIniReader>(), Substitute.For<ILogHandler>());

            // Assert
            Assert.That(Call, Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("netFilePath"));
        }

        [Test]
        public void Read_IniReaderNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => BranchFile.Read("branches.gui", "FlowFM_net.nc", null, Substitute.For<ILogHandler>());

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("iniReader"));
        }

        [Test]
        public void Read_LogHandlerNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => BranchFile.Read("branches.gui", "FlowFM_net.nc", Substitute.For<IIniReader>(), null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("logHandler"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Read_NullOrWhiteSpaceFileVersion_SkipsReadingFile(string fileVersion)
        {
            // Setup
            var filePath = "branches.gui";
            var iniReader = Substitute.For<IIniReader>();
            var logHandler = Substitute.For<ILogHandler>();

            var readIniSections = new List<IniSection>
            {
                CreateGeneralIniSection(fileVersion),
                CreateBranchIniSection("1"),
                CreateBranchIniSection("2")
            };
            iniReader.ReadIniFile(filePath).Returns(readIniSections);

            // Call
            IList<BranchProperties> branchProperties = BranchFile.Read(filePath, "FlowFM_net.nc", iniReader, logHandler);

            // Assert
            logHandler.Received(1).ReportError($"File version in general section is empty. branches.gui file will not be read.");
            Assert.That(branchProperties, Is.Empty);
        }

        [Test]
        public void Read_InvalidFileVersion_SkipsReadingFile()
        {
            // Setup
            var filePath = "branches.gui";
            var iniReader = Substitute.For<IIniReader>();
            var logHandler = Substitute.For<ILogHandler>();

            var readIniSections = new List<IniSection>
            {
                CreateGeneralIniSection("abc"),
                CreateBranchIniSection("1"),
                CreateBranchIniSection("2")
            };
            iniReader.ReadIniFile(filePath).Returns(readIniSections);

            // Call
            IList<BranchProperties> branchProperties = BranchFile.Read(filePath, "FlowFM_net.nc", iniReader, logHandler);

            // Assert
            logHandler.Received(1).ReportError("File version in general section is invalid: abc. branches.gui file will not be read.");
            Assert.That(branchProperties, Is.Empty);
        }

        [Test]
        public void Read_UnsupportedFileVersion_SkipsReadingFile()
        {
            // Setup
            var filePath = "branches.gui";
            var iniReader = Substitute.For<IIniReader>();
            var logHandler = Substitute.For<ILogHandler>();

            const string fileVersion = "1.01";

            var readIniSections = new List<IniSection>
            {
                CreateGeneralIniSection(fileVersion),
                CreateBranchIniSection("1"),
                CreateBranchIniSection("2")
            };
            iniReader.ReadIniFile(filePath).Returns(readIniSections);

            // Call
            IList<BranchProperties> branchProperties = BranchFile.Read(filePath, "FlowFM_net.nc", iniReader, logHandler);

            // Assert
            logHandler.Received(1).ReportError($"File version in general section is not supported: {fileVersion}. branches.gui file will not be read.");
            Assert.That(branchProperties, Is.Empty);
        }

        [Test]
        public void Read_MissingGeneralIniSection_LogsWarningAndReadsFileCorrectly()
        {
            // Setup
            var filePath = "branches.gui";
            var iniReader = Substitute.For<IIniReader>();
            var logHandler = Substitute.For<ILogHandler>();

            var readIniSections = new List<IniSection>
            {
                CreateBranchIniSection("1"),
                CreateBranchIniSection("2")
            };
            iniReader.ReadIniFile(filePath).Returns(readIniSections);

            // Call
            IList<BranchProperties> branchProperties = BranchFile.Read(filePath, "FlowFM_net.nc", iniReader, logHandler);

            // Assert
            logHandler.Received(1).ReportWarning("branches.gui file does not contain a general section. Model has probably been made with an older version of this software.");

            Assert.That(branchProperties, Has.Count.EqualTo(2));

            BranchProperties firstBranchProperty = branchProperties[0];
            Assert.That(firstBranchProperty.Name, Is.EqualTo("some_branch_1"));
            Assert.That(firstBranchProperty.BranchType, Is.EqualTo(BranchFile.BranchType.SewerConnection));
            Assert.That(firstBranchProperty.IsCustomLength, Is.True);
            Assert.That(firstBranchProperty.SourceCompartmentName, Is.EqualTo("some_source_compartment_1"));
            Assert.That(firstBranchProperty.TargetCompartmentName, Is.EqualTo("some_target_compartment_1"));

            BranchProperties secondBranchProperty = branchProperties[1];
            Assert.That(secondBranchProperty.Name, Is.EqualTo("some_branch_2"));
            Assert.That(secondBranchProperty.BranchType, Is.EqualTo(BranchFile.BranchType.SewerConnection));
            Assert.That(secondBranchProperty.IsCustomLength, Is.True);
            Assert.That(secondBranchProperty.SourceCompartmentName, Is.EqualTo("some_source_compartment_2"));
            Assert.That(secondBranchProperty.TargetCompartmentName, Is.EqualTo("some_target_compartment_2"));
        }

        [Test]
        public void Read_SupportedFileVersion_ReadsFileCorrectly()
        {
            // Setup
            var filePath = "branches.gui";
            var iniReader = Substitute.For<IIniReader>();
            var logHandler = Substitute.For<ILogHandler>();

            var readIniSections = new List<IniSection>
            {
                CreateGeneralIniSection(),
                CreateBranchIniSection("1"),
                CreateBranchIniSection("2")
            };
            iniReader.ReadIniFile(filePath).Returns(readIniSections);

            // Call
            IList<BranchProperties> branchProperties = BranchFile.Read(filePath, "FlowFM_net.nc", iniReader, logHandler);

            // Assert
            Assert.That(branchProperties, Has.Count.EqualTo(2));

            BranchProperties firstBranchProperty = branchProperties[0];
            Assert.That(firstBranchProperty.Name, Is.EqualTo("some_branch_1"));
            Assert.That(firstBranchProperty.BranchType, Is.EqualTo(BranchFile.BranchType.SewerConnection));
            Assert.That(firstBranchProperty.IsCustomLength, Is.True);
            Assert.That(firstBranchProperty.SourceCompartmentName, Is.EqualTo("some_source_compartment_1"));
            Assert.That(firstBranchProperty.TargetCompartmentName, Is.EqualTo("some_target_compartment_1"));

            BranchProperties secondBranchProperty = branchProperties[1];
            Assert.That(secondBranchProperty.Name, Is.EqualTo("some_branch_2"));
            Assert.That(secondBranchProperty.BranchType, Is.EqualTo(BranchFile.BranchType.SewerConnection));
            Assert.That(secondBranchProperty.IsCustomLength, Is.True);
            Assert.That(secondBranchProperty.SourceCompartmentName, Is.EqualTo("some_source_compartment_2"));
            Assert.That(secondBranchProperty.TargetCompartmentName, Is.EqualTo("some_target_compartment_2"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Write_FilePathNullOrWhiteSpace_ThrowsArgumentException(string filePath)
        {
            // Call
            void Call() => BranchFile.Write(filePath, Enumerable.Empty<IBranch>(), Substitute.For<IIniWriter>());

            // Assert
            Assert.That(Call, Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("filePath"));
        }

        [Test]
        public void Write_BranchesNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => BranchFile.Write("branches.gui", null, Substitute.For<IIniWriter>());

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("branches"));
        }

        [Test]
        public void Write_IniWriterNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => BranchFile.Write("branches.gui", Enumerable.Empty<IBranch>(), null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("iniWriter"));
        }

        [Test]
        public void Write_AddsGeneralIniSection()
        {
            // Setup
            var iniWriter = Substitute.For<IIniWriter>();

            IniSection generalIniSection = CreateGeneralIniSection();

            IniSection[] expectedIniSections =
            {
                generalIniSection
            };

            // Call
            BranchFile.Write("branches.gui", Enumerable.Empty<IBranch>(), iniWriter);

            // Assert
            iniWriter.Received(1).WriteIniFile(
                MatchingIniSections(expectedIniSections),
                "branches.gui",
                true);
        }

        private void WriteAndCheckBranchTypeFileContent(List<IBranch> branches)
        {
            BranchFile.Write(filePath, branches, new IniWriter());
            IList<BranchProperties> propertiesPerBranch = BranchFile.Read(filePath, "FlowFM_net.nc", new IniReader(), Substitute.For<ILogHandler>());
            for (var n = 0; n < propertiesPerBranch.Count; n++)
            {
                Assert.That(propertiesPerBranch[n].Name, Is.EqualTo(branches[n].Name));
                Assert.That(propertiesPerBranch[n].BranchType, Is.EqualTo(GetBranchType(branches[n])));
                Assert.That(propertiesPerBranch[n].Material, Is.EqualTo(GetMaterial(branches[n])));
            }
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

        private static IEnumerable<IniSection> MatchingIniSections(IEnumerable<IniSection> expectedIniSections)
        {
            return Arg.Is<IEnumerable<IniSection>>(actualIniSections => expectedIniSections.SequenceEqual(actualIniSections));
        }

        private static IniSection CreateGeneralIniSection(string fileVersion = "2.00")
        {
            var generalIniSection = new IniSection("General");
            generalIniSection.AddPropertyWithOptionalComment("fileVersion", fileVersion, "File version. Do not edit this.");
            generalIniSection.AddPropertyWithOptionalComment("fileType", "branches", "File type. Do not edit this.");

            return generalIniSection;
        }

        private static IniSection CreateBranchIniSection(string id)
        {
            var branchIniSection = new IniSection("Branch");
            branchIniSection.AddPropertyWithOptionalComment("name", $"some_branch_{id}", "Unique branch id");
            branchIniSection.AddPropertyWithOptionalComment("branchType", "1", "Channel = 0, SewerConnection = 1, Pipe = 2");
            branchIniSection.AddPropertyWithOptionalComment("isLengthCustom", "True", "branch length specified by user");
            branchIniSection.AddPropertyWithOptionalComment("sourceCompartmentName", $"some_source_compartment_{id}", "Source compartment name this sewer connection is beginning");
            branchIniSection.AddPropertyWithOptionalComment("targetCompartmentName", $"some_target_compartment_{id}", "Target compartment name this sewer connection is ending");

            return branchIniSection;
        }
    }
}