using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Reader;
using DeltaShell.Plugins.NetworkEditor;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class CrossSectionDefinitionFileWriterIntegrationTestInMemory
    {
        [SetUp]
        public void SetUp()
        {
            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Info);
        }

        [TearDown]
        public void TearDown()
        {
            LogHelper.SetLoggingLevel(Level.Error);
        }

        [Test]
        public void TestIntegrationFileWriterCrossSectionDefinitions()
        {

            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Run();
                var path = "Sobek_FB_Write.dsproj";

                app.SaveProjectAs(path);

                using (var waterFlowModel1D = new WaterFlowModel1D()
                {
                    // use a valid network for the calculation
                    //Network = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch()
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
                        HydroNetworkHelper.GenerateDiscretization(waterFlowModel1D.NetworkDiscretization,
                            (IChannel) branch,
                            offsets);
                    }
                    app.Project.RootFolder.Add(waterFlowModel1D);
                    app.SaveProjectAs(path);

                    var relativePathCrossSectionDefinitionsExpectedFile =
                        TestHelper.GetTestFilePath(
                            @"FileWriters\IntegrationTests\CrossSectionDefinitions_expected.txt");

                    waterFlowModel1D.Network.Branches.AddMultipleCrossSections();

                    WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(waterFlowModel1D.Network);

                    foreach (var branch in waterFlowModel1D.Network.Branches)
                    {
                        var mainRoughnessSection =
                            waterFlowModel1D.RoughnessSections.First(
                                r => r.Name == CrossSectionDefinitionZW.MainSectionName);
                        var floodPlain1RoughnessSection =
                            waterFlowModel1D.RoughnessSections.First(
                                r => r.Name == CrossSectionDefinitionZW.Floodplain1SectionTypeName);
                        var floodPlain2RoughnessSection =
                            waterFlowModel1D.RoughnessSections.First(
                                r => r.Name == CrossSectionDefinitionZW.Floodplain2SectionTypeName);

                        mainRoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 20)] = new object[]
                        {45.0, RoughnessType.Chezy};
                        floodPlain1RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 20)] =
                            new object[] {40.0, RoughnessType.Manning};
                        floodPlain2RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 20)] =
                            new object[] {35.0, RoughnessType.DeBosAndBijkerk};

                        mainRoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 25)] = new object[]
                        {45.0, RoughnessType.Chezy};
                        floodPlain1RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 25)] =
                            new object[] {40.0, RoughnessType.Manning};
                        floodPlain2RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 25)] =
                            new object[] {35.0, RoughnessType.DeBosAndBijkerk};
                    }
                    waterFlowModel1D.Network.Branches[0].AddMultipleStructures();

                    string relativePathActualFile = string.Empty;
                    try
                    {
                        app.SaveProjectAs(path);
                        app.RunActivity(waterFlowModel1D);
                        var actualDirectory = Path.Combine(waterFlowModel1D.ExplicitWorkingDirectory, waterFlowModel1D.DirectoryName);
                        relativePathActualFile = Path.Combine(actualDirectory, new ModelFileNames().CrossSectionDefinitions);
                        
                    }
                    catch (Exception exception)
                    {
                        // ignored
                        Console.WriteLine(exception.Message);
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
        
        [Test]
        [Category(TestCategory.Slow)]
        public void TestIntegrationFileReaderCrossSection()
        {
            var fileName = "Sobek_FB_Read.dsproj";

            TestHelper.PerformActionInTemporaryDirectory(temp =>
            {
                var path = Path.Combine(temp, fileName);
                using (var app = new DeltaShellApplication())
                {
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                    app.Run();
                    app.SaveProjectAs(path);
                    using (var waterFlowModel1D = new WaterFlowModel1D()
                    {
                        // use a valid network for the calculation
                        Network = HydroNetworkHelper.GetSnakeHydroNetwork(9)

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

                        var offsets = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
                        foreach (var branch in waterFlowModel1D.Network.Branches)
                        {
                            HydroNetworkHelper.GenerateDiscretization(waterFlowModel1D.NetworkDiscretization,
                                (IChannel)branch,
                                offsets);
                        }
                        app.Project.RootFolder.Add(waterFlowModel1D);
                        app.SaveProjectAs(path);


                        waterFlowModel1D.Network.Branches.AddMultipleCrossSections();

                        waterFlowModel1D.Network.Branches.AddEvenMoreMultipleCrossSections();

                        WaterFlowModel1DTestHelper.RefreshCrossSectionDefinitionSectionWidths(waterFlowModel1D.Network);

                        foreach (var branch in waterFlowModel1D.Network.Branches)
                        {
                            var mainRoughnessSection =
                                waterFlowModel1D.RoughnessSections.First(
                                    r => r.Name == CrossSectionDefinitionZW.MainSectionName);
                            var floodPlain1RoughnessSection =
                                waterFlowModel1D.RoughnessSections.First(
                                    r => r.Name == CrossSectionDefinitionZW.Floodplain1SectionTypeName);
                            var floodPlain2RoughnessSection =
                                waterFlowModel1D.RoughnessSections.First(
                                    r => r.Name == CrossSectionDefinitionZW.Floodplain2SectionTypeName);

                            mainRoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 20)] = new object[]
                            {45.0, RoughnessType.Chezy};
                            floodPlain1RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 20)] = new object
                                []
                            {40.0, RoughnessType.Manning};
                            floodPlain2RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 20)] = new object
                                []
                            {35.0, RoughnessType.DeBosAndBijkerk};

                            mainRoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 25)] = new object[]
                            {45.0, RoughnessType.Chezy};
                            floodPlain1RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 25)] = new object
                                []
                            {40.0, RoughnessType.Manning};
                            floodPlain2RoughnessSection.RoughnessNetworkCoverage[new NetworkLocation(branch, 25)] = new object
                                []
                            {35.0, RoughnessType.DeBosAndBijkerk};
                        }
                        waterFlowModel1D.Network.Branches[0].AddMultipleStructures();

                        var targetPath = string.Empty;
                        try
                        {
                            app.SaveProjectAs(path);
                            targetPath = Path.Combine(waterFlowModel1D.ExplicitWorkingDirectory, waterFlowModel1D.DirectoryName);
                            app.RunActivity(waterFlowModel1D);
                        }
                        catch (Exception exception)
                        {
                            // ignored
                            Console.WriteLine(exception.Message);
                        }
                        var modelFilename = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
                        var modelFileNames = new ModelFileNames(modelFilename);

                        var crossSectionFileReader = new CrossSectionFileReader((header, errorMessages) => {});

                        crossSectionFileReader.Read(modelFileNames.CrossSectionDefinitions,
                            modelFileNames.CrossSectionLocations, waterFlowModel1D.Network);
                    }
                }
            });

        }
    }
         
}