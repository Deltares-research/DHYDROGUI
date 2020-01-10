using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Roughness
{
    [TestFixture]
    public class RoughnessFromCsvToSectionImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void RoughnessFromCsvToSectionImporterPropertiesTest()
        {
            var importer = new RoughnessFromCsvToSectionImporter();
            var expectedSupportedItemTypes = new List<Type>{
                typeof(RoughnessSection),
                typeof(IList<RoughnessSection>)};
            var importerSupportedItemTypes = importer.SupportedItemTypes;
            Assert.IsFalse(expectedSupportedItemTypes.Any(e => !importerSupportedItemTypes.Contains(e)));
            Assert.IsFalse(importerSupportedItemTypes.Any(e => !expectedSupportedItemTypes.Contains(e)));

            Assert.IsFalse(expectedSupportedItemTypes.Any( s => !importer.CanImportOn(s)));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportMaasCsvPerBranchJustForMainAndCheckIntegration()
        {
            //import maas
            var modelImporter = new SobekModelToIntegratedModelImporter { TargetItem = new WaterFlowModel1D()};

            var modelPath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"ReModels\J_10BANK.sbk\4\DEFTOP.1");
            var flowModel = (WaterFlowModel1D) modelImporter.ImportItem(modelPath);
            
            var mainSection = flowModel.RoughnessSections[0];
            var fp1 = flowModel.RoughnessSections[1];
            var fp2 = flowModel.RoughnessSections[2];

            //clear some roughness data
            mainSection.Clear();
            fp1.Clear();

            int countBefore = fp2.RoughnessNetworkCoverage.Locations.Values.Count;

            //import csv
            var fileName = TestHelper.GetTestFilePath("roughness_voorbeeld_Maas.csv");
            var importer = new RoughnessFromCsvToSectionImporter();
            importer.ImportItem(fileName, mainSection);

            var branch1 = mainSection.Network.Branches.First(br => br.Name.Equals("001"));
            var branch9 = mainSection.Network.Branches.First(br => br.Name.Equals("009"));
            var branch20 = mainSection.Network.Branches.First(br => br.Name.Equals("020"));

            Assert.IsNotNull(mainSection.FunctionOfQ(branch1));
            Assert.IsNotNull(mainSection.FunctionOfQ(branch9));
            Assert.IsNotNull(mainSection.FunctionOfQ(branch20));
            Assert.AreEqual(8, mainSection.RoughnessNetworkCoverage.Locations.Values.Count);

            Assert.AreEqual(0, fp1.RoughnessNetworkCoverage.Locations.Values.Count); //unchanged: cleared
            Assert.AreEqual(countBefore, fp2.RoughnessNetworkCoverage.Locations.Values.Count); //unchanged
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportMaasCsvPerBranchAndCheckIntegration()
        {
            //import maas
            var modelImporter = new SobekModelToIntegratedModelImporter { TargetItem = new WaterFlowModel1D() };

            var modelPath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"ReModels\J_10BANK.sbk\4\DEFTOP.1");
            var flowModel = (WaterFlowModel1D)modelImporter.ImportItem(modelPath);
            
            //clear roughness data
            flowModel.RoughnessSections.ForEach(rs => rs.Clear());

            //import csv
            var fileName = TestHelper.GetTestFilePath("roughness_voorbeeld_Maas.csv");
            var importer = new RoughnessFromCsvToSectionImporter();
            importer.ImportItem(fileName, flowModel.RoughnessSections);
            
            var mainSection = flowModel.RoughnessSections[0];

            var branch1 = mainSection.Network.Branches.First(br => br.Name.Equals("001"));
            var branch9 = mainSection.Network.Branches.First(br => br.Name.Equals("009"));
            var branch20 = mainSection.Network.Branches.First(br => br.Name.Equals("020"));

            Assert.IsNotNull(mainSection.FunctionOfQ(branch1));
            Assert.IsNotNull(mainSection.FunctionOfQ(branch9));
            Assert.IsNotNull(mainSection.FunctionOfQ(branch20));
            Assert.AreEqual(8, mainSection.RoughnessNetworkCoverage.Locations.Values.Count);
        }
    }
}
