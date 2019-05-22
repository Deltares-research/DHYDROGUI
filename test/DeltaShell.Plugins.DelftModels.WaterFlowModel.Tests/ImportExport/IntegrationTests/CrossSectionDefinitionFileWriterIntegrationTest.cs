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
        }

        [TearDown]
        public void TearDown()
        {
            app.Dispose();
            LogHelper.SetLoggingLevel(Level.Error);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void TestIntegrationFileWriterGivesExpectedResults_CrossSectionDefinitions_MultipleTypes()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D
            {
                // use a valid network for the calculation
                Network = HydroNetworkHelper.GetSnakeHydroNetwork(4)

            })
            {
                waterFlowModel1D.Network.CrossSectionSectionTypes.Add(new CrossSectionSectionType
                {
                    Name = CrossSectionDefinitionZW.Floodplain1SectionTypeName
                });
                waterFlowModel1D.Network.CrossSectionSectionTypes.Add(new CrossSectionSectionType
                {
                    Name = CrossSectionDefinitionZW.Floodplain2SectionTypeName
                });
                //var branch = model.Network.Branches.FirstOrDefault();
                var offsets = new double[] {0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100};
                foreach (var branch in waterFlowModel1D.Network.Branches)
                {
                    HydroNetworkHelper.GenerateDiscretization(waterFlowModel1D.NetworkDiscretization, (IChannel) branch, offsets);
                }

                app.Project.RootFolder.Add(waterFlowModel1D);

                string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                waterFlowModel1D.ExplicitWorkingDirectory = Path.Combine(tempDirectory, "dflow1d");
                
                waterFlowModel1D.Network.Branches.AddMultipleCrossSections();

                WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(waterFlowModel1D.Network);

                foreach (var branch in waterFlowModel1D.Network.Branches)
                {
                    var mainRoughnessSection = waterFlowModel1D.RoughnessSections.First(r => r.Name == CrossSectionDefinitionZW.MainSectionName);
                    var floodPlain1RoughnessSection = waterFlowModel1D.RoughnessSections.First(r => r.Name == CrossSectionDefinitionZW.Floodplain1SectionTypeName);
                    var floodPlain2RoughnessSection = waterFlowModel1D.RoughnessSections.First(r => r.Name == CrossSectionDefinitionZW.Floodplain2SectionTypeName);

                    mainRoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 20)] = new object[] {45.0, RoughnessType.Chezy};
                    floodPlain1RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 20)] = new object[] {40.0, RoughnessType.Manning};
                    floodPlain2RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 20)] = new object[] {35.0, RoughnessType.DeBosAndBijkerk};

                    mainRoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 25)] = new object[] {45.0, RoughnessType.Chezy};
                    floodPlain1RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 25)] = new object[] {40.0, RoughnessType.Manning};
                    floodPlain2RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 25)] = new object[] {35.0, RoughnessType.DeBosAndBijkerk};
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
                var relativePathCrossSectionDefinitionsExpectedFile = TestHelper.GetTestFilePath(@"FileWriters\IntegrationTests\CrossSectionDefinitions_expected.txt");
                string errorMessage;
                Assert.IsTrue(
                    FileComparer.Compare(relativePathCrossSectionDefinitionsExpectedFile, relativePathActualFile, out errorMessage, true),
                    string.Format("Generated CrossSectionDefinitions file does not match template!{0}{1}", Environment.NewLine, errorMessage));
            }
        }

    }
}