using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    [TestFixture]
    public class StructureParserBaseTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.Pump;

        [Test]
        public void Constructor_IniSectionNull_ThrowsArgumentNullException()
        {
            // Setup
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => new TestStructureParser(structureType, null, branch, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void Constructor_BranchNull_ThrowsArgumentNullException()
        {
            // Setup
            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();

            // Call
            TestDelegate call = () => new TestStructureParser(structureType, iniSection, null, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_StructuresFilenameNull_ThrowsArgumentNullException()
        {
            // Setup
            IBranch branch = new Channel();
            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            
            // Call
            TestDelegate call = () => new TestStructureParser(structureType, iniSection, branch, null);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_InvalidStructuresType_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            StructureType structureType = (StructureType) 9999;
            IBranch branch = new Channel();
            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            
            // Call
            TestDelegate call = () => new TestStructureParser(structureType, iniSection, branch, structuresFilename);

            // Assert
            Assert.That(call, Throws.Exception.TypeOf<InvalidEnumArgumentException>());
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            IBranch branch = new Channel();
            
            // Call
            var parser = new TestStructureParser(structureType, iniSection, branch, structuresFilename);

            // Assert
            Assert.That(parser, Is.InstanceOf<IStructureParser>());
        }

        [Test]
        public void ParseStructure_PropertyWithMissingValue_ThrowsFileReadingException()
        {
            // Setup
            const string structureName = "TestStructureName";
            const int structureLineNumber = 123;
            const string propertyName = "PropertyWithMissingValue";
            const int propertyLineNumber = 456;
                         
            IniSection iniSection = CreateStructureIniSectionWithPropertyAndMissingValue(structureLineNumber,
                                                                                            structureName,
                                                                                            propertyName,
                                                                                            propertyLineNumber);
            IBranch branch = new Channel();
            
            var parser = new TestStructureParser(structureType, iniSection, branch, structuresFilename);

            // Call
            TestDelegate call = () => parser.ParseStructure();
            
            // Assert
            string expectedMessage = string.Format(Resources.StructureParserBase_Missing_structure_property,
                                                   parser.GetStructureType(),
                                                   structureName,
                                                   propertyName,
                                                   structuresFilename,
                                                   propertyLineNumber);
            Assert.That(call, Throws.Exception
                                    .TypeOf<FileReadingException>()
                                    .With.Message.EqualTo(expectedMessage));
        }

        [Test]
        public void ParseStructure_CallsAbstractParseMethod()
        {
            // Setup
            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            IBranch branch = new Channel();
            var parser = new TestStructureParser(structureType, iniSection, branch, structuresFilename);

            // Call
            IStructure1D parsedStructure = parser.ParseStructure();

            // Assert
            Assert.That(parsedStructure.Name, Is.EqualTo("ParsedStructure"));
        }

        private IniSection CreateStructureIniSectionWithPropertyAndMissingValue(int structureLineNumber,
                                                                                     string structureName,
                                                                                     string propertyName, 
                                                                                     int lineNumber)
        {
            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection(structureLineNumber);
            iniSection.AddProperty(StructureRegion.Id.Key, structureName, lineNumber);
            iniSection.AddProperty(propertyName, string.Empty, lineNumber);
            
            return iniSection;
        }

        private class TestStructureParser : StructureParserBase
        {
            public TestStructureParser(StructureType structureType,
                                       IniSection iniSection,
                                       IBranch branch,
                                       string structuresFilename) 
                : base(structureType, iniSection, branch, structuresFilename) {}
            
            protected override IStructure1D Parse()
            {
                return new Weir("ParsedStructure");
            }

            public string GetStructureType()
            {
                return StructureType.ToString();
            }
        }
    }
}