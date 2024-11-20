using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers.Concepts;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.ModelApiControllers.Concepts
{
    [TestFixture]
    public class HbvModelControllerTest
    {
        private readonly MockRepository mocks = new MockRepository();

        [Test]
        public void AddDefaultHbv()
        {
            var links = new List<ModelLink>();
            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var controller = new HbvModelController();
            const string catchmentId = "catchment";
            var HbvData = new HbvData(new Catchment {Name = catchmentId});


            var snowParameters = Enumerable.Repeat(0.0, 6).ToArray();
            var soilParameters = Enumerable.Repeat(0.0, 3).ToArray();
            var flowParameters = Enumerable.Repeat(0.0, 5).ToArray();

            // non-zero defaults:
            flowParameters[0] = 0.1;
            flowParameters[1] = 0.1;
            flowParameters[2] = 0.1;

            var hiniParameters = Enumerable.Repeat(0.0, 5).ToArray();

            writer.Expect(
                    fileWriter =>
                        fileWriter.AddHbv(catchmentId, 0, 0, snowParameters, soilParameters, flowParameters,
                            hiniParameters, "", 1, "", 0d,0d))
                .Return(1)
                .Repeat.Once();

            mocks.ReplayAll();

            HbvData.Catchment.Geometry = null;
            controller.Writer = writer;
            controller.AddArea(null, HbvData, links, new List<IFeature>());

            mocks.VerifyAll();
        }

        [Test]
        public void AddCustomHbv()
        {
            var links = new List<ModelLink>();
            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var controller = new HbvModelController();
            const string catchmentId = "catchment";

            var HbvData = new HbvData(new Catchment {Name = catchmentId})
            {
                // Global parameters
                CalculationArea = 100000,
                SurfaceLevel = 0.2,

                // Meteo parameters
                MeteoStationName = "deBiltMeteo",
                AreaAdjustmentFactor = 0.3,
                TemperatureStationName = "deBiltTemp",

                // Snow parameters
                SnowMeltingConstant = 0.4,
                SnowFallTemperature = 0.5,
                SnowMeltTemperature = 0.6,
                TemperatureAltitudeConstant = 0.7,
                FreezingEfficiency = 0.8,
                FreeWaterFraction = 0.9,

                // Soil parameters
                Beta = 1.0,
                FieldCapacity = 1.1,
                FieldCapacityThreshold = 1.2,

                // Reservoir flow parameters
                BaseFlowReservoirConstant = 1.3,
                InterflowReservoirConstant = 1.4,
                QuickFlowReservoirConstant = 1.5,
                UpperZoneThreshold = 1.6,
                MaximumPercolation = 1.7,

                // Initial water level parameters
                    
                InitialDrySnowContent = 1.8,
                InitialFreeWaterContent = 1.9,
                InitialSoilMoistureContents = 2,
                InitialUpperZoneContent = 2.1,
                InitialLowerZoneContent = 2.2
            };

            var snowParameters = new[] {0.4, 0.5, 0.6, 0.7, 0.8, 0.9};
            var soilParameters = new[] {1.0, 1.1, 1.2};
            var flowParameters = new[] {1.3, 1.4, 1.5, 1.6, 1.7};
            var hiniParameters = new[] {1.8, 1.9, 2, 2.1, 2.2};

            writer.Expect(
                    fileWriter =>
                        fileWriter.AddHbv(catchmentId, 100000, 0.2, snowParameters, soilParameters, flowParameters,
                            hiniParameters, "deBiltMeteo", 0.3, "deBiltTemp", 0d, 0d))
                .Return(1)
                .Repeat.Once();

            mocks.ReplayAll();

            HbvData.Catchment.Geometry = null;
            controller.Writer = writer;
            controller.AddArea(null, HbvData, links, new List<IFeature>());

            mocks.VerifyAll();
        }
    }
}