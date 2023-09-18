using System.Collections.Generic;
using System.Collections.ObjectModel;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    [TestFixture]
    public class CrossSectionDependentStructureParserBaseTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.Bridge;

        [Test]
        public void Constructor_IniSectionNull_ThrowsArgumentNullException()
        {
            // Setup
            IniSection iniSection = null;
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch channel = new Channel();

            // Call
            TestDelegate call = () => new TestCrossSectionDependentStructureParser(structureType, iniSection, 
                                                                                   crossSectionDefinitions, 
                                                                                   channel, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void Constructor_CrossSectionDefinitionsNull_ThrowsArgumentNullException()
        {
            // Setup
            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = null;
            IBranch channel = new Channel();

            // Call
            TestDelegate call = () => new TestCrossSectionDependentStructureParser(structureType, iniSection, 
                                                                                   crossSectionDefinitions,
                                                                                   channel, structuresFilename);
            
            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void Constructor_BranchNull_ThrowsArgumentNullException()
        {
            // Setup
            IniSection iniSection = StructureParserTestHelper.CreateStructureIniSection();
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch channel = null;

            // Call
            TestDelegate call = () => new TestCrossSectionDependentStructureParser(structureType, iniSection, 
                                                                                   crossSectionDefinitions,
                                                                                   channel, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var iniSection = StructureParserTestHelper.CreateStructureIniSection();
            var crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            var channel = new Channel();

            // Call
            var parser = new TestCrossSectionDependentStructureParser(structureType, iniSection, crossSectionDefinitions,
                                                                      channel, structuresFilename);

            // Assert
            Assert.That(parser, Is.InstanceOf<StructureParserBase>());
        }

        private class TestCrossSectionDependentStructureParser : CrossSectionDependentStructureParserBase
        {
            public TestCrossSectionDependentStructureParser(StructureType structureType, IniSection iniSection,
                                                            ICollection<ICrossSectionDefinition> crossSectionDefinitions, 
                                                            IBranch branch,
                                                            string structuresFilename) 
                : base(structureType, iniSection, crossSectionDefinitions, branch, structuresFilename) {}

            protected override IStructure1D Parse()
            {
                return new Bridge("ParsedCrossSectionDependentStructure");
            }
        }
    }
}