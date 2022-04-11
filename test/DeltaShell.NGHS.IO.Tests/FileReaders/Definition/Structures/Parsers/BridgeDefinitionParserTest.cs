using System.Collections.Generic;
using System.Collections.ObjectModel;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    [TestFixture]
    public class BridgeDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.Bridge;

        [Test]
        public void Constructor_CategoryNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = null;
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => new BridgeDefinitionParser(structureType, category, crossSectionDefinitions, 
                                                                 branch, structuresFilename);
            
            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void Constructor_CrossSectionDefinitionsNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = null;
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => new BridgeDefinitionParser(structureType, category, crossSectionDefinitions, 
                                                                 branch, structuresFilename);
            
            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void Constructor_BranchNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = null;

            // Call
            TestDelegate call = () => new BridgeDefinitionParser(structureType, category, crossSectionDefinitions, 
                                                                 branch, structuresFilename);
            
            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void Constructor_StructuresFilenameNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();

            // Call
            TestDelegate call = () => new BridgeDefinitionParser(structureType, category, crossSectionDefinitions, 
                                                                 branch, null);
            
            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var category = StructureParserTestHelper.CreateStructureCategory();
            var crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            var branch = new Channel();

            // Call
            var parser = new BridgeDefinitionParser(structureType, category, crossSectionDefinitions, branch, structuresFilename);

            // Assert
            Assert.That(parser, Is.InstanceOf<CrossSectionDependentStructureParserBase>());
        }
        
        [Test]
        public void ParseStructure_CorrectlyParsesBridge()
        {
            // Setup
            const string crossSectionName = "NameOfCrossSection";
            const string name = "NameOfStructure";
            const string longName = "LongNameOfStructure";
            const int chainage = 123;
            const double shift = 1.1;
            const string allowedFlowDir = "both";
            const FlowDirection expectedFlowDirection = FlowDirection.Both;
            const double length = 2.2;
            const double inletLossCoefficient = 3.3;
            const double outletLossCoefficient = 4.4;
            const double pillarWidth = 5.5;
            const double shapeFactor = 6.6;
            const string frictionType = "Chezy";
            const Friction expectedFrictionType = Friction.Chezy;
            const double friction = 7.7;
            
            var crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            crossSectionDefinitions.Add(new CrossSectionDefinitionStandard()
            {
                Name = crossSectionName
            });

            IBranch branch = new Channel() { Length = 999 };
            
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, name);
            category.AddProperty(StructureRegion.Name.Key, longName);
            category.AddProperty(StructureRegion.Chainage.Key, chainage);
            category.AddProperty(StructureRegion.CsDefId.Key, crossSectionName);
            category.AddProperty(StructureRegion.Shift.Key, shift);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, allowedFlowDir);
            category.AddProperty(StructureRegion.Length.Key, length);
            category.AddProperty(StructureRegion.InletLossCoeff.Key, inletLossCoefficient);
            category.AddProperty(StructureRegion.OutletLossCoeff.Key, outletLossCoefficient);
            category.AddProperty(StructureRegion.PillarWidth.Key, pillarWidth);
            category.AddProperty(StructureRegion.FormFactor.Key, shapeFactor);
            category.AddProperty(StructureRegion.FrictionType.Key, frictionType);
            category.AddProperty(StructureRegion.Friction.Key, friction);
            
            var parser = new BridgeDefinitionParser(structureType, category, crossSectionDefinitions, branch, structuresFilename);

            // Call
            IStructure1D parsedStructure = parser.ParseStructure();

            // Assert
            Assert.That(parsedStructure, Is.TypeOf<Bridge>());
            
            var bridge = (Bridge)parsedStructure;
            Assert.That(bridge.Name, Is.EqualTo(name));
            Assert.That(bridge.LongName, Is.EqualTo(longName));
            Assert.That(bridge.Branch, Is.EqualTo(branch));
            Assert.That(bridge.Chainage, Is.EqualTo(chainage));
            Assert.That(bridge.Shift, Is.EqualTo(shift));
            Assert.That(bridge.FlowDirection, Is.EqualTo(expectedFlowDirection));
            Assert.That(bridge.BridgeType, Is.EqualTo(BridgeType.Rectangle));
            Assert.That(bridge.Length, Is.EqualTo(length));
            Assert.That(bridge.InletLossCoefficient, Is.EqualTo(inletLossCoefficient));
            Assert.That(bridge.OutletLossCoefficient, Is.EqualTo(outletLossCoefficient));
            Assert.That(bridge.PillarWidth, Is.EqualTo(pillarWidth));
            Assert.That(bridge.ShapeFactor, Is.EqualTo(shapeFactor));
            Assert.That(bridge.FrictionDataType, Is.EqualTo(expectedFrictionType));
            Assert.That(bridge.Friction, Is.EqualTo(friction));
        }
        
        [Test]
        public void ParseStructure_NoCrossSectionDefinitions_CorrectlyParsesBridgeUsingDefaultValues()
        {
            // Setup
            const string crossSectionName = "NameOfCrossSection";
            const string name = "NameOfStructure";
            const string longName = "LongNameOfStructure";
            const int chainage = 123;
            const double shift = 1.1;
            const string allowedFlowDir = "both";
            const FlowDirection expectedFlowDirection = FlowDirection.Both;
            const double length = 2.2;
            const double inletLossCoefficient = 3.3;
            const double outletLossCoefficient = 4.4;
            const double pillarWidth = 5.5;
            const double shapeFactor = 6.6;
            const string frictionType = "Chezy";
            const Friction expectedFrictionType = Friction.Chezy;
            const double friction = 7.7;

            const double expectedWidth = 50; // default value
            const double expectedHeight = 3; // default value
            
            var crossSectionDefinitions = new Collection<ICrossSectionDefinition>();

            IBranch branch = new Channel() { Length = 999 };
            
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, name);
            category.AddProperty(StructureRegion.Name.Key, longName);
            category.AddProperty(StructureRegion.Chainage.Key, chainage);
            category.AddProperty(StructureRegion.CsDefId.Key, crossSectionName);
            category.AddProperty(StructureRegion.Shift.Key, shift);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, allowedFlowDir);
            category.AddProperty(StructureRegion.Length.Key, length);
            category.AddProperty(StructureRegion.InletLossCoeff.Key, inletLossCoefficient);
            category.AddProperty(StructureRegion.OutletLossCoeff.Key, outletLossCoefficient);
            category.AddProperty(StructureRegion.PillarWidth.Key, pillarWidth);
            category.AddProperty(StructureRegion.FormFactor.Key, shapeFactor);
            category.AddProperty(StructureRegion.FrictionType.Key, frictionType);
            category.AddProperty(StructureRegion.Friction.Key, friction);
            
            var parser = new BridgeDefinitionParser(structureType, category, crossSectionDefinitions, 
                                                    branch, structuresFilename);

            // Call
            IStructure1D parsedStructure = parser.ParseStructure();

            // Assert
            Assert.That(parsedStructure, Is.TypeOf<Bridge>());
            
            var bridge = (Bridge)parsedStructure;
            Assert.That(bridge.Name, Is.EqualTo(name));
            Assert.That(bridge.LongName, Is.EqualTo(longName));
            Assert.That(bridge.Branch, Is.EqualTo(branch));
            Assert.That(bridge.Chainage, Is.EqualTo(chainage));
            Assert.That(bridge.Shift, Is.EqualTo(shift));
            Assert.That(bridge.FlowDirection, Is.EqualTo(expectedFlowDirection));
            Assert.That(bridge.BridgeType, Is.EqualTo(BridgeType.Rectangle));
            Assert.That(bridge.Length, Is.EqualTo(length));
            Assert.That(bridge.InletLossCoefficient, Is.EqualTo(inletLossCoefficient));
            Assert.That(bridge.OutletLossCoefficient, Is.EqualTo(outletLossCoefficient));
            Assert.That(bridge.PillarWidth, Is.EqualTo(pillarWidth));
            Assert.That(bridge.ShapeFactor, Is.EqualTo(shapeFactor));
            Assert.That(bridge.FrictionDataType, Is.EqualTo(expectedFrictionType));
            Assert.That(bridge.Friction, Is.EqualTo(friction));
            Assert.That(bridge.Width, Is.EqualTo(expectedWidth));
            Assert.That(bridge.Height, Is.EqualTo(expectedHeight));
        }
    }
}