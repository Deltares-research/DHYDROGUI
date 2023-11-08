using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using GeoAPI.Extensions.Networks;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Roughness
{
    [TestFixture]
    public class RoughnessDataFileWriterTest
    {
        private MockFileSystem fileSystem;
        private RoughnessDataFileWriter writer;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            writer = new RoughnessDataFileWriter(fileSystem);
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new RoughnessDataFileWriter(null));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void WriteFile_FileNameIsNullOrEmpty_ThrowsArgumentException(string fileName)
        {
            Assert.Throws<ArgumentException>(() => writer.WriteFile(fileName, CreateEmptyRoughnessSection()));
        }

        [Test]
        public void WriteFile_RoughnessSectionIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => writer.WriteFile("TestFile", null));
        }

        [Test]
        public void WriteFile_WithRoughnessSectionAndEmptyNetwork_LogsInfoMessage()
        {
            RoughnessSection roughnessSection = CreateEmptyRoughnessSection();

            IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(() => WriteRoughnessFile(roughnessSection), Level.Info);

            Assert.That(messages, Has.One.EqualTo("Writing roughness data to roughness-main.ini."));
        }

        [Test]
        public void WriteFile_WithRoughnessSectionAndEmptyNetwork_WritesGeneralAndGlobalSections()
        {
            RoughnessSection roughnessSection = CreateEmptyRoughnessSection();

            string ini = WriteRoughnessFile(roughnessSection);

            Assert.That(ini, Is.EqualTo(@"[General]
    fileVersion           = 3.00                
    fileType              = roughness           

[Global]
    frictionId            = TestCrossSection    
    frictionType          = Chezy               
    frictionValue         = 45.000              

"));
        }

        [Test]
        public void WriteFile_WithReverseRoughnessSectionAndEmptyNetwork_WritesGeneralAndGlobalSections()
        {
            RoughnessSection roughnessSection = CreateEmptyReverseRoughnessSection();

            string ini = WriteRoughnessFile(roughnessSection);

            Assert.That(ini, Is.EqualTo(@"[General]
    fileVersion           = 3.00                
    fileType              = roughness           

[Global]
    frictionId            = TestCrossSection    

"));
        }

        [Test]
        [TestCase(RoughnessType.Chezy, "Chezy")]
        [TestCase(RoughnessType.Manning, "Manning")]
        [TestCase(RoughnessType.StricklerNikuradse, "StricklerNikuradse")]
        [TestCase(RoughnessType.Strickler, "Strickler")]
        [TestCase(RoughnessType.WhiteColebrook, "WhiteColebrook")]
        [TestCase(RoughnessType.DeBosBijkerk, "deBosBijkerk")]
        [TestCase(RoughnessType.WallLawNikuradse, "wallLawNikuradse")]
        public void WriteFile_WithDifferentRoughnessTypes_WritesExpectedFrictionType(RoughnessType roughnessType, string expectedFrictionType)
        {
            RoughnessSection roughnessSection = CreateEmptyRoughnessSection();
            roughnessSection.SetDefaultRoughnessType(roughnessType);

            string ini = WriteRoughnessFile(roughnessSection);

            Assert.That(ini, Does.Contain($"frictionType          = {expectedFrictionType}"));
        }

        [Test]
        [TestCase(0.0d)]
        [TestCase(45.0d)]
        [TestCase(90.0d)]
        public void WriteFile_WithDifferentRoughnessValues_WritesExpectedFrictionValue(double roughnessValue)
        {
            RoughnessSection roughnessSection = CreateEmptyRoughnessSection();
            roughnessSection.SetDefaultRoughnessValue(roughnessValue);

            string ini = WriteRoughnessFile(roughnessSection);

            Assert.That(ini, Does.Contain(FormattableString.Invariant($"frictionValue         = {roughnessValue:0.000}")));
        }

        private string WriteRoughnessFile(RoughnessSection roughnessSection)
        {
            const string fileName = "roughness-main.ini";

            writer.WriteFile(fileName, roughnessSection);
            MockFileData fileData = fileSystem.GetFile(fileName);

            return fileData.TextContents;
        }

        private static RoughnessSection CreateEmptyRoughnessSection()
        {
            INetwork network = CreateEmptyNetwork();
            CrossSectionSectionType crossSectionType = CreateCrossSectionSectionType();

            return new RoughnessSection(crossSectionType, network);
        }

        private static ReverseRoughnessSection CreateEmptyReverseRoughnessSection()
        {
            RoughnessSection roughnessSection = CreateEmptyRoughnessSection();

            return new ReverseRoughnessSection(roughnessSection);
        }

        private static INetwork CreateEmptyNetwork()
        {
            return new HydroNetwork { Name = "Network" };
        }

        private static CrossSectionSectionType CreateCrossSectionSectionType()
        {
            return new CrossSectionSectionType { Name = "TestCrossSection" };
        }
    }
}