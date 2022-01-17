using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Extensions;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekRRNwrwImporterTest
    {
        [Test]
        public void SobekRRNwrwImporterImportsDryweatherFlowDefinitionsAndNwrwSettingsCorrectlyWithoutFmModelPresent()
        {
            var pathToSobekNetwork = TestHelper.GetTestFilePath(@"waard08.lit\NETWORK.TP");
            var rrModel = new RainfallRunoffModel();

            var importer = new SobekRRNwrwImporter
            {
                TargetObject = rrModel,
                PathSobek = pathToSobekNetwork
            };

            importer.Import();

            Assert.That(rrModel.NwrwDefinitions.Count, Is.EqualTo(12));
            Assert.That(rrModel.NwrwDryWeatherFlowDefinitions.Count, Is.EqualTo(3));
            Assert.That(rrModel.ModelData.OfType<NwrwData>().Count(), Is.EqualTo(75));
            AssertNwrwDefinitionsAreCorrect(rrModel.NwrwDefinitions);
            AssertNwrwDryweatherFlowDefinitionsAreCorrect(rrModel.NwrwDryWeatherFlowDefinitions);

        }
        
        [Test]
        public void SobekRRNwrwImporterImportsCorrectlyWithFmModel()
        {
            var pathToSobekNetwork = TestHelper.GetTestFilePath(@"waard08.lit\NETWORK.TP");
            var hydroModel = new HydroModelBuilder().BuildModel(ModelGroup.RHUModels);
            var rrModel = hydroModel.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
            Assert.That(rrModel, Is.Not.Null);
            var fmModel = hydroModel.GetAllActivitiesRecursive<WaterFlowFMModel>()?.FirstOrDefault();
            SetupFmModel(fmModel);
            Assert.That(fmModel, Is.Not.Null);

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroModel,
                new IPartialSobekImporter[] {new SobekRRNwrwImporter()});

            importer.Import();

            Assert.That(rrModel.NwrwDefinitions.Count, Is.EqualTo(12));
            Assert.That(rrModel.NwrwDryWeatherFlowDefinitions.Count, Is.EqualTo(3));
            AssertNwrwDefinitionsAreCorrect(rrModel.NwrwDefinitions);
            AssertNwrwDryweatherFlowDefinitionsAreCorrect(rrModel.NwrwDryWeatherFlowDefinitions);

            var nwrwDatas = rrModel.ModelData.OfType<NwrwData>();
            var lateralSourceDictionary = fmModel.LateralSourcesData.Select(lsd => lsd.Feature)
                .ToDictionary(lateral => lateral.Name, StringComparer.InvariantCultureIgnoreCase);

            Assert.That(nwrwDatas.Count(), Is.EqualTo(75));
            foreach (var nwrwData in nwrwDatas)
            {
                var nodeOrBranchId = nwrwData.NodeOrBranchId;

                Assert.That(nwrwData.Catchment, Is.Not.Null);
                Assert.That(nwrwData.Catchment.Geometry, Is.Not.Null);
                Assert.That(string.IsNullOrWhiteSpace(nodeOrBranchId), Is.False);
                Assert.That(nwrwData.SurfaceLevelDict.Count(), Is.EqualTo(12));
                Assert.That(lateralSourceDictionary.ContainsKey(nodeOrBranchId), Is.True);
            }
        }

        private void AssertNwrwDryweatherFlowDefinitionsAreCorrect(
            IList<NwrwDryWeatherFlowDefinition> rrModelNwrwDryWeatherFlowDefinitions)
        {
            var importedDefinition = rrModelNwrwDryWeatherFlowDefinitions[0];
            Assert.That(importedDefinition.Name, Is.EqualTo(NwrwData.DEFAULT_DWA_ID));
            Assert.That(importedDefinition.DailyVolumeConstant, Is.EqualTo(240));
            Assert.That(importedDefinition.DailyVolumeVariable, Is.EqualTo(120));
            Assert.That(importedDefinition.DistributionType, Is.EqualTo(DryweatherFlowDistributionType.Constant));
            Assert.That(importedDefinition.HourlyPercentageDailyVolume,
                Is.EqualTo(new[]
                    {1.5, 1.5, 1.5, 1.5, 1.5, 3, 4, 5, 6, 6.5, 7.5, 8.5, 7.5, 6.5, 6, 5, 5, 5, 4, 3.5, 3, 2.5, 2, 2}));

            importedDefinition = rrModelNwrwDryWeatherFlowDefinitions[1];
            Assert.That(importedDefinition.Name, Is.EqualTo("1"));
            Assert.That(importedDefinition.DailyVolumeConstant, Is.EqualTo(240));
            Assert.That(importedDefinition.DailyVolumeVariable, Is.EqualTo(120));
            Assert.That(importedDefinition.DistributionType, Is.EqualTo(DryweatherFlowDistributionType.Constant));
            Assert.That(importedDefinition.HourlyPercentageDailyVolume,
                Is.EqualTo(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));

            importedDefinition = rrModelNwrwDryWeatherFlowDefinitions[2];
            Assert.That(importedDefinition.Name, Is.EqualTo("P02_1"));
            Assert.That(importedDefinition.DailyVolumeConstant, Is.EqualTo(240));
            Assert.That(importedDefinition.DailyVolumeVariable, Is.EqualTo(120));
            Assert.That(importedDefinition.DistributionType, Is.EqualTo(DryweatherFlowDistributionType.Constant));
            Assert.That(importedDefinition.HourlyPercentageDailyVolume,
                Is.EqualTo(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));


        }

        private void AssertNwrwDefinitionsAreCorrect(IList<NwrwDefinition> rrModelNwrwDefinitions)
        {
            #region GVH_HEL

            var importedDefinition = rrModelNwrwDefinitions[0];
            Assert.That(importedDefinition.Name, Is.EqualTo("GVH_HEL"));
            Assert.That(importedDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.ClosedPavedWithSlope));
            Assert.That(importedDefinition.SurfaceStorage, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityMax, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityMin, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityReduction, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffDelay, Is.EqualTo(0.5));
            Assert.That(importedDefinition.RunoffLength, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffSlope, Is.EqualTo(0));
            Assert.That(importedDefinition.TerrainRoughness, Is.EqualTo(0));

            #endregion

            #region GVH_VLA

            importedDefinition = rrModelNwrwDefinitions[1];
            Assert.That(importedDefinition.Name, Is.EqualTo("GVH_VLA"));
            Assert.That(importedDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.ClosedPavedFlat));
            Assert.That(importedDefinition.SurfaceStorage, Is.EqualTo(0.5));
            Assert.That(importedDefinition.InfiltrationCapacityMax, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityMin, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityReduction, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffDelay, Is.EqualTo(0.2));
            Assert.That(importedDefinition.RunoffLength, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffSlope, Is.EqualTo(0));
            Assert.That(importedDefinition.TerrainRoughness, Is.EqualTo(0));

            #endregion

            #region GVH_VLU

            importedDefinition = rrModelNwrwDefinitions[2];
            Assert.That(importedDefinition.Name, Is.EqualTo("GVH_VLU"));
            Assert.That(importedDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.ClosedPavedFlatStretch));
            Assert.That(importedDefinition.SurfaceStorage, Is.EqualTo(1));
            Assert.That(importedDefinition.InfiltrationCapacityMax, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityMin, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityReduction, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffDelay, Is.EqualTo(0.1));
            Assert.That(importedDefinition.RunoffLength, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffSlope, Is.EqualTo(0));
            Assert.That(importedDefinition.TerrainRoughness, Is.EqualTo(0));

            #endregion

            #region OVH_HEL

            importedDefinition = rrModelNwrwDefinitions[3];
            Assert.That(importedDefinition.Name, Is.EqualTo("OVH_HEL"));
            Assert.That(importedDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.OpenPavedWithSlope));
            Assert.That(importedDefinition.SurfaceStorage, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityMax, Is.EqualTo(2));
            Assert.That(importedDefinition.InfiltrationCapacityMin, Is.EqualTo(0.5));
            Assert.That(importedDefinition.InfiltrationCapacityReduction, Is.EqualTo(3));
            Assert.That(importedDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0.1));
            Assert.That(importedDefinition.RunoffDelay, Is.EqualTo(0.5));
            Assert.That(importedDefinition.RunoffLength, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffSlope, Is.EqualTo(0));
            Assert.That(importedDefinition.TerrainRoughness, Is.EqualTo(0));

            #endregion

            #region OVH_VLA

            importedDefinition = rrModelNwrwDefinitions[4];
            Assert.That(importedDefinition.Name, Is.EqualTo("OVH_VLA"));
            Assert.That(importedDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.OpenPavedFlat));
            Assert.That(importedDefinition.SurfaceStorage, Is.EqualTo(0.5));
            Assert.That(importedDefinition.InfiltrationCapacityMax, Is.EqualTo(2));
            Assert.That(importedDefinition.InfiltrationCapacityMin, Is.EqualTo(0.5));
            Assert.That(importedDefinition.InfiltrationCapacityReduction, Is.EqualTo(3));
            Assert.That(importedDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0.1));
            Assert.That(importedDefinition.RunoffDelay, Is.EqualTo(0.2));
            Assert.That(importedDefinition.RunoffLength, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffSlope, Is.EqualTo(0));
            Assert.That(importedDefinition.TerrainRoughness, Is.EqualTo(0));

            #endregion

            #region OVH_VLU

            importedDefinition = rrModelNwrwDefinitions[5];
            Assert.That(importedDefinition.Name, Is.EqualTo("OVH_VLU"));
            Assert.That(importedDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.OpenPavedFlatStretched));
            Assert.That(importedDefinition.SurfaceStorage, Is.EqualTo(1));
            Assert.That(importedDefinition.InfiltrationCapacityMax, Is.EqualTo(2));
            Assert.That(importedDefinition.InfiltrationCapacityMin, Is.EqualTo(0.5));
            Assert.That(importedDefinition.InfiltrationCapacityReduction, Is.EqualTo(3));
            Assert.That(importedDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0.1));
            Assert.That(importedDefinition.RunoffDelay, Is.EqualTo(0.1));
            Assert.That(importedDefinition.RunoffLength, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffSlope, Is.EqualTo(0));
            Assert.That(importedDefinition.TerrainRoughness, Is.EqualTo(0));

            #endregion

            #region DAK_HEL

            importedDefinition = rrModelNwrwDefinitions[6];
            Assert.That(importedDefinition.Name, Is.EqualTo("DAK_HEL"));
            Assert.That(importedDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.RoofWithSlope));
            Assert.That(importedDefinition.SurfaceStorage, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityMax, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityMin, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityReduction, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffDelay, Is.EqualTo(0.5));
            Assert.That(importedDefinition.RunoffLength, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffSlope, Is.EqualTo(0));
            Assert.That(importedDefinition.TerrainRoughness, Is.EqualTo(0));

            #endregion

            #region DAK_VLA

            importedDefinition = rrModelNwrwDefinitions[7];
            Assert.That(importedDefinition.Name, Is.EqualTo("DAK_VLA"));
            Assert.That(importedDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.RoofFlat));
            Assert.That(importedDefinition.SurfaceStorage, Is.EqualTo(2));
            Assert.That(importedDefinition.InfiltrationCapacityMax, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityMin, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityReduction, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffDelay, Is.EqualTo(0.2));
            Assert.That(importedDefinition.RunoffLength, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffSlope, Is.EqualTo(0));
            Assert.That(importedDefinition.TerrainRoughness, Is.EqualTo(0));

            #endregion

            #region DAK_VLU

            importedDefinition = rrModelNwrwDefinitions[8];
            Assert.That(importedDefinition.Name, Is.EqualTo("DAK_VLU"));
            Assert.That(importedDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.RoofFlatStretched));
            Assert.That(importedDefinition.SurfaceStorage, Is.EqualTo(4));
            Assert.That(importedDefinition.InfiltrationCapacityMax, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityMin, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityReduction, Is.EqualTo(0));
            Assert.That(importedDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffDelay, Is.EqualTo(0.1));
            Assert.That(importedDefinition.RunoffLength, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffSlope, Is.EqualTo(0));
            Assert.That(importedDefinition.TerrainRoughness, Is.EqualTo(0));

            #endregion

            #region ONV_HEL

            importedDefinition = rrModelNwrwDefinitions[9];
            Assert.That(importedDefinition.Name, Is.EqualTo("ONV_HEL"));
            Assert.That(importedDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.UnpavedWithSlope));
            Assert.That(importedDefinition.SurfaceStorage, Is.EqualTo(2));
            Assert.That(importedDefinition.InfiltrationCapacityMax, Is.EqualTo(5));
            Assert.That(importedDefinition.InfiltrationCapacityMin, Is.EqualTo(1));
            Assert.That(importedDefinition.InfiltrationCapacityReduction, Is.EqualTo(3));
            Assert.That(importedDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0.1));
            Assert.That(importedDefinition.RunoffDelay, Is.EqualTo(0.5));
            Assert.That(importedDefinition.RunoffLength, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffSlope, Is.EqualTo(0));
            Assert.That(importedDefinition.TerrainRoughness, Is.EqualTo(0));

            #endregion

            #region ONV_VLA

            importedDefinition = rrModelNwrwDefinitions[10];
            Assert.That(importedDefinition.Name, Is.EqualTo("ONV_VLA"));
            Assert.That(importedDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.UnpavedFlat));
            Assert.That(importedDefinition.SurfaceStorage, Is.EqualTo(4));
            Assert.That(importedDefinition.InfiltrationCapacityMax, Is.EqualTo(5));
            Assert.That(importedDefinition.InfiltrationCapacityMin, Is.EqualTo(1));
            Assert.That(importedDefinition.InfiltrationCapacityReduction, Is.EqualTo(3));
            Assert.That(importedDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0.1));
            Assert.That(importedDefinition.RunoffDelay, Is.EqualTo(0.2));
            Assert.That(importedDefinition.RunoffLength, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffSlope, Is.EqualTo(0));
            Assert.That(importedDefinition.TerrainRoughness, Is.EqualTo(0));

            #endregion

            #region ONV_VLU

            importedDefinition = rrModelNwrwDefinitions[11];
            Assert.That(importedDefinition.Name, Is.EqualTo("ONV_VLU"));
            Assert.That(importedDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.UnpavedFlatStretched));
            Assert.That(importedDefinition.SurfaceStorage, Is.EqualTo(6));
            Assert.That(importedDefinition.InfiltrationCapacityMax, Is.EqualTo(5));
            Assert.That(importedDefinition.InfiltrationCapacityMin, Is.EqualTo(1));
            Assert.That(importedDefinition.InfiltrationCapacityReduction, Is.EqualTo(3));
            Assert.That(importedDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0.1));
            Assert.That(importedDefinition.RunoffDelay, Is.EqualTo(0.1));
            Assert.That(importedDefinition.RunoffLength, Is.EqualTo(0));
            Assert.That(importedDefinition.RunoffSlope, Is.EqualTo(0));
            Assert.That(importedDefinition.TerrainRoughness, Is.EqualTo(0));

            #endregion
        }

        private void SetupFmModel(WaterFlowFMModel fmModel)
        {
            var pathToSobekNetwork = TestHelper.GetTestFilePath(@"waard08.lit\NETWORK.TP");

            var importers = new List<IPartialSobekImporter>
            {
                new SobekBranchesImporter(),
                new SobekLateralSourcesImporter(),
                new SobekLateralSourcesDataImporter(),
            };

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, fmModel, importers);
            importer.Import();
        }
    }
}