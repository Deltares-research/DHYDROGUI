using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.TestUtils;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.CrossSectionDefinition
{
    [TestFixture]
    class CrossSectionDefinitionFileWritersTest
    {
        private IHydroNetwork network;

        [SetUp]
        public void SetUp()
        {
            network = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_MultipleTypes()
        {
            var relativePathCrossSectionDefinitionsExpectedFile = TestHelper.GetTestFilePath(@"FileWriters/CrossSectionDefinitions_expected.txt");
            
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionYz(branch, 20.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionXyz(branch, 80.0,
                               new[] { 585.0, 610.0, 635.0, 660.0, 685.0, 710.0 },
                               new[] { 950.0, 910.0, 870.0, 830.0, 790.0, 750.0 },
                               new[] { 10.0, 6.5, 2.5, 2.5, 6.5, 10.0 });

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionZw(branch, 30.0, -2.0, 100.0, 200.0, 0.5);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionRectangle(branch, 30.0, 100.0, 80.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionElliptical(branch, 30.0, 100.0, 80.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionCircle(branch, 30.0, 200.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionEgg(branch, 30.0, 100.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionArch(branch, 30.0, 100.0, 200.0, 150.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionCunette(branch, 30.0, 100.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionSteelCunette(branch, 30.0, 100.0, 50.0, 100.0, 50.0, 100.0, 45.0, 135.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionTrapezium(branch, 30.0, 100.0, 200.0, 150.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            string errorMessage;

            if (FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions != null)
            {
                var relativePathActualFile = Path.Combine(FileWriterTestHelper.RelativeTargetDirectory, Path.GetFileName(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions));
                var filesAreIdentical = FileComparer.Compare(relativePathCrossSectionDefinitionsExpectedFile, relativePathActualFile, out errorMessage, true);
                Assert.IsTrue(filesAreIdentical,
                    $"Generated CrossSectionDefinitions file does not match template!{Environment.NewLine}{errorMessage}");
            }
        }
        

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Yz()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionYz(branch, 20.0);
            FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.YZ, 80.0, 1.5, true);//+ a shift to check if z values ARE NOT shifted
            
            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);
            
            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(10, content.Properties.Count());
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_1", idValue);
            
            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Yz, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("50.000", thalwegValue);

            var yzCountValue = content.GetPropertyValue(DefinitionPropertySettings.YZCount.Key);
            Assert.AreEqual("6", yzCountValue);

            var yCoordsValue = content.GetPropertyValue(DefinitionPropertySettings.YCoors.Key);
            Assert.AreEqual("0.00000 22.22222 33.33333 66.66667 77.77778 100.00000", yCoordsValue);

            var zCoordsValue = content.GetPropertyValue(DefinitionPropertySettings.ZCoors.Key);
            Assert.AreEqual("0.00000 0.00000 -10.00000 -10.00000 0.00000 0.00000", zCoordsValue);

            var sectionCountValue = content.GetPropertyValue(DefinitionPropertySettings.SectionCount.Key);
            Assert.AreEqual("3", sectionCountValue);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FrictionIds.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FrictionPositions.Key));

            content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(10, content.Properties.Count());
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Yz, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("50.000", thalwegValue);

            yzCountValue = content.GetPropertyValue(DefinitionPropertySettings.YZCount.Key);
            Assert.AreEqual("6", yzCountValue);

            yCoordsValue = content.GetPropertyValue(DefinitionPropertySettings.YCoors.Key);
            Assert.AreEqual("0.00000 22.22222 33.33333 66.66667 77.77778 100.00000", yCoordsValue);

            zCoordsValue = content.GetPropertyValue(DefinitionPropertySettings.ZCoors.Key);
            Assert.AreEqual("0.00000 0.00000 -10.00000 -10.00000 0.00000 0.00000", zCoordsValue);

            sectionCountValue = content.GetPropertyValue(DefinitionPropertySettings.SectionCount.Key);
            Assert.AreEqual("3", sectionCountValue);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FrictionIds.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FrictionPositions.Key));
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Xyz()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionXyz(branch, 20.0, 
                               new[] { 585.0, 610.0, 635.0, 660.0, 685.0, 710.0 }, 
                               new[] { 950.0, 910.0, 870.0, 830.0, 790.0, 750.0 }, 
                               new[] { 10.0, 6.5, 2.5, 2.5, 6.5, 10.0 });

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionXyz(branch, 60.0,
                               new[] { 485.0, 510.0, 535.0, 560.0, 585.0, 610.0 },
                               new[] { 1050.0, 1010.0, 970.0, 930.0, 890.0, 850.0 },
                               new[] { 10.5, 7.0, 3.0, 3.0, 7.0, 10.5 });

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(11, content.Properties.Count());
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Xyz, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            var xyzCountValue = content.GetPropertyValue(DefinitionPropertySettings.XYZCount.Key);
            Assert.AreEqual("6", xyzCountValue);

            var xCoorsValue = content.GetPropertyValue(DefinitionPropertySettings.XCoors.Key);
            Assert.AreEqual("585.00000 610.00000 635.00000 660.00000 685.00000 710.00000", xCoorsValue);

            var yCoorsValue = content.GetPropertyValue(DefinitionPropertySettings.YCoors.Key);
            Assert.AreEqual("950.00000 910.00000 870.00000 830.00000 790.00000 750.00000", yCoorsValue);

            var zCoorsValue = content.GetPropertyValue(DefinitionPropertySettings.ZCoors.Key);
            Assert.AreEqual("10.00000 6.50000 2.50000 2.50000 6.50000 10.00000", zCoorsValue);

            var sectionCountValue = content.GetPropertyValue(DefinitionPropertySettings.SectionCount.Key);
            Assert.AreEqual("3", sectionCountValue);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FrictionIds.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FrictionPositions.Key));

            content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(11, content.Properties.Count());
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Xyz, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            xyzCountValue = content.GetPropertyValue(DefinitionPropertySettings.XYZCount.Key);
            Assert.AreEqual("6", xyzCountValue);

            xCoorsValue = content.GetPropertyValue(DefinitionPropertySettings.XCoors.Key);
            Assert.AreEqual("485.00000 510.00000 535.00000 560.00000 585.00000 610.00000", xCoorsValue);

            yCoorsValue = content.GetPropertyValue(DefinitionPropertySettings.YCoors.Key);
            Assert.AreEqual("1050.00000 1010.00000 970.00000 930.00000 890.00000 850.00000", yCoorsValue);

            zCoorsValue = content.GetPropertyValue(DefinitionPropertySettings.ZCoors.Key);
            Assert.AreEqual("10.50000 7.00000 3.00000 3.00000 7.00000 10.50000", zCoorsValue);

            sectionCountValue = content.GetPropertyValue(DefinitionPropertySettings.SectionCount.Key);
            Assert.AreEqual("3", sectionCountValue);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FrictionIds.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FrictionPositions.Key));

        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_ZwFromCulvert()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            var culvert = new Culvert()
            {
                Name = "Culvert1",
                GeometryType = CulvertGeometryType.Tabulated
            };
            
            culvert.TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(0, 30, 0);
            culvert.TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(-5, 50, 0);
            culvert.TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(-10, 20, 0);

            branch.BranchFeatures.Add(culvert);
            var culvertCrossSectionDefinition = network.Culverts.First().CrossSectionDefinition;
            var culvertCrossSection = new CrossSection(culvertCrossSectionDefinition) { Name = culvertCrossSectionDefinition.Name };
            
            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(new []{culvertCrossSection});
            
            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(12, content.Properties.Count());
            var nameValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("Culvert1", nameValue);

            var mainValue = content.GetPropertyValue(DefinitionPropertySettings.Main.Key);
            Assert.AreEqual("50.000", mainValue);

            var floodPlain1PropertyExists = content.Properties.Any(p => p.Key == DefinitionPropertySettings.FloodPlain1.Key);
            Assert.IsFalse(floodPlain1PropertyExists, "CrossSectionDefinition from Culvert should not write floodplain1 to file");
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_ZwFromBridge()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            var bridge = new Bridge("Bridge1");

            bridge.TabulatedCrossSectionDefinition.ZWDataTable.Clear();
            bridge.TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(0, 30, 0);
            bridge.TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(-5, 50, 0);
            bridge.TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(-10, 20, 0);

            branch.BranchFeatures.Add(bridge);
            var bridgeCrossSectionDefinition = network.Bridges.First().CrossSectionDefinition;
            var bridgeCrossSection = new CrossSection(bridgeCrossSectionDefinition) { Name = bridgeCrossSectionDefinition.Name };

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(new[] { bridgeCrossSection });

            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(12, content.Properties.Count());
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("Bridge1", idValue);

            var mainValue = content.GetPropertyValue(DefinitionPropertySettings.Main.Key);
            Assert.AreEqual("50.000", mainValue);

            var floodPlain1PropertyExists = content.Properties.Any(p => p.Key == DefinitionPropertySettings.FloodPlain1.Key);
            Assert.IsFalse(floodPlain1PropertyExists, "CrossSectionDefinition from Bridge should not write floodplain1 to file");
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Zw()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branch was added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionZw(branch, 30.0, -2.0, 100.0, 200.0, 0.5);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionZw(branch, 80.0, -3.0, 200.0, 300.0, 1.5);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(14, content.Properties.Count());
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);
            
            var numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("2", numLevelsValue);

            var levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("-10.00000 0.00000", levelsValue);

            var flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("33.33333 100.00000", flowWidthsValue);

            var totalWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.TotalWidths.Key);
            Assert.AreEqual("33.33333 100.00000", totalWidthsValue);

            var sdCrestValue = content.GetPropertyValue(DefinitionPropertySettings.CrestLevee.Key);
            Assert.AreEqual("-2.000", sdCrestValue);

            var sdFlowAreaValue = content.GetPropertyValue(DefinitionPropertySettings.FlowAreaLevee.Key);
            Assert.AreEqual("100.000", sdFlowAreaValue);

            var sdTotalAreaValue = content.GetPropertyValue(DefinitionPropertySettings.TotalAreaLevee.Key);
            Assert.AreEqual("200.000", sdTotalAreaValue);

            var sdBaseLevelValue = content.GetPropertyValue(DefinitionPropertySettings.BaseLevelLevee.Key);
            Assert.AreEqual("0.500", sdBaseLevelValue);

            var mainValue = content.GetPropertyValue(DefinitionPropertySettings.Main.Key);
            Assert.AreEqual("12.500", mainValue);

            var floodPlain1Value = content.GetPropertyValue(DefinitionPropertySettings.FloodPlain1.Key);
            Assert.AreEqual("25.000", floodPlain1Value);

            var floodPlain2Value = content.GetPropertyValue(DefinitionPropertySettings.FloodPlain2.Key);
            Assert.AreEqual("62.500", floodPlain2Value);

            content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(14, content.Properties.Count());
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);
            
            numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("2", numLevelsValue);

            levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("-10.00000 0.00000", levelsValue);

            flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("33.33333 100.00000", flowWidthsValue);

            totalWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.TotalWidths.Key);
            Assert.AreEqual("33.33333 100.00000", totalWidthsValue);

            sdCrestValue = content.GetPropertyValue(DefinitionPropertySettings.CrestLevee.Key);
            Assert.AreEqual("-3.000", sdCrestValue);

            sdFlowAreaValue = content.GetPropertyValue(DefinitionPropertySettings.FlowAreaLevee.Key);
            Assert.AreEqual("200.000", sdFlowAreaValue);

            sdTotalAreaValue = content.GetPropertyValue(DefinitionPropertySettings.TotalAreaLevee.Key);
            Assert.AreEqual("300.000", sdTotalAreaValue);

            sdBaseLevelValue = content.GetPropertyValue(DefinitionPropertySettings.BaseLevelLevee.Key);
            Assert.AreEqual("1.500", sdBaseLevelValue);

            mainValue = content.GetPropertyValue(DefinitionPropertySettings.Main.Key);
            Assert.AreEqual("12.500", mainValue);

            floodPlain1Value = content.GetPropertyValue(DefinitionPropertySettings.FloodPlain1.Key);
            Assert.AreEqual("25.000", floodPlain1Value);

            floodPlain2Value = content.GetPropertyValue(DefinitionPropertySettings.FloodPlain2.Key);
            Assert.AreEqual("62.500", floodPlain2Value);
        }
        
        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Rectangle()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionRectangle(branch, 30.0, 100.0, 80.0, false);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionRectangle(branch, 30.0, 200.0, 160.0, true);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);
            
            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(7, content.Properties.Count());
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Rectangle, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            var frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            var widthValue = content.GetPropertyValue(DefinitionPropertySettings.RectangleWidth.Key);
            Assert.AreEqual("100.000", widthValue);

            var heightValue = content.GetPropertyValue(DefinitionPropertySettings.RectangleHeight.Key);
            Assert.AreEqual("80.000", heightValue);

            var isClosed = content.GetPropertyValue(DefinitionPropertySettings.Closed.Key);
            Assert.AreEqual("no", isClosed);

            content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(7, content.Properties.Count());
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Rectangle, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            widthValue = content.GetPropertyValue(DefinitionPropertySettings.RectangleWidth.Key);
            Assert.AreEqual("200.000", widthValue);

            heightValue = content.GetPropertyValue(DefinitionPropertySettings.RectangleHeight.Key);
            Assert.AreEqual("160.000", heightValue);

            isClosed = content.GetPropertyValue(DefinitionPropertySettings.Closed.Key);
            Assert.AreEqual("yes", isClosed);
        }
        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Elliptical()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionElliptical(branch, 30.0, 100.0, 80.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionElliptical(branch, 30.0, 200.0, 160.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(11, content.Properties.Count());
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            var frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            var template = content.GetPropertyValue(DefinitionPropertySettings.Template.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Elliptical, template);

            var widthValue = content.GetPropertyValue(DefinitionPropertySettings.EllipseWidth.Key);
            Assert.AreEqual("100.000", widthValue);

            var heightValue = content.GetPropertyValue(DefinitionPropertySettings.EllipseHeight.Key);
            Assert.AreEqual("80.000", heightValue);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.NumLevels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.Levels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FlowWidths.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.TotalWidths.Key));

            content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(11, content.Properties.Count());
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            template = content.GetPropertyValue(DefinitionPropertySettings.Template.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Elliptical, template);

            widthValue = content.GetPropertyValue(DefinitionPropertySettings.EllipseWidth.Key);
            Assert.AreEqual("200.000", widthValue);

            heightValue = content.GetPropertyValue(DefinitionPropertySettings.EllipseHeight.Key);
            Assert.AreEqual("160.000", heightValue);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.NumLevels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.Levels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FlowWidths.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.TotalWidths.Key));
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Circle()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionCircle(branch, 30.0, 100.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionCircle(branch, 30.0, 200.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(5, content.Properties.Count());
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Circle, typeValue);
            
            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            var frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            var diameterValue = content.GetPropertyValue(DefinitionPropertySettings.Diameter.Key);
            Assert.AreEqual("100.000", diameterValue);
            
            content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(5, content.Properties.Count());
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Circle, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            diameterValue = content.GetPropertyValue(DefinitionPropertySettings.Diameter.Key);
            Assert.AreEqual("200.000", diameterValue);
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Egg()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionEgg(branch, 30.0, 100.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionEgg(branch, 40.0, 200.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);
            
            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(11, content.Properties.Count());
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            var frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            var template = content.GetPropertyValue(DefinitionPropertySettings.Template.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Egg, template);

            var widthValue = content.GetPropertyValue(DefinitionPropertySettings.EggWidth.Key);
            Assert.AreEqual("100.000", widthValue);

            var heightValue = content.GetPropertyValue(DefinitionPropertySettings.EggHeight.Key);
            Assert.AreEqual("150.000", heightValue);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.NumLevels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.Levels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FlowWidths.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.TotalWidths.Key));
            
            content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(11, content.Properties.Count());
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            template = content.GetPropertyValue(DefinitionPropertySettings.Template.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Egg, template);

            widthValue = content.GetPropertyValue(DefinitionPropertySettings.EggWidth.Key);
            Assert.AreEqual("200.000", widthValue);

            heightValue = content.GetPropertyValue(DefinitionPropertySettings.EggHeight.Key);
            Assert.AreEqual("300.000", heightValue);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.NumLevels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.Levels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FlowWidths.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.TotalWidths.Key));
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Arch()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionArch(branch, 30.0, 100.0, 200.0, 150.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionArch(branch, 40.0, 200.0, 400.0, 300.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(12, content.Properties.Count());
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            var frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            var template = content.GetPropertyValue(DefinitionPropertySettings.Template.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Arch, template);

            var widthValue = content.GetPropertyValue(DefinitionPropertySettings.ArchCrossSectionWidth.Key);
            Assert.AreEqual("100.000", widthValue);

            var heightValue = content.GetPropertyValue(DefinitionPropertySettings.ArchCrossSectionHeight.Key);
            Assert.AreEqual("200.000", heightValue);

            var archHeightValue = content.GetPropertyValue(DefinitionPropertySettings.ArchHeight.Key);
            Assert.AreEqual("150.000", archHeightValue);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.NumLevels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.Levels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FlowWidths.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.TotalWidths.Key));

            content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(12, content.Properties.Count());
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            template = content.GetPropertyValue(DefinitionPropertySettings.Template.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Arch, template);

            widthValue = content.GetPropertyValue(DefinitionPropertySettings.ArchCrossSectionWidth.Key);
            Assert.AreEqual("200.000", widthValue);

            heightValue = content.GetPropertyValue(DefinitionPropertySettings.ArchCrossSectionHeight.Key);
            Assert.AreEqual("400.000", heightValue);

            archHeightValue = content.GetPropertyValue(DefinitionPropertySettings.ArchHeight.Key);
            Assert.AreEqual("300.000", archHeightValue);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.NumLevels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.Levels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FlowWidths.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.TotalWidths.Key));
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Cunette()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionCunette(branch, 30.0, 100.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionCunette(branch, 40.0, 200.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(11, content.Properties.Count());

            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            var frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            var template = content.GetPropertyValue(DefinitionPropertySettings.Template.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Mouth, template);

            var widthValue = content.GetPropertyValue(DefinitionPropertySettings.CunetteWidth.Key);
            Assert.AreEqual("100.000", widthValue);

            var heightValue = content.GetPropertyValue(DefinitionPropertySettings.CunetteHeight.Key);
            Assert.AreEqual("63.400", heightValue);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.NumLevels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.Levels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FlowWidths.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.TotalWidths.Key));

            content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(11, content.Properties.Count());
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            template = content.GetPropertyValue(DefinitionPropertySettings.Template.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Mouth, template);

            widthValue = content.GetPropertyValue(DefinitionPropertySettings.CunetteWidth.Key);
            Assert.AreEqual("200.000", widthValue);

            heightValue = content.GetPropertyValue(DefinitionPropertySettings.CunetteHeight.Key);
            Assert.AreEqual("126.800", heightValue);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.NumLevels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.Levels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FlowWidths.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.TotalWidths.Key));
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_SteelCunette()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionSteelCunette(branch, 30.0, 100.0, 50.0, 100.0, 50.0, 100.0, 45.0, 135.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionSteelCunette(branch, 40.0, 200.0, 100.0, 200.0, 100.0, 200.0, 30.0, 120.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(16, content.Properties.Count());
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            var frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            var template = content.GetPropertyValue(DefinitionPropertySettings.Template.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.SteelMouth, template);

            var heightValue = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteHeight.Key);
            Assert.AreEqual("100.000", heightValue);

            var rValue = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteR.Key);
            Assert.AreEqual("50.000", rValue);

            var r1Value = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteR1.Key);
            Assert.AreEqual("100.000", r1Value);

            var r2Value = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteR2.Key);
            Assert.AreEqual("50.000", r2Value);

            var r3Value = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteR3.Key);
            Assert.AreEqual("100.000", r3Value);

            var aValue = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteA.Key);
            Assert.AreEqual("45.000", aValue);

            var a1Value = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteA1.Key);
            Assert.AreEqual("135.000", a1Value);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.NumLevels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.Levels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FlowWidths.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.TotalWidths.Key));
            
            content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(16, content.Properties.Count());
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            template = content.GetPropertyValue(DefinitionPropertySettings.Template.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.SteelMouth, template);

            heightValue = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteHeight.Key);
            Assert.AreEqual("200.000", heightValue);

            rValue = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteR.Key);
            Assert.AreEqual("100.000", rValue);

            r1Value = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteR1.Key);
            Assert.AreEqual("200.000", r1Value);

            r2Value = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteR2.Key);
            Assert.AreEqual("100.000", r2Value);

            r3Value = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteR3.Key);
            Assert.AreEqual("200.000", r3Value);

            aValue = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteA.Key);
            Assert.AreEqual("30.000", aValue);

            a1Value = content.GetPropertyValue(DefinitionPropertySettings.SteelCunetteA1.Key);
            Assert.AreEqual("120.000", a1Value);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.NumLevels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.Levels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FlowWidths.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.TotalWidths.Key));
        }
        
        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Trapezium()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionTrapezium(branch, 30.0, 100.0, 200.0, 150.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionTrapezium(branch, 40.0, 200.0, 400.0, 300.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, iniSections.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(12, content.Properties.Count());
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            var frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            var template = content.GetPropertyValue(DefinitionPropertySettings.Template.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Trapezium, template);

            var slopeValue = content.GetPropertyValue(DefinitionPropertySettings.Slope.Key);
            Assert.AreEqual("100.000", slopeValue);

            var heightValue = content.GetPropertyValue(DefinitionPropertySettings.MaximumFlowWidth.Key);
            Assert.AreEqual("200.000", heightValue);

            var baseWidth = content.GetPropertyValue(DefinitionPropertySettings.BottomWidth.Key);
            Assert.AreEqual("150.000", baseWidth);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.NumLevels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.Levels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FlowWidths.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.TotalWidths.Key));

            content = iniSections.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(12, content.Properties.Count());
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection_1D_2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            frictionId = content.GetPropertyValue(DefinitionPropertySettings.FrictionId.Key);
            Assert.AreEqual("Main", frictionId);

            template = content.GetPropertyValue(DefinitionPropertySettings.Template.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Trapezium, template);

            slopeValue = content.GetPropertyValue(DefinitionPropertySettings.Slope.Key);
            Assert.AreEqual("200.000", slopeValue);

            heightValue = content.GetPropertyValue(DefinitionPropertySettings.MaximumFlowWidth.Key);
            Assert.AreEqual("400.000", heightValue);

            baseWidth = content.GetPropertyValue(DefinitionPropertySettings.BottomWidth.Key);
            Assert.AreEqual("300.000", baseWidth);

            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.NumLevels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.Levels.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.FlowWidths.Key));
            Assert.IsNotNull(content.Properties.FirstOrDefault(p => p.Key == DefinitionPropertySettings.TotalWidths.Key));
        }

    }
}

