using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Nwrw
{
    public static class NwrwFileWriterIntegrationTestHelper
    {
        public static void GenerateNwrwModelData(RainfallRunoffModel rrModel)
        {
            const string catchment1Name = "Catchment1";
            var nwrwImportHelper = new NwrwImporterHelper();
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            NwrwData.CreateNewNwrwDataWithCatchment(rrModel, catchment1Name, nwrwImportHelper, logHandler);
            NwrwData nwrwData = rrModel.GetAllModelData().OfType<NwrwData>().FirstOrDefault(md =>
                md.Catchment.Name.Equals(catchment1Name, StringComparison.InvariantCultureIgnoreCase));
            Assert.That(nwrwData, Is.Not.Null);
            nwrwData.Name = "node1";
            nwrwData.NodeOrBranchId = "node1";
            nwrwData.LateralSurface = 2.3;
            nwrwData.SurfaceLevelDict = GenerateSurfaceLevelDict();
            nwrwData.DryWeatherFlows = GenerateSingleDryWeatherFlow();

            const string catchment2Name = "Catchment2";
            NwrwData.CreateNewNwrwDataWithCatchment(rrModel, catchment2Name, nwrwImportHelper, logHandler);
            NwrwData anotherNwrwData = rrModel.GetAllModelData().OfType<NwrwData>().FirstOrDefault(md =>
                md.Catchment.Name.Equals(catchment2Name, StringComparison.InvariantCultureIgnoreCase));
            Assert.That(anotherNwrwData, Is.Not.Null);
            anotherNwrwData.Name = "node2";
            anotherNwrwData.NodeOrBranchId = "node2";
            anotherNwrwData.LateralSurface = 4.5;
            anotherNwrwData.SurfaceLevelDict = GenerateSurfaceLevelDict();
            anotherNwrwData.DryWeatherFlows = GenerateMultipleDryWeatherFlows();
            anotherNwrwData.MeteoStationId = "meteostation node2";
            anotherNwrwData.NumberOfSpecialAreas = 2;
            anotherNwrwData.SpecialAreas = GenerateSpecialAreas();
        }

        public static IList<NwrwSpecialArea> GenerateSpecialAreas()
        {
            var specialAreas = new List<NwrwSpecialArea>();

            specialAreas.Add(new NwrwSpecialArea
            {
                Area = 12345,
                SpecialInflowReference = "Inflow reference one"
            });
            specialAreas.Add(new NwrwSpecialArea
            {
                Area = 54321,
                SpecialInflowReference = "Inflow reference two"
            });
            return specialAreas;
        }

        public static IList<DryWeatherFlow> GenerateMultipleDryWeatherFlows()
        {
            var dryWeatherFlows = new List<DryWeatherFlow>();

            dryWeatherFlows.Add(new DryWeatherFlow
            {
                NumberOfUnits = 123,
                DryWeatherFlowId = "First dryweather flow"
            });

            dryWeatherFlows.Add(new DryWeatherFlow
            {
                NumberOfUnits = 456,
                DryWeatherFlowId = "Second dryweather flow"
            });

            return dryWeatherFlows;
        }

        public static IList<DryWeatherFlow> GenerateSingleDryWeatherFlow()
        {
            var dryWeatherFlows = new List<DryWeatherFlow>();

            dryWeatherFlows.Add(new DryWeatherFlow
            {
                NumberOfUnits = 123,
                DryWeatherFlowId = "First dryweather flow"
            });

            return dryWeatherFlows;
        }

        public static IDictionary<NwrwSurfaceType, double> GenerateSurfaceLevelDict()
        {
            var surfaceLevelDict = new Dictionary<NwrwSurfaceType, double>();

            NwrwSurfaceType[] surfaceTypes = NwrwSurfaceTypeHelper.SurfaceTypesInCorrectOrder.ToArray();

            for (int i = 0; i < surfaceTypes.Length; i++)
            {
                surfaceLevelDict.Add(surfaceTypes[i], i);
            }

            return surfaceLevelDict;
        }

        public static EventedList<NwrwDefinition> GenerateNwrwDefinitions()
        {
            var nwrwDefinitions = new EventedList<NwrwDefinition>();
            var gvhHelDefinition = new NwrwDefinition()
            {
                Name = "GVH_HEL",
                SurfaceStorage = 0,
                InfiltrationCapacityMax = 0,
                InfiltrationCapacityMin = 0,
                InfiltrationCapacityReduction = 0,
                InfiltrationCapacityRecovery = 0,
                RunoffDelay = 0.5,
                SurfaceType = NwrwSurfaceType.ClosedPavedWithSlope
            };
            nwrwDefinitions.Add(gvhHelDefinition);

            var gvhVlaDefinition = new NwrwDefinition()
            {
                Name = "GVH_VLA",
                SurfaceStorage = 0.5,
                InfiltrationCapacityMax = 0,
                InfiltrationCapacityMin = 0,
                InfiltrationCapacityReduction = 0,
                InfiltrationCapacityRecovery = 0,
                RunoffDelay = 0.2,
                SurfaceType = NwrwSurfaceType.ClosedPavedFlat
            };
            nwrwDefinitions.Add(gvhVlaDefinition);

            var gvhVluDefinition = new NwrwDefinition()
            {
                Name = "GVH_VLU",
                SurfaceStorage = 1,
                InfiltrationCapacityMax = 0,
                InfiltrationCapacityMin = 0,
                InfiltrationCapacityReduction = 0,
                InfiltrationCapacityRecovery = 0,
                RunoffDelay = 0.1,
                SurfaceType = NwrwSurfaceType.ClosedPavedFlatStretch
            };
            nwrwDefinitions.Add(gvhVluDefinition);

            var ovhHelDefinition = new NwrwDefinition()
            {
                Name = "OVH_HEL",
                SurfaceStorage = 0,
                InfiltrationCapacityMax = 2,
                InfiltrationCapacityMin = 0.5,
                InfiltrationCapacityReduction = 3,
                InfiltrationCapacityRecovery = 0.1,
                RunoffDelay = 0.5,
                SurfaceType = NwrwSurfaceType.OpenPavedWithSlope
            };
            nwrwDefinitions.Add(ovhHelDefinition);

            var ovhVlaDefinition = new NwrwDefinition()
            {
                Name = "OVH_VLA",
                SurfaceStorage = 0.5,
                InfiltrationCapacityMax = 2,
                InfiltrationCapacityMin = 0.5,
                InfiltrationCapacityReduction = 3,
                InfiltrationCapacityRecovery = 0.1,
                RunoffDelay = 0.2,
                SurfaceType = NwrwSurfaceType.OpenPavedFlat
            };
            nwrwDefinitions.Add(ovhVlaDefinition);

            var ovhVluDefinition = new NwrwDefinition()
            {
                Name = "OVH_VLU",
                SurfaceStorage = 1,
                InfiltrationCapacityMax = 2,
                InfiltrationCapacityMin = 0.5,
                InfiltrationCapacityReduction = 3,
                InfiltrationCapacityRecovery = 0.1,
                RunoffDelay = 0.1,
                SurfaceType = NwrwSurfaceType.OpenPavedFlatStretched
            };
            nwrwDefinitions.Add(ovhVluDefinition);

            var dakHelDefinition = new NwrwDefinition()
            {
                Name = "DAK_HEL",
                SurfaceStorage = 0,
                InfiltrationCapacityMax = 0,
                InfiltrationCapacityMin = 0,
                InfiltrationCapacityReduction = 0,
                InfiltrationCapacityRecovery = 0,
                RunoffDelay = 0.5,
                SurfaceType = NwrwSurfaceType.RoofWithSlope
            };
            nwrwDefinitions.Add(dakHelDefinition);

            var dakVlaDefinition = new NwrwDefinition()
            {
                Name = "DAK_VLA",
                SurfaceStorage = 2,
                InfiltrationCapacityMax = 0,
                InfiltrationCapacityMin = 0,
                InfiltrationCapacityReduction = 0,
                InfiltrationCapacityRecovery = 0,
                RunoffDelay = 0.2,
                SurfaceType = NwrwSurfaceType.RoofFlat
            };
            nwrwDefinitions.Add(dakVlaDefinition);

            var dakVluDefinition = new NwrwDefinition()
            {
                Name = "dak_VLU",
                SurfaceStorage = 4,
                InfiltrationCapacityMax = 0,
                InfiltrationCapacityMin = 0,
                InfiltrationCapacityReduction = 0,
                InfiltrationCapacityRecovery = 0,
                RunoffDelay = 0.1,
                SurfaceType = NwrwSurfaceType.RoofFlatStretched
            };
            nwrwDefinitions.Add(dakVluDefinition);

            var onvHelDefinition = new NwrwDefinition()
            {
                Name = "ONV_HEL",
                SurfaceStorage = 2,
                InfiltrationCapacityMax = 5,
                InfiltrationCapacityMin = 1,
                InfiltrationCapacityReduction = 3,
                InfiltrationCapacityRecovery = 0.1,
                RunoffDelay = 0.5,
                SurfaceType = NwrwSurfaceType.UnpavedWithSlope
            };
            nwrwDefinitions.Add(onvHelDefinition);

            var onvVlaDefinition = new NwrwDefinition()
            {
                Name = "ONV_VLA",
                SurfaceStorage = 4,
                InfiltrationCapacityMax = 5,
                InfiltrationCapacityMin = 1,
                InfiltrationCapacityReduction = 3,
                InfiltrationCapacityRecovery = 0.1,
                RunoffDelay = 0.2,
                SurfaceType = NwrwSurfaceType.UnpavedFlat
            };
            nwrwDefinitions.Add(onvVlaDefinition);

            var onvVluDefinition = new NwrwDefinition()
            {
                Name = "ONV_VLU",
                SurfaceStorage = 6,
                InfiltrationCapacityMax = 5,
                InfiltrationCapacityMin = 1,
                InfiltrationCapacityReduction = 3,
                InfiltrationCapacityRecovery = 0.1,
                RunoffDelay = 0.1,
                SurfaceType = NwrwSurfaceType.UnpavedFlatStretched
            };
            nwrwDefinitions.Add(onvVluDefinition);

            return nwrwDefinitions;

        }

        public static IEventedList<NwrwDryWeatherFlowDefinition> GenerateNwrwDryWeatherFlowDefinitions()
        {
            var dryweatherFlowDefinitions = new EventedList<NwrwDryWeatherFlowDefinition>();
            
            var constantDwfDefinition = new NwrwDryWeatherFlowDefinition()
            {
                Name = "Constant DWF definition",
                DailyVolumeConstant = 240,
                DailyVolumeVariable = 50,
                DayNumber = 3,
                DistributionType = DryweatherFlowDistributionType.Constant,
                HourlyPercentageDailyVolume = new double[]
                    {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23},
            };
            dryweatherFlowDefinitions.Add(constantDwfDefinition);

            var dailyDwfDefinition = new NwrwDryWeatherFlowDefinition()
            {
                Name = "Daily DWF definition",
                DailyVolumeConstant = 2952,
                DailyVolumeVariable = 456,
                DayNumber = 7,
                DistributionType = DryweatherFlowDistributionType.Daily,
                HourlyPercentageDailyVolume = new double[]
                    {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24},
            };
            dryweatherFlowDefinitions.Add(dailyDwfDefinition);

            return dryweatherFlowDefinitions;
        }
    }
}