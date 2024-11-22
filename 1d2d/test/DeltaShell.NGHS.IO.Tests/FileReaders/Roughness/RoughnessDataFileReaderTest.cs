﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Roughness;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Roughness
{
    [TestFixture]
    public class RoughnessDataFileReaderTest
    {
        private MockFileSystem fileSystem;
        private RoughnessDataFileReader reader;

        private IHydroNetwork network;
        private IList<RoughnessSection> sections;
        
        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            reader = new RoughnessDataFileReader(fileSystem);

            network = new HydroNetwork();
            sections = new List<RoughnessSection>();
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new RoughnessDataFileReader(null));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ReadFile_FileNameIsNullOrEmpty_ThrowsArgumentException(string fileName)
        {
            Assert.Throws<ArgumentException>(() => reader.ReadFile(fileName, network, sections));
        }

        [Test]
        public void ReadFile_NetworkIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => reader.ReadFile("TestFile", null, sections));
        }

        [Test]
        public void ReadFile_SectionsIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => reader.ReadFile("TestFile", network, null));
        }

        [Test]
        public void ReadFile_FileDoesNotExist_ThrowsFileReadingException()
        {
            Assert.Throws<FileReadingException>(() => reader.ReadFile("TestFile", network, sections));
        }

        [Test]
        public void ReadFile_EmptyFile_ThrowsFileReadingException()
        {
            const string ini = "";

            Assert.Throws<FileReadingException>(() => ReadRoughnessFile(ini));
        }

        [Test]
        public void ReadFile_MissingGlobalHeader_ThrowsFileReadingException()
        {
            const string ini = @"
[General]
fileVersion   = 3.00    
fileType      = roughness";

            Assert.Throws<FileReadingException>(() => ReadRoughnessFile(ini));
        }

        [Test]
        public void ReadFile_ValidRoughnessFile_LogsInfoMessage()
        {
            const string ini = @"
[General]
fileVersion   = 3.00    
fileType      = roughness

[Global]
frictionId    = TestCrossSection";

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(() => ReadRoughnessFile(ini), Level.Info);

            Assert.That(messages, Has.One.EqualTo("Reading roughness data from roughness-main.ini."));
        }

        [Test]
        public void ReadFile_UnknownFrictionId_AddsNewRoughnessSection()
        {
            const string ini = @"
[General]
fileVersion   = 3.00    
fileType      = roughness

[Global]
frictionId    = SomeSection";

            ReadRoughnessFile(ini);

            Assert.That(sections, Has.Count.EqualTo(1));
            Assert.That(sections, Has.One.Matches<RoughnessSection>(s => s.Name == "SomeSection"));
        }

        [Test]
        [TestCase("Chezy", RoughnessType.Chezy)]
        [TestCase("TabbedDischarge", RoughnessType.Chezy)]
        [TestCase("TabbedLevel", RoughnessType.Chezy)]
        [TestCase("Chezy", RoughnessType.Chezy)]
        [TestCase("Manning", RoughnessType.Manning)]
        [TestCase("StricklerNikuradse", RoughnessType.StricklerNikuradse)]
        [TestCase("Strickler", RoughnessType.Strickler)]
        [TestCase("WhiteColebrook", RoughnessType.WhiteColebrook)]
        [TestCase("DeBosBijkerk", RoughnessType.DeBosBijkerk)]
        [TestCase("WallLawNikuradse", RoughnessType.WallLawNikuradse)]
        public void ReadFile_WithDifferentFrictionTypes_SetsExpectedRoughnessType(string frictionType, RoughnessType expectedRoughnessType)
        {
            var ini = $@"
[General]
fileVersion   = 3.00    
fileType      = roughness

[Global]
frictionId    = TestCrossSection
frictionType  = {frictionType}";

            RoughnessSection roughnessSection = CreateEmptyRoughnessSection();
            sections.Add(roughnessSection);

            ReadRoughnessFile(ini);

            Assert.That(expectedRoughnessType, Is.EqualTo(roughnessSection.GetDefaultRoughnessType()));
        }

        [Test]
        [TestCase(0.0d)]
        [TestCase(45.0d)]
        [TestCase(90.0d)]
        public void ReadFile_WithDifferentFrictionValues_SetsExpectedRoughnessValue(double frictionValue)
        {
            var ini = $@"
[General]
fileVersion   = 3.00    
fileType      = roughness

[Global]
frictionId    = TestCrossSection
frictionValue = {frictionValue}";

            RoughnessSection roughnessSection = CreateEmptyRoughnessSection();
            sections.Add(roughnessSection);

            ReadRoughnessFile(ini);

            Assert.That(frictionValue, Is.EqualTo(roughnessSection.GetDefaultRoughnessValue()));
        }
        
        [Test]
        public void ReadFile_WithMultiDifferentFrictionValues_SetsExpectedRoughnessValue()
        {
            var ini = @"
[General]
    fileVersion           = 3.00                
    fileType              = roughness           

[Global]
    frictionId            = TestCrossSection                
    frictionType          = Chezy               
    frictionValue         = 45.000              

[Branch]
    branchId              = Channel_1D_1_A      
    frictionType          = Chezy               
    functionType          = absDischarge        
    numLevels             = 3                   
    levels                = 50.000 100.000 200.000
    numLocations          = 2                   
    chainage              = 0.000000 1073.929741
    frictionValues        = 40.00000 40.00000
45.00000 45.00000
50.00000 50.00000";

            RoughnessSection roughnessSection = CreateEmptyRoughnessSection();
            sections.Add(roughnessSection);
            IBranch branch = AddBranch(roughnessSection);

            ReadRoughnessFile(ini); 

            var values = roughnessSection.FunctionOfQ(branch).GetValues<double>();
            Assert.That(values, Is.EqualTo(new double[] {40, 45, 50, 40, 45, 50}));
        }

        [Test]
        public void ReadFile_MissingFrictionTypeAndValue_PreservesRoughnessTypeAndValue()
        {
            const string ini = @"
[General]
fileVersion   = 3.00    
fileType      = roughness

[Global]
frictionId    = TestCrossSection";

            RoughnessSection roughnessSection = CreateEmptyRoughnessSection();
            roughnessSection.SetDefaultRoughnessType(RoughnessType.WhiteColebrook);
            roughnessSection.SetDefaultRoughnessValue(0.2);
            sections.Add(roughnessSection);

            ReadRoughnessFile(ini);

            Assert.That(roughnessSection.GetDefaultRoughnessType(), Is.EqualTo(RoughnessType.WhiteColebrook));
            Assert.That(roughnessSection.GetDefaultRoughnessValue(), Is.EqualTo(0.2));
        }

        private void ReadRoughnessFile(string ini)
        {
            const string fileName = "roughness-main.ini";
            
            fileSystem.AddFile(fileName, new MockFileData(ini));

            reader.ReadFile(fileName, network, sections);
        }

        private RoughnessSection CreateEmptyRoughnessSection()
        {
            CrossSectionSectionType crossSectionType = CreateCrossSectionSectionType();

            return new RoughnessSection(crossSectionType, network);
        }

        private static CrossSectionSectionType CreateCrossSectionSectionType()
        {
            return new CrossSectionSectionType { Name = "TestCrossSection" };
        }
        
        private static IBranch AddBranch(RoughnessSection roughnessSection)
        {
            var branch = Substitute.For<IBranch>();
            
            branch.Name = "Channel_1D_1_A";
            branch.Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2500, 0) });
            branch.Length = 2500;
            roughnessSection.Network.Branches.Add(branch);
            
            return branch;
        }
    }
}