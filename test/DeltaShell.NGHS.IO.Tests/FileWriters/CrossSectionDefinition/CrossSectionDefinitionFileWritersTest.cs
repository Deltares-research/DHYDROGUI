using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
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

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionYz(branch, 1, 20.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionXyz(branch, 2, 80.0,
                               new[] { 585.0, 610.0, 635.0, 660.0, 685.0, 710.0 },
                               new[] { 950.0, 910.0, 870.0, 830.0, 790.0, 750.0 },
                               new[] { 10.0, 6.5, 2.5, 2.5, 6.5, 10.0 });

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionZw(branch, 30.0, -2.0, 100.0, 200.0, 0.5);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionRectangle(branch, 4, 30.0, 100.0, 80.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionElliptical(branch, 5, 30.0, 100.0, 80.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionCircle(branch, 6, 30.0, 200.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionEgg(branch, 7, 30.0, 100.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionArch(branch, 8, 30.0, 100.0, 200.0, 150.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionCunette(branch, 9, 30.0, 100.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionSteelCunette(branch, 10, 30.0, 100.0, 50.0, 100.0, 50.0, 100.0, 45.0, 135.0);
            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionTrapezium(branch, 11, 30.0, 100.0, 200.0, 150.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            string errorMessage;

            if (FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions != null)
            {
                var relativePathActualFile = Path.Combine(FileWriterTestHelper.RelativeTargetDirectory, Path.GetFileName(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions));
                var filesAreIdentical = FileComparer.Compare(relativePathCrossSectionDefinitionsExpectedFile, relativePathActualFile, out errorMessage, true);
                Assert.IsTrue(filesAreIdentical,
                    string.Format("Generated CrossSectionDefinitions file does not match template!{0}{1}", Environment.NewLine, errorMessage));
            }
        }
        

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Yz()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionYz(branch, 1, 20.0);
            FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.YZ, 80.0, 1.5, true);//+ a shift to check if z values ARE NOT shifted
            
            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);
            
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(7, content.Properties.Count);
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection1", idValue);
            
            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Yz, typeValue);
            
            var yzCountValue = content.GetPropertyValue(DefinitionPropertySettings.YZCount.Key);
            Assert.AreEqual("6", yzCountValue);

            var yValuesValue = content.GetPropertyValue(DefinitionPropertySettings.YValues.Key);
            Assert.AreEqual("0.000 22.222 33.333 66.667 77.778 100.000", yValuesValue);

            var zValuesValue = content.GetPropertyValue(DefinitionPropertySettings.ZValues.Key);
            Assert.AreEqual("0.000 0.000 -10.000 -10.000 0.000 0.000", zValuesValue);
            
            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("50.000", thalwegValue);
            
            var deltaZStorageValue = content.GetPropertyValue(DefinitionPropertySettings.DeltaZStorage.Key);
            Assert.AreEqual("0.000 0.000 0.000 0.000 0.000 0.000", deltaZStorageValue);

            content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(7, content.Properties.Count);
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Yz, typeValue);

            yzCountValue = content.GetPropertyValue(DefinitionPropertySettings.YZCount.Key);
            Assert.AreEqual("6", yzCountValue);

            yValuesValue = content.GetPropertyValue(DefinitionPropertySettings.YValues.Key);
            Assert.AreEqual("0.000 22.222 33.333 66.667 77.778 100.000", yValuesValue);

            zValuesValue = content.GetPropertyValue(DefinitionPropertySettings.ZValues.Key);
            Assert.AreEqual("0.000 0.000 -10.000 -10.000 0.000 0.000", zValuesValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("50.000", thalwegValue);

            deltaZStorageValue = content.GetPropertyValue(DefinitionPropertySettings.DeltaZStorage.Key);
            Assert.AreEqual("0.000 0.000 0.000 0.000 0.000 0.000", deltaZStorageValue);
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Xyz()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionXyz(branch, 1, 20.0, 
                               new[] { 585.0, 610.0, 635.0, 660.0, 685.0, 710.0 }, 
                               new[] { 950.0, 910.0, 870.0, 830.0, 790.0, 750.0 }, 
                               new[] { 10.0, 6.5, 2.5, 2.5, 6.5, 10.0 });

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionXyz(branch, 2, 60.0,
                               new[] { 485.0, 510.0, 535.0, 560.0, 585.0, 610.0 },
                               new[] { 1050.0, 1010.0, 970.0, 930.0, 890.0, 850.0 },
                               new[] { 10.5, 7.0, 3.0, 3.0, 7.0, 10.5 });

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(10, content.Properties.Count);
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Xyz, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            var xyzCountValue = content.GetPropertyValue(DefinitionPropertySettings.XYZCount.Key);
            Assert.AreEqual("6", xyzCountValue);

            var xCoorsValue = content.GetPropertyValue(DefinitionPropertySettings.XCoors.Key);
            Assert.AreEqual("585.000 610.000 635.000 660.000 685.000 710.000", xCoorsValue);

            var yCoorsValue = content.GetPropertyValue(DefinitionPropertySettings.YCoors.Key);
            Assert.AreEqual("950.000 910.000 870.000 830.000 790.000 750.000", yCoorsValue);

            var zCoorsValue = content.GetPropertyValue(DefinitionPropertySettings.ZCoors.Key);
            Assert.AreEqual("10.000 6.500 2.500 2.500 6.500 10.000", zCoorsValue);
            
            var yValuesValue = content.GetPropertyValue(DefinitionPropertySettings.YValues.Key);
            Assert.AreEqual("0.000 47.170 94.340 141.510 188.680 235.850", yValuesValue);

            var zValuesValue = content.GetPropertyValue(DefinitionPropertySettings.ZValues.Key);
            Assert.AreEqual("10.000 6.500 2.500 2.500 6.500 10.000", zValuesValue);

            var deltaZStorageValue = content.GetPropertyValue(DefinitionPropertySettings.DeltaZStorage.Key);
            Assert.AreEqual("0.000 0.000 0.000 0.000 0.000 0.000", deltaZStorageValue);
            
            content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(10, content.Properties.Count);
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Xyz, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            xyzCountValue = content.GetPropertyValue(DefinitionPropertySettings.XYZCount.Key);
            Assert.AreEqual("6", xyzCountValue);

            xCoorsValue = content.GetPropertyValue(DefinitionPropertySettings.XCoors.Key);
            Assert.AreEqual("485.000 510.000 535.000 560.000 585.000 610.000", xCoorsValue);

            yCoorsValue = content.GetPropertyValue(DefinitionPropertySettings.YCoors.Key);
            Assert.AreEqual("1050.000 1010.000 970.000 930.000 890.000 850.000", yCoorsValue);

            zCoorsValue = content.GetPropertyValue(DefinitionPropertySettings.ZCoors.Key);
            Assert.AreEqual("10.500 7.000 3.000 3.000 7.000 10.500", zCoorsValue);
            
            yValuesValue = content.GetPropertyValue(DefinitionPropertySettings.YValues.Key);
            Assert.AreEqual("0.000 47.170 94.340 141.510 188.680 235.850", yValuesValue);

            zValuesValue = content.GetPropertyValue(DefinitionPropertySettings.ZValues.Key);
            Assert.AreEqual("10.500 7.000 3.000 3.000 7.000 10.500", zValuesValue);

            deltaZStorageValue = content.GetPropertyValue(DefinitionPropertySettings.DeltaZStorage.Key);
            Assert.AreEqual("0.000 0.000 0.000 0.000 0.000 0.000", deltaZStorageValue);

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
            
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(12, content.Properties.Count);
            var nameValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("Culvert1", nameValue);

            var mainValue = content.GetPropertyValue(DefinitionPropertySettings.Main.Key);
            Assert.AreEqual("50.000", mainValue);

            var floodPlain1PropertyExists = content.Properties.Any(p => p.Name == DefinitionPropertySettings.FloodPlain1.Key);
            Assert.IsFalse(floodPlain1PropertyExists, "CrossSectionDefinition from Culvert should not write floodplain1 to file");
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_ZwFromBridge()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            var bridge = new Bridge("Bridge1");

            bridge.TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(0, 30, 0);
            bridge.TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(-5, 50, 0);
            bridge.TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(-10, 20, 0);

            branch.BranchFeatures.Add(bridge);
            var bridgeCrossSectionDefinition = network.Bridges.First().CrossSectionDefinition;
            var bridgeCrossSection = new CrossSection(bridgeCrossSectionDefinition) { Name = bridgeCrossSectionDefinition.Name };

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(new[] { bridgeCrossSection });

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(1, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(12, content.Properties.Count);
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("Bridge1", idValue);

            var mainValue = content.GetPropertyValue(DefinitionPropertySettings.Main.Key);
            Assert.AreEqual("50.000", mainValue);

            var floodPlain1PropertyExists = content.Properties.Any(p => p.Name == DefinitionPropertySettings.FloodPlain1.Key);
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

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(14, content.Properties.Count);
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection1", idValue);

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

            var sdCrestValue = content.GetPropertyValue(DefinitionPropertySettings.CrestSummerdike.Key);
            Assert.AreEqual("-2.000", sdCrestValue);

            var sdFlowAreaValue = content.GetPropertyValue(DefinitionPropertySettings.FlowAreaSummerdike.Key);
            Assert.AreEqual("100.000", sdFlowAreaValue);

            var sdTotalAreaValue = content.GetPropertyValue(DefinitionPropertySettings.TotalAreaSummerdike.Key);
            Assert.AreEqual("200.000", sdTotalAreaValue);

            var sdBaseLevelValue = content.GetPropertyValue(DefinitionPropertySettings.BaseLevelSummerdike.Key);
            Assert.AreEqual("0.500", sdBaseLevelValue);

            var mainValue = content.GetPropertyValue(DefinitionPropertySettings.Main.Key);
            Assert.AreEqual("12.500", mainValue);

            var floodPlain1Value = content.GetPropertyValue(DefinitionPropertySettings.FloodPlain1.Key);
            Assert.AreEqual("25.000", floodPlain1Value);

            var floodPlain2Value = content.GetPropertyValue(DefinitionPropertySettings.FloodPlain2.Key);
            Assert.AreEqual("62.500", floodPlain2Value);

            content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(14, content.Properties.Count);
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection2", idValue);

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

            sdCrestValue = content.GetPropertyValue(DefinitionPropertySettings.CrestSummerdike.Key);
            Assert.AreEqual("-3.000", sdCrestValue);

            sdFlowAreaValue = content.GetPropertyValue(DefinitionPropertySettings.FlowAreaSummerdike.Key);
            Assert.AreEqual("200.000", sdFlowAreaValue);

            sdTotalAreaValue = content.GetPropertyValue(DefinitionPropertySettings.TotalAreaSummerdike.Key);
            Assert.AreEqual("300.000", sdTotalAreaValue);

            sdBaseLevelValue = content.GetPropertyValue(DefinitionPropertySettings.BaseLevelSummerdike.Key);
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

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionRectangle(branch, 1, 30.0, 100.0, 80.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionRectangle(branch, 2, 30.0, 200.0, 160.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);
            
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(9, content.Properties.Count);
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Rectangle, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            var widthValue = content.GetPropertyValue(DefinitionPropertySettings.RectangleWidth.Key);
            Assert.AreEqual("100.000", widthValue);

            var heightValue = content.GetPropertyValue(DefinitionPropertySettings.RectangleHeight.Key);
            Assert.AreEqual("80.000", heightValue);

            CheckCrossSectionStandardPropertyValues(content);

            content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(9, content.Properties.Count);
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Rectangle, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            widthValue = content.GetPropertyValue(DefinitionPropertySettings.RectangleWidth.Key);
            Assert.AreEqual("200.000", widthValue);

            heightValue = content.GetPropertyValue(DefinitionPropertySettings.RectangleHeight.Key);
            Assert.AreEqual("160.000", heightValue);

            CheckCrossSectionStandardPropertyValues(content);
        }
        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Elliptical()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionElliptical(branch, 1, 30.0, 100.0, 80.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionElliptical(branch, 2, 30.0, 200.0, 160.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(11, content.Properties.Count);
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Elliptical, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            var widthValue = content.GetPropertyValue(DefinitionPropertySettings.EllipseWidth.Key);
            Assert.AreEqual("100.000", widthValue);

            var heightValue = content.GetPropertyValue(DefinitionPropertySettings.EllipseHeight.Key);
            Assert.AreEqual("80.000", heightValue);

            CheckCrossSectionStandardPropertyValues(content);

            var numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("21", numLevelsValue);
            
            var levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("0.00000 0.49247 1.95774 4.35974 7.63932 11.71573 16.48859 21.84038 27.63932 33.74262 40.00000 46.25738 52.36068 58.15962 63.51141 68.28427 72.36068 75.64026 78.04226 79.50753 80.00000", levelsValue);

            var flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("0.00000 15.64345 30.90170 45.39905 58.77853 70.71068 80.90170 89.10065 95.10565 98.76883 100.00000 98.76883 95.10565 89.10065 80.90170 70.71068 58.77853 45.39905 30.90170 15.64345 0.00000", flowWidthsValue);

            content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(11, content.Properties.Count);
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Elliptical, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            widthValue = content.GetPropertyValue(DefinitionPropertySettings.EllipseWidth.Key);
            Assert.AreEqual("200.000", widthValue);

            heightValue = content.GetPropertyValue(DefinitionPropertySettings.EllipseHeight.Key);
            Assert.AreEqual("160.000", heightValue);

            CheckCrossSectionStandardPropertyValues(content);

            numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("21", numLevelsValue);

            levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("0.00000 0.98493 3.91548 8.71948 15.27864 23.43146 32.97718 43.68076 55.27864 67.48524 80.00000 92.51476 104.72136 116.31924 127.02282 136.56854 144.72136 151.28052 156.08452 159.01507 160.00000", levelsValue);

            flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("0.00000 31.28689 61.80340 90.79810 117.55705 141.42136 161.80340 178.20130 190.21130 197.53767 200.00000 197.53767 190.21130 178.20130 161.80340 141.42136 117.55705 90.79810 61.80340 31.28689 0.00000", flowWidthsValue);

        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Circle()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionCircle(branch, 1, 30.0, 100.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionCircle(branch, 2, 30.0, 200.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(10, content.Properties.Count);
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Circle, typeValue);
            
            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            var diameterValue = content.GetPropertyValue(DefinitionPropertySettings.Diameter.Key);
            Assert.AreEqual("100.000", diameterValue);

            CheckCrossSectionStandardPropertyValues(content);

            var numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("21", numLevelsValue);

            var levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("0.00000 0.61558 2.44717 5.44967 9.54915 14.64466 20.61074 27.30048 34.54915 42.17828 50.00000 57.82172 65.45085 72.69952 79.38926 85.35534 90.45085 94.55033 97.55283 99.38442 100.00000", levelsValue);

            var flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("0.00000 15.64345 30.90170 45.39905 58.77853 70.71068 80.90170 89.10065 95.10565 98.76883 100.00000 98.76883 95.10565 89.10065 80.90170 70.71068 58.77853 45.39905 30.90170 15.64345 0.00000", flowWidthsValue);

            content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(10, content.Properties.Count);
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Circle, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            diameterValue = content.GetPropertyValue(DefinitionPropertySettings.Diameter.Key);
            Assert.AreEqual("200.000", diameterValue);

            CheckCrossSectionStandardPropertyValues(content);

            numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("21", numLevelsValue);

            levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("0.00000 1.23117 4.89435 10.89935 19.09830 29.28932 41.22147 54.60095 69.09830 84.35655 100.00000 115.64345 130.90170 145.39905 158.77853 170.71068 180.90170 189.10065 195.10565 198.76883 200.00000", levelsValue);

            flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("0.00000 31.28689 61.80340 90.79810 117.55705 141.42136 161.80340 178.20130 190.21130 197.53767 200.00000 197.53767 190.21130 178.20130 161.80340 141.42136 117.55705 90.79810 61.80340 31.28689 0.00000", flowWidthsValue);
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Egg()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionEgg(branch, 1, 30.0, 100.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionEgg(branch, 2, 40.0, 200.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);
            
            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(10, content.Properties.Count);
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Egg, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            var eggWidthValue = content.GetPropertyValue(DefinitionPropertySettings.EggWidth.Key);
            Assert.AreEqual("100.000", eggWidthValue);

            CheckCrossSectionStandardPropertyValues(content);

            var numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("21", numLevelsValue);

            var levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("0.00000 1.23117 4.89435 10.89935 19.09830 29.28932 41.22147 54.60095 69.09830 84.35655 100.00000 107.82172 115.45085 122.69952 129.38926 135.35534 140.45085 144.55033 147.55283 149.38442 150.00000", levelsValue);

            var flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("0.00000 15.64345 30.90170 45.39905 58.77853 70.71068 80.90170 89.10065 95.10565 98.76883 100.00000 98.76883 95.10565 89.10065 80.90170 70.71068 58.77853 45.39905 30.90170 15.64345 0.00000", flowWidthsValue);

            content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(10, content.Properties.Count);
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Egg, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            eggWidthValue = content.GetPropertyValue(DefinitionPropertySettings.EggWidth.Key);
            Assert.AreEqual("200.000", eggWidthValue);

            CheckCrossSectionStandardPropertyValues(content);

            numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("21", numLevelsValue);

            levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("0.00000 2.46233 9.78870 21.79870 38.19660 58.57864 82.44295 109.20190 138.19660 168.71311 200.00000 215.64345 230.90170 245.39905 258.77853 270.71068 280.90170 289.10065 295.10565 298.76883 300.00000", levelsValue);

            flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("0.00000 31.28689 61.80340 90.79810 117.55705 141.42136 161.80340 178.20130 190.21130 197.53767 200.00000 197.53767 190.21130 178.20130 161.80340 141.42136 117.55705 90.79810 61.80340 31.28689 0.00000", flowWidthsValue);
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Arch()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionArch(branch, 1, 30.0, 100.0, 200.0, 150.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionArch(branch, 2, 40.0, 200.0, 400.0, 300.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(12, content.Properties.Count);
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Arch, typeValue);
            
            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            var widthValue = content.GetPropertyValue(DefinitionPropertySettings.ArchCrossSectionWidth.Key);
            Assert.AreEqual("100.000", widthValue);

            var heightValue = content.GetPropertyValue(DefinitionPropertySettings.ArchCrossSectionHeight.Key);
            Assert.AreEqual("200.000", heightValue);

            var archHeightValue = content.GetPropertyValue(DefinitionPropertySettings.ArchHeight.Key);
            Assert.AreEqual("150.000", archHeightValue);

            CheckCrossSectionStandardPropertyValues(content);

            var numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("13", numLevelsValue);

            var levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("0.00000 50.00000 64.32372 78.64744 92.97116 107.29488 121.61860 135.94233 150.26605 164.58977 178.91349 193.23721 200.00000", levelsValue);

            var flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("100.00000 100.00000 99.54302 98.15934 95.80879 92.41764 87.86549 81.95911 74.37658 64.52970 51.12648 29.68802 0.00000", flowWidthsValue);

            content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(12, content.Properties.Count);
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Arch, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue);

            widthValue = content.GetPropertyValue(DefinitionPropertySettings.ArchCrossSectionWidth.Key);
            Assert.AreEqual("200.000", widthValue);

            heightValue = content.GetPropertyValue(DefinitionPropertySettings.ArchCrossSectionHeight.Key);
            Assert.AreEqual("400.000", heightValue);

            archHeightValue = content.GetPropertyValue(DefinitionPropertySettings.ArchHeight.Key);
            Assert.AreEqual("300.000", archHeightValue);

            CheckCrossSectionStandardPropertyValues(content);

            numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("13", numLevelsValue);

            levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("0.00000 100.00000 128.64744 157.29488 185.94233 214.58977 243.23721 271.88465 300.53209 329.17953 357.82698 386.47442 400.00000", levelsValue);

            flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("200.00000 200.00000 199.08605 196.31867 191.61758 184.83528 175.73098 163.91823 148.75316 129.05940 102.25296 59.37605 0.00000", flowWidthsValue);
        }

        private static void CheckCrossSectionStandardPropertyValues(DelftIniCategory category)
        {
            var closedValue = category.GetPropertyValue(DefinitionPropertySettings.Closed.Key);
            var groundLayerUsedValue = category.GetPropertyValue(DefinitionPropertySettings.GroundlayerUsed.Key);

            Assert.That(closedValue, Is.EqualTo("1"));
            Assert.That(groundLayerUsedValue, Is.EqualTo("0"));
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Cunette()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionCunette(branch, 1, 30.0, 100.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionCunette(branch, 2, 40.0, 200.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(10, content.Properties.Count);
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Cunette, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            var widthValue = content.GetPropertyValue(DefinitionPropertySettings.CunetteWidth.Key);
            Assert.AreEqual("100.000", widthValue);

            CheckCrossSectionStandardPropertyValues(content);

            var numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("44", numLevelsValue);

            var levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("0.00000 1.50952 3.01905 4.52857 6.03810 7.54762 9.05714 10.56667 12.07619 13.58571 15.09524 16.60476 18.11429 19.62381 21.13333 22.64286 24.15238 25.66190 27.17143 28.68095 30.19048 31.70000 33.20952 34.71905 36.22857 37.73810 39.24762 40.75714 42.26667 43.77619 45.28571 46.79524 48.30476 49.81429 51.32381 52.83333 54.34286 55.85238 57.36190 58.87143 60.38095 61.89048 62.99990 62.99990", levelsValue);

            var flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("0.00000 34.96651 49.26545 60.11059 69.14659 77.01293 84.03869 90.42003 96.28517 99.99314 99.91216 99.73978 99.47551 99.11862 98.66811 98.12268 97.48076 96.74040 95.89933 94.95489 93.90394 92.74287 91.46748 90.07293 88.55357 86.90286 85.11316 83.17550 81.07928 78.81183 76.35791 73.69890 70.81169 67.66708 64.22727 60.44189 56.24120 51.52372 46.13118 39.79005 31.94028 20.94959 0.20000 0.00000", flowWidthsValue);

            content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(10, content.Properties.Count);
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Cunette, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            widthValue = content.GetPropertyValue(DefinitionPropertySettings.CunetteWidth.Key);
            Assert.AreEqual("200.000", widthValue);

            CheckCrossSectionStandardPropertyValues(content);

            numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("44", numLevelsValue);

            levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("0.00000 3.01905 6.03810 9.05714 12.07619 15.09524 18.11429 21.13333 24.15238 27.17143 30.19048 33.20952 36.22857 39.24762 42.26667 45.28571 48.30476 51.32381 54.34286 57.36190 60.38095 63.40000 66.41905 69.43810 72.45714 75.47619 78.49524 81.51429 84.53333 87.55238 90.57143 93.59048 96.60952 99.62857 102.64762 105.66667 108.68571 111.70476 114.72381 117.74286 120.76190 123.78095 125.99990 125.99990", levelsValue);

            flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("0.00000 69.93302 98.53089 120.22117 138.29319 154.02585 168.07738 180.84006 192.57034 199.98628 199.82432 199.47955 198.95101 198.23724 197.33622 196.24537 194.96151 193.48080 191.79867 189.90978 187.80788 185.48574 182.93497 180.14585 177.10713 173.80571 170.22632 166.35100 162.15855 157.62366 152.71582 147.39780 141.62338 135.33416 128.45454 120.88378 112.48240 103.04744 92.26236 79.58010 63.88056 41.89918 0.28284 0.00000", flowWidthsValue);
        }

        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_SteelCunette()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionSteelCunette(branch, 1, 30.0, 100.0, 50.0, 100.0, 50.0, 100.0, 45.0, 135.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionSteelCunette(branch, 2, 40.0, 200.0, 100.0, 200.0, 100.0, 200.0, 30.0, 120.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(16, content.Properties.Count);
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.SteelCunette, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
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

            CheckCrossSectionStandardPropertyValues(content);

            var numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("43", numLevelsValue);

            var levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("0.00000 2.38095 4.76190 7.14286 9.52381 11.90476 14.28571 16.66667 19.04762 21.42857 23.80952 26.19048 28.57143 30.95238 33.33333 35.71429 38.09524 40.47619 42.85714 45.23810 47.61905 50.00000 52.38095 54.76190 57.14286 59.52381 61.90476 64.28571 66.66667 69.04762 71.42857 73.80952 76.19048 78.57143 80.95238 83.33333 85.71429 88.09524 90.47619 92.85714 95.23810 97.61905 100.00000", levelsValue);

            var flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("0.00000 161.71136 161.72253 161.62029 161.40447 161.07469 160.63040 160.07081 159.39496 158.60163 157.68942 156.65664 155.50137 154.22141 152.81425 151.27705 149.60662 147.79938 145.85130 143.75786 141.51397 139.11393 136.55130 133.81883 130.90829 127.81030 124.51418 121.00761 117.27638 113.30388 109.07061 104.55337 99.72429 94.54935 88.98636 82.98193 76.46678 69.34817 61.49703 52.72393 42.72984 30.49107 0.00000", flowWidthsValue);

            content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(16, content.Properties.Count);
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.SteelCunette, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
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

            CheckCrossSectionStandardPropertyValues(content);

            numLevelsValue = content.GetPropertyValue(DefinitionPropertySettings.NumLevels.Key);
            Assert.AreEqual("43", numLevelsValue);

            levelsValue = content.GetPropertyValue(DefinitionPropertySettings.Levels.Key);
            Assert.AreEqual("0.00000 4.76190 9.52381 14.28571 19.04762 23.80952 28.57143 33.33333 38.09524 42.85714 47.61905 52.38095 57.14286 61.90476 66.66667 71.42857 76.19048 80.95238 85.71429 90.47619 95.23810 100.00000 104.76190 109.52381 114.28571 119.04762 123.80952 128.57143 133.33333 138.09524 142.85714 147.61905 152.38095 157.14286 161.90476 166.66667 171.42857 176.19048 180.95238 185.71429 190.47619 195.23810 200.00000", levelsValue);

            flowWidthsValue = content.GetPropertyValue(DefinitionPropertySettings.FlowWidths.Key);
            Assert.AreEqual("0.00000 348.22705 348.04912 347.64409 347.01126 346.14955 345.05745 343.73307 342.17406 340.37762 338.34047 336.05882 333.52833 330.74405 327.70040 324.39109 320.80903 316.94628 312.79392 308.34192 303.57902 298.49254 293.06816 287.28967 281.13862 274.59395 267.63147 260.22320 252.33657 243.93329 234.96796 225.38603 215.12110 204.09097 192.19188 179.28960 165.20525 149.69106 132.38599 112.72579 89.73212 61.39080 0.00000", flowWidthsValue);
        }
        
        [Test]
        public void TestCrossSectionDefinitionFileWriterGivesExpectedResults_Trapezium()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionTrapezium(branch, 1, 30.0, 100.0, 200.0, 150.0);

            CrossSectionDefinitionFileWritersTestHelper.AddCrossSectionTrapezium(branch, 2, 40.0, 200.0, 400.0, 300.0);

            CrossSectionDefinitionFileWritersTestHelper.WriteCrossSectionsToIni(network.CrossSections);

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionDefinitions);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(2, categories.Count(op => op.Name == DefinitionPropertySettings.Header));

            var content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().First();
            Assert.AreEqual(9, content.Properties.Count);
            var idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection1", idValue);

            var typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Trapezium, typeValue);

            var thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            var widthValue = content.GetPropertyValue(DefinitionPropertySettings.Slope.Key);
            Assert.AreEqual("100.000", widthValue);

            var heightValue = content.GetPropertyValue(DefinitionPropertySettings.MaximumFlowWidth.Key);
            Assert.AreEqual("200.000", heightValue);

            var archHeightValue = content.GetPropertyValue(DefinitionPropertySettings.BottomWidth.Key);
            Assert.AreEqual("150.000", archHeightValue);

            CheckCrossSectionStandardPropertyValues(content);

            content = categories.Where(c => c.Name == DefinitionPropertySettings.Header).ToList().Last();
            Assert.AreEqual(9, content.Properties.Count);
            idValue = content.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.AreEqual("CrossSection2", idValue);

            typeValue = content.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.AreEqual(CrossSectionRegion.CrossSectionDefinitionType.Trapezium, typeValue);

            thalwegValue = content.GetPropertyValue(DefinitionPropertySettings.Thalweg.Key);
            Assert.AreEqual("0.000", thalwegValue); 
            
            widthValue = content.GetPropertyValue(DefinitionPropertySettings.Slope.Key);
            Assert.AreEqual("200.000", widthValue);

            heightValue = content.GetPropertyValue(DefinitionPropertySettings.MaximumFlowWidth.Key);
            Assert.AreEqual("400.000", heightValue);

            archHeightValue = content.GetPropertyValue(DefinitionPropertySettings.BottomWidth.Key);
            Assert.AreEqual("300.000", archHeightValue);

            CheckCrossSectionStandardPropertyValues(content);
        }

    }
}

