using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    [TestFixture]
    public class CulvertDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";
        private const StructureType structureType = StructureType.Culvert;
        private readonly DateTime referenceDateTime = new DateTime(2022, 5, 5);

        private static IEnumerable<TestCaseData> ConstructorParameterNullData()
        {
            ITimeSeriesFileReader specificTimeSeriesFileReader = Substitute.For<ITimeSeriesFileReader>();
            IDelftIniCategory category = Substitute.For<IDelftIniCategory>();
            ICrossSectionDefinition[] crossSectionDefinitions = {};
            var branch = Substitute.For<IBranch>();

            yield return new TestCaseData(null, category, crossSectionDefinitions, branch, structuresFilename, "fileReader");
            yield return new TestCaseData(specificTimeSeriesFileReader, null, crossSectionDefinitions, branch, structuresFilename, "category");
            yield return new TestCaseData(specificTimeSeriesFileReader, category, null, branch, structuresFilename, "crossSectionDefinitions");
            yield return new TestCaseData(specificTimeSeriesFileReader, category, crossSectionDefinitions, null, structuresFilename, "branch");
            yield return new TestCaseData(specificTimeSeriesFileReader, category, crossSectionDefinitions, branch, null, "structuresFilename");
        }

        [Test]
        [TestCaseSource(nameof(ConstructorParameterNullData))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(ITimeSeriesFileReader specificTimeFileReader,
                                                                          IDelftIniCategory category,
                                                                          ICollection<ICrossSectionDefinition> crossSectionDefinitions,
                                                                          IBranch branch, 
                                                                          string structuresFilePath,
                                                                          string expectedParameterName)
        {
            void Call() => new CulvertDefinitionParser(specificTimeFileReader,
                                                       structureType, 
                                                       category, 
                                                       crossSectionDefinitions, 
                                                       branch, 
                                                       structuresFilePath, 
                                                       referenceDateTime);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var category = StructureParserTestHelper.CreateStructureCategory();
            var crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            var branch = new Channel();

            // Call
            var parser = new CulvertDefinitionParser(Substitute.For<ITimeSeriesFileReader>(),
                                                     structureType, 
                                                     category, 
                                                     crossSectionDefinitions, 
                                                     branch, 
                                                     structuresFilename, 
                                                     referenceDateTime);

            // Assert
            Assert.That(parser, Is.InstanceOf<CrossSectionDependentStructureParserBase>());
        }

        [Test]
        public void ParseStructure_ReadsTimStructuresCorrectly()
        {
            // Setup
            const string valveOpeningHeightTimeSeries = "valve_opening.tim";
            const string name = "NameOfStructure";
            const string longName = "LongNameOfStructure";
            const int chainage = 123;
            const string allowedFlowDir = "both";
            const string frictionType = "Chezy";

            var crossSectionDefinitions = new Collection<ICrossSectionDefinition>();

            IBranch branch = new Channel() { Length = 999 };
            
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, name);
            category.AddProperty(StructureRegion.Name.Key, longName);
            category.AddProperty(StructureRegion.Chainage.Key, chainage);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, allowedFlowDir);
            category.AddProperty(StructureRegion.BedFrictionType.Key, frictionType);
            category.AddProperty(StructureRegion.IniValveOpen.Key, valveOpeningHeightTimeSeries);

            var reader = Substitute.For<ITimeSeriesFileReader>();
            reader.IsTimeSeriesProperty("").ReturnsForAnyArgs(true);

            var parser = new CulvertDefinitionParser(reader,
                                                     structureType, 
                                                     category, 
                                                     crossSectionDefinitions, 
                                                     branch, 
                                                     structuresFilename, 
                                                     referenceDateTime);

            // Call
            IStructure1D _ = parser.ParseStructure();

            // Assert
            reader.Received(1).Read(Arg.Any<string>(),valveOpeningHeightTimeSeries, Arg.Any<IStructureTimeSeries>(), referenceDateTime);
        }

        [Test]
        [TestCase(CrossSectionStandardShapeType.Rectangle, CulvertGeometryType.Rectangle)]
        [TestCase(CrossSectionStandardShapeType.Arch, CulvertGeometryType.Arch)]
        [TestCase(CrossSectionStandardShapeType.Cunette, CulvertGeometryType.Cunette)]
        [TestCase(CrossSectionStandardShapeType.Elliptical, CulvertGeometryType.Ellipse)]
        [TestCase(CrossSectionStandardShapeType.SteelCunette, CulvertGeometryType.SteelCunette)]
        [TestCase(CrossSectionStandardShapeType.Egg, CulvertGeometryType.Egg)]
        [TestCase(CrossSectionStandardShapeType.Circle, CulvertGeometryType.Round)]
        [TestCase(CrossSectionStandardShapeType.InvertedEgg, CulvertGeometryType.InvertedEgg)]
        [TestCase(CrossSectionStandardShapeType.UShape, CulvertGeometryType.UShape)]
        public void ParseStructure_DifferentCrossSectionStandardShapes_CorrectlyParsesCulvert(
            CrossSectionStandardShapeType crossSectionShapeType,
            CulvertGeometryType expectedCulvertGeometryType)
        {
            // Setup
            const string crossSectionName = "NameOfCrossSection";
            const string name = "NameOfStructure";
            const string longName = "LongNameOfStructure";
            const int chainage = 123;
            const string allowedFlowDir = "both";
            const FlowDirection expectedFlowDirection = FlowDirection.Both;
            const double inletLevel = 1.1;
            const double outletLevel = 2.2;
            const double length = 3.3;
            const double inletLossCoefficient = 4.4;
            const double outletLossCoefficient = 5.5;
            const bool isGated = true;
            const double bendLossCoefficient = 6.6;
            const string frictionType = "Chezy";
            const CulvertFrictionType expectedFrictionType = CulvertFrictionType.Chezy;
            const double friction = 7.7;
            const double gateInitialOpening = 8.8;
            const int numLossCoefficient = 3;
            double[] relativeOpening = { 1, 2, 3 };
            double[] lossCoefficient = { 4, 5, 6 };
            const string subType = "invertedSiphon";
            const CulvertType expectedSubtype = CulvertType.InvertedSiphon;

            var crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            crossSectionDefinitions.Add(new CrossSectionDefinitionStandard()
            {
                Name = crossSectionName,
                ShapeType = crossSectionShapeType
            });

            IBranch branch = new Channel() { Length = 999 };
            
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, name);
            category.AddProperty(StructureRegion.Name.Key, longName);
            category.AddProperty(StructureRegion.Chainage.Key, chainage);
            category.AddProperty(StructureRegion.CsDefId.Key, crossSectionName);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, allowedFlowDir);
            category.AddProperty(StructureRegion.LeftLevel.Key, inletLevel);
            category.AddProperty(StructureRegion.RightLevel.Key, outletLevel);
            category.AddProperty(StructureRegion.Length.Key, length);
            category.AddProperty(StructureRegion.InletLossCoeff.Key, inletLossCoefficient);
            category.AddProperty(StructureRegion.OutletLossCoeff.Key, outletLossCoefficient);
            category.AddProperty(StructureRegion.ValveOnOff.Key, isGated ? "1" : "0");
            category.AddProperty(StructureRegion.BendLossCoef.Key, bendLossCoefficient);
            category.AddProperty(StructureRegion.BedFrictionType.Key, frictionType);
            category.AddProperty(StructureRegion.BedFriction.Key, friction);
            category.AddProperty(StructureRegion.IniValveOpen.Key, gateInitialOpening);
            category.AddProperty(StructureRegion.LossCoeffCount.Key, numLossCoefficient);
            category.AddProperty(StructureRegion.RelativeOpening.Key, string.Join(", ", relativeOpening));
            category.AddProperty(StructureRegion.LossCoefficient.Key, string.Join(", ", lossCoefficient));
            category.AddProperty(StructureRegion.SubType.Key, subType);
            
            var fileReaderSubstitute = Substitute.For<ITimeSeriesFileReader>();

            var parser = new CulvertDefinitionParser(fileReaderSubstitute,
                                                     structureType, 
                                                     category, 
                                                     crossSectionDefinitions, 
                                                     branch, 
                                                     structuresFilename, 
                                                     referenceDateTime);

            // Call
            IStructure1D parsedStructure = parser.ParseStructure();

            // Assert
            Assert.That(parsedStructure, Is.TypeOf<Culvert>());
            
            var culvert = (Culvert)parsedStructure;
            Assert.That(culvert.Name, Is.EqualTo(name));
            Assert.That(culvert.LongName, Is.EqualTo(longName));
            Assert.That(culvert.Branch, Is.EqualTo(branch));
            Assert.That(culvert.Chainage, Is.EqualTo(chainage));
            Assert.That(culvert.GeometryType, Is.EqualTo(expectedCulvertGeometryType));
            Assert.That(culvert.FlowDirection, Is.EqualTo(expectedFlowDirection));
            Assert.That(culvert.InletLevel, Is.EqualTo(inletLevel));
            Assert.That(culvert.OutletLevel, Is.EqualTo(outletLevel));
            Assert.That(culvert.Length, Is.EqualTo(length));
            Assert.That(culvert.InletLossCoefficient, Is.EqualTo(inletLossCoefficient));
            Assert.That(culvert.OutletLossCoefficient, Is.EqualTo(outletLossCoefficient));
            Assert.That(culvert.IsGated, Is.EqualTo(isGated));
            Assert.That(culvert.BendLossCoefficient, Is.EqualTo(bendLossCoefficient));
            Assert.That(culvert.FrictionType, Is.EqualTo(expectedFrictionType));
            Assert.That(culvert.Friction, Is.EqualTo(friction));
            Assert.That(culvert.GateInitialOpening, Is.EqualTo(gateInitialOpening));
            Assert.That(culvert.GateOpeningLossCoefficientFunction.GetValues<double>(), Is.EqualTo(lossCoefficient));
            Assert.That(culvert.GateOpeningLossCoefficientFunction.Arguments[0].GetValues<double>(), Is.EqualTo(relativeOpening));
            Assert.That(culvert.CulvertType, Is.EqualTo(expectedSubtype));
        }
        
        [Test]
        public void ParseStructure_NoCrossSectionDefinitions_CorrectlyParsesCulvertWithTabulatedGeometryType()
        {
            // Setup
            const string name = "NameOfStructure";
            const string longName = "LongNameOfStructure";
            const int chainage = 123;
            const string allowedFlowDir = "both";
            const string frictionType = "Chezy";

            var crossSectionDefinitions = new Collection<ICrossSectionDefinition>();

            IBranch branch = new Channel() { Length = 999 };
            
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, name);
            category.AddProperty(StructureRegion.Name.Key, longName);
            category.AddProperty(StructureRegion.Chainage.Key, chainage);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, allowedFlowDir);
            category.AddProperty(StructureRegion.BedFrictionType.Key, frictionType);

            var parser = new CulvertDefinitionParser(Substitute.For<ITimeSeriesFileReader>(),
                                                     structureType, 
                                                     category, 
                                                     crossSectionDefinitions, 
                                                     branch, 
                                                     structuresFilename, 
                                                     referenceDateTime);

            // Call
            IStructure1D parsedStructure = parser.ParseStructure();

            // Assert
            Assert.That(parsedStructure, Is.TypeOf<Culvert>());
            
            var culvert = (Culvert)parsedStructure;
            Assert.That(culvert.Name, Is.EqualTo(name));
            Assert.That(culvert.LongName, Is.EqualTo(longName));
            Assert.That(culvert.Branch, Is.EqualTo(branch));
            Assert.That(culvert.Chainage, Is.EqualTo(chainage));
            Assert.That(culvert.GeometryType, Is.EqualTo(CulvertGeometryType.Tabulated));
        }
        
        [Test]
        [TestCaseSource(nameof(CulvertDimensionsBasedOnProfileTestCases))]
        public void ParseStructure_CorrectlySetsCulvertDimensionsBasedOnProfile(
            CrossSectionStandardShapeType crossSectionShapeType,
            Action<ICulvert, ICrossSectionDefinition> assertCulvertDimensions)
        {
            // Setup
            const string crossSectionName = "NameOfCrossSection";
            const string name = "NameOfStructure";
            const string longName = "LongNameOfStructure";
            const int chainage = 123;
            const string allowedFlowDir = "both";
            const string frictionType = "Chezy";

            var crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            var crossSectionDefinition = new CrossSectionDefinitionStandard()
            {
                Name = crossSectionName,
                ShapeType = crossSectionShapeType
            };
            crossSectionDefinitions.Add(crossSectionDefinition);

            IBranch branch = new Channel() { Length = 999 };
            
            IDelftIniCategory category = StructureParserTestHelper.CreateStructureCategory();
            category.AddProperty(StructureRegion.Id.Key, name);
            category.AddProperty(StructureRegion.Name.Key, longName);
            category.AddProperty(StructureRegion.Chainage.Key, chainage);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, allowedFlowDir);
            category.AddProperty(StructureRegion.BedFrictionType.Key, frictionType);
            category.AddProperty(StructureRegion.CsDefId.Key, crossSectionName);

            var parser = new CulvertDefinitionParser(Substitute.For<ITimeSeriesFileReader>(), 
                                                     structureType, 
                                                     category, 
                                                     crossSectionDefinitions, 
                                                     branch, 
                                                     structuresFilename, 
                                                     referenceDateTime);

            // Call
            IStructure1D parsedStructure = parser.ParseStructure();

            // Assert
            Assert.That(parsedStructure, Is.TypeOf<Culvert>());
            
            var culvert = (Culvert)parsedStructure;
            Assert.That(culvert.Name, Is.EqualTo(name));
            Assert.That(culvert.LongName, Is.EqualTo(longName));
            Assert.That(culvert.Branch, Is.EqualTo(branch));
            Assert.That(culvert.Chainage, Is.EqualTo(chainage));

            assertCulvertDimensions(culvert, crossSectionDefinition);
        }
        
        private static IEnumerable<TestCaseData> CulvertDimensionsBasedOnProfileTestCases()
        {
            yield return new TestCaseData(CrossSectionStandardShapeType.Circle, 
                                          new Action<ICulvert, ICrossSectionDefinition>(AssertCulvertDimensionsForCircle));
            yield return new TestCaseData(CrossSectionStandardShapeType.Rectangle, 
                                          new Action<ICulvert, ICrossSectionDefinition>(AssertCulvertDimensionsForRectangle));
            yield return new TestCaseData(CrossSectionStandardShapeType.Egg,
                                          new Action<ICulvert, ICrossSectionDefinition>(AssertCulvertDimensionsForEgg));
            yield return new TestCaseData(CrossSectionStandardShapeType.InvertedEgg,
                                          new Action<ICulvert, ICrossSectionDefinition>(AssertCulvertDimensionsForInvertedEgg));
            yield return new TestCaseData(CrossSectionStandardShapeType.Cunette,
                                          new Action<ICulvert, ICrossSectionDefinition>(AssertCulvertDimensionsForCunette));
            yield return new TestCaseData(CrossSectionStandardShapeType.Elliptical, 
                                          new Action<ICulvert, ICrossSectionDefinition>(AssertCulvertDimensionsForEllipse));
            yield return new TestCaseData(CrossSectionStandardShapeType.Arch, 
                                          new Action<ICulvert, ICrossSectionDefinition>(AssertCulvertDimensionsForArch));
            yield return new TestCaseData(CrossSectionStandardShapeType.UShape, 
                                          new Action<ICulvert, ICrossSectionDefinition>(AssertCulvertDimensionsForUShape));
            yield return new TestCaseData(CrossSectionStandardShapeType.SteelCunette, 
                                          new Action<ICulvert, ICrossSectionDefinition>(AssertCulvertDimensionsForSteelCunette));
        }

        private static void AssertCulvertDimensionsForRectangle(ICulvert culvert, ICrossSectionDefinition definition)
        {
            var stdDef = definition as CrossSectionDefinitionStandard;
            Assert.That(stdDef, Is.Not.Null);
                
            var heightbase = stdDef.Shape as CrossSectionStandardShapeWidthHeightBase;
            Assert.That(heightbase, Is.Not.Null);
            Assert.That(culvert.Width, Is.EqualTo(heightbase.Width));
            Assert.That(culvert.Height, Is.EqualTo(heightbase.Height));

            bool closed = (heightbase as ICrossSectionStandardShapeOpenClosed)?.Closed ?? false;
            Assert.That(culvert.Closed, Is.EqualTo(closed));
        }
        
        private static void AssertCulvertDimensionsForCircle(ICulvert culvert, ICrossSectionDefinition definition)
        {
            var stdDef = definition as CrossSectionDefinitionStandard;
            Assert.That(stdDef, Is.Not.Null);
                
            var round = stdDef.Shape as CrossSectionStandardShapeCircle;
            Assert.That(round, Is.Not.Null);
            Assert.That(culvert.Diameter, Is.EqualTo(round.Diameter));
        }
        
        private static void AssertCulvertDimensionsForEgg(ICulvert culvert, ICrossSectionDefinition definition)
        {
            var stdDef = definition as CrossSectionDefinitionStandard;
            Assert.That(stdDef, Is.Not.Null);
                
            var heightbase = stdDef.Shape as CrossSectionStandardShapeWidthHeightBase;
            Assert.That(heightbase, Is.Not.Null);
            Assert.That(culvert.Width, Is.EqualTo(heightbase.Width));
            Assert.That(culvert.Height, Is.EqualTo(heightbase.Height));
        }
        
        private static void AssertCulvertDimensionsForInvertedEgg(ICulvert culvert, ICrossSectionDefinition definition)
        {
            // same as Egg
            AssertCulvertDimensionsForEgg(culvert, definition);
        }
        
        private static void AssertCulvertDimensionsForCunette(ICulvert culvert, ICrossSectionDefinition definition)
        {
            // same as Egg
            AssertCulvertDimensionsForEgg(culvert, definition);
        }
        
        private static void AssertCulvertDimensionsForEllipse(ICulvert culvert, ICrossSectionDefinition definition)
        {
            // same as Egg
            AssertCulvertDimensionsForEgg(culvert, definition);
        }
        
        private static void AssertCulvertDimensionsForArch(ICulvert culvert, ICrossSectionDefinition definition)
        {
            var stdDef = definition as CrossSectionDefinitionStandard;
            Assert.That(stdDef, Is.Not.Null);
                
            var arch = stdDef.Shape as CrossSectionStandardShapeArch;
            Assert.That(arch, Is.Not.Null);
            Assert.That(culvert.Width, Is.EqualTo(arch.Width));
            Assert.That(culvert.Height, Is.EqualTo(arch.Height));
            Assert.That(culvert.ArcHeight, Is.EqualTo(arch.ArcHeight));
        }

        private static void AssertCulvertDimensionsForUShape(ICulvert culvert, ICrossSectionDefinition definition)
        {
            // same as Arch
            AssertCulvertDimensionsForArch(culvert, definition);
        }

        private static void AssertCulvertDimensionsForSteelCunette(ICulvert culvert, ICrossSectionDefinition definition)
        {
            var stdDef = definition as CrossSectionDefinitionStandard;
            Assert.That(stdDef, Is.Not.Null);
                
            var steelCunette = stdDef.Shape as CrossSectionStandardShapeSteelCunette;
            Assert.That(steelCunette, Is.Not.Null);
            Assert.That(culvert.Angle, Is.EqualTo(steelCunette.AngleA));
            Assert.That(culvert.Angle1, Is.EqualTo(steelCunette.AngleA1));
            Assert.That(culvert.Height, Is.EqualTo(steelCunette.Height));
            Assert.That(culvert.Radius, Is.EqualTo(steelCunette.RadiusR));
            Assert.That(culvert.Radius1, Is.EqualTo(steelCunette.RadiusR1));
            Assert.That(culvert.Radius2, Is.EqualTo(steelCunette.RadiusR2));
            Assert.That(culvert.Radius3, Is.EqualTo(steelCunette.RadiusR3));
        }
    }
}