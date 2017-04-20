using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.IntegrationTests
{
    [TestFixture]
    public class CrossSectionDefinitionFileWriterIntegrationTest
    {
        private DeltaShellApplication app;

        [SetUp]
        public void SetUp()
        {
            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Info);

            app = CrossSectionsFromDsProjTest.GetRunningDSApplication();
            app.SaveProjectAs("Sobek_FB.dsproj");

            using (var model = new WaterFlowModel1D()
            {
                // use a valid network for the calculation
                Network = HydroNetworkHelper.GetSnakeHydroNetwork(4)

            })
            {
                model.Network.CrossSectionSectionTypes.Add(new CrossSectionSectionType
                {
                    Name = CrossSectionDefinitionZW.Floodplain1SectionTypeName
                });
                model.Network.CrossSectionSectionTypes.Add(new CrossSectionSectionType
                {
                    Name = CrossSectionDefinitionZW.Floodplain2SectionTypeName
                });
                //var branch = model.Network.Branches.FirstOrDefault();
                var offsets = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
                foreach (var branch in model.Network.Branches)
                {
                    HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, (IChannel)branch, offsets);    
                }
                
                app.Project.RootFolder.Add(model);
            }

        }

        [TearDown]
        public void TearDown()
        {
            app.Dispose();
            LogHelper.SetLoggingLevel(Level.Error);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category("DIMR_Introduction")]
        [Category(TestCategory.WorkInProgress)]
        public void TestIntegrationFileWriterGivesExpectedResults_CrossSectionDefinitions_MultipleTypes()
        {
            var waterFlowModel1D = (WaterFlowModel1D) app.Project.RootFolder.Models.First();
            //waterFlowModel1D.ExplicitWorkingDirectory = "flow1d_output";

            var relativePathCrossSectionDefinitionsExpectedFile =
                TestHelper.GetTestFilePath(
                    @"FileWriters\IntegrationTests\CrossSectionDefinitions_expected.txt");

            /*var branch = waterFlowModel1D.Network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");
*/
            waterFlowModel1D.Network.Branches.AddMultipleCrossSections();

            foreach (var branch in waterFlowModel1D.Network.Branches)
            {
                var mainRoughnessSection =
                    waterFlowModel1D.RoughnessSections.First(r => r.Name == CrossSectionDefinitionZW.MainSectionName);
                var floodPlain1RoughnessSection =
                    waterFlowModel1D.RoughnessSections.First(
                        r => r.Name == CrossSectionDefinitionZW.Floodplain1SectionTypeName);
                var floodPlain2RoughnessSection =
                    waterFlowModel1D.RoughnessSections.First(
                        r => r.Name == CrossSectionDefinitionZW.Floodplain2SectionTypeName);

                mainRoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 20)] = new object[]
                {45.0, RoughnessType.Chezy};
                floodPlain1RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 20)] = new object[]
                {40.0, RoughnessType.Manning};
                floodPlain2RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 20)] = new object[]
                {35.0, RoughnessType.DeBosAndBijkerk};

                mainRoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 25)] = new object[]
                {45.0, RoughnessType.Chezy};
                floodPlain1RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 25)] = new object[]
                {40.0, RoughnessType.Manning};
                floodPlain2RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 25)] = new object[]
                {35.0, RoughnessType.DeBosAndBijkerk};
            }
            string relativePathActualFile = string.Empty;
            try
            {
                app.RunActivity(waterFlowModel1D);
                var actualDirectory = Path.Combine(waterFlowModel1D.ExplicitWorkingDirectory, waterFlowModel1D.DirectoryName);
                relativePathActualFile = Path.Combine(actualDirectory, new ModelFileNames().CrossSectionDefinitions);
            }
            catch
            {
                // ignored
            }
            string errorMessage;
            Assert.IsTrue(
                FileComparer.Compare(relativePathCrossSectionDefinitionsExpectedFile, relativePathActualFile,
                    out errorMessage, true),
                string.Format("Generated CrossSectionDefinitions file does not match template!{0}{1}",
                    Environment.NewLine, errorMessage));

        }

    }
}