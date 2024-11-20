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
    class SacramentoModelApiControllerTest
    {
        private readonly MockRepository mocks = new MockRepository();

        [Test]
        public void AddDefaultSacramento()
        {
            var links = new List<ModelLink>();
            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var controller = new SacramentoModelController();
            const string catchmentId = "catchment";
            var sacramentoData = new SacramentoData(new Catchment {Name = catchmentId});
            var parameters = Enumerable.Repeat(0.0, 12).ToArray();
            var capacities = Enumerable.Repeat(0.0, 13).ToArray();

            // non-zero defaults:
            capacities[0] = 1.0;
            capacities[2] = 1.0;
            capacities[4] = 1.0;

            var hydrographValues = Enumerable.Repeat(0.0, 36).ToArray();
            hydrographValues[0] = 1;

            writer.Expect(fileWriter => fileWriter.AddSacramento(catchmentId, 0, parameters, capacities, 0, hydrographValues, "", 0d, 0d))
                    .Return(1)
                    .Repeat.Once();
            
            mocks.ReplayAll();

            sacramentoData.Catchment.Geometry = null;
            controller.Writer = writer;
            controller.AddArea(null, sacramentoData, links, new List<IFeature>());

            mocks.VerifyAll();
        }

        [Test]
        public void AddCustomSacramento()
        {
            var links = new List<ModelLink>();
            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var controller = new SacramentoModelController();
            const string catchmentId = "catchment";
            var sacramentoData = new SacramentoData(new Catchment {Name = catchmentId})
                {
                    // area
                    CalculationArea = 100000,
                    AreaAdjustmentFactor = 0.1,

                    // area parameters
                    PercolationIncrease = 0.2,
                    PercolationExponent = 0.3,
                    PercolatedWaterFraction = 0.4,
                    FreeWaterFraction = 0.5,
                    PermanentlyImperviousFraction = 0.6,
                    RainfallImperviousFraction = 0.7,
                    WaterAndVegetationAreaFraction = 0.8,
                    RatioUnobservedToObservedBaseFlow = 0.9,
                    SubSurfaceOutflow = 1,
                    TimeIntervalIncrement = 1.1,
                    LowerRainfallThreshold = 1.2,
                    UpperRainfallThreshold = 1.3,

                    // capacity parameters                  
                    UpperZoneTensionWaterStorageCapacity = 100,
                    UpperZoneTensionWaterInitialContent = 110,
                    UpperZoneFreeWaterStorageCapacity = 120,
                    UpperZoneFreeWaterInitialContent = 130,
                    LowerZoneTensionWaterStorageCapacity = 140,
                    LowerZoneTensionWaterInitialContent = 150,
                    LowerZoneSupplementalFreeWaterStorageCapacity = 160,
                    LowerZoneSupplementalFreeWaterInitialContent = 170,
                    LowerZonePrimaryFreeWaterStorageCapacity = 180,
                    LowerZonePrimaryFreeWaterInitialContent = 190,                   
                    UpperZoneFreeWaterDrainageRate = 0.2,
                    LowerZoneSupplementalFreeWaterDrainageRate = 0.21,
                    LowerZonePrimaryFreeWaterDrainageRate = 0.22,
                    HydrographStep = 1
                };

            for (int i = 0; i < 36; i++)
            {
                sacramentoData.HydrographValues[i] = 1.0/(1.0 + i*i);
            }

            var parameters = new[] {0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1, 1.1, 1.2, 1.3};
            var capacities = new[] {100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 0.2, 0.21, 0.22};
            var hydrographValues = Enumerable.Range(0, 36).Select(i => 1.0/(1.0 + i*i)).ToArray();

            writer.Expect(fileWriter => fileWriter.AddSacramento(catchmentId, 100000, parameters, capacities, 1, hydrographValues, "", 0d, 0d))
                    .Return(1)
                    .Repeat.Once();

            mocks.ReplayAll();

            sacramentoData.Catchment.Geometry = null;
            controller.Writer = writer;
            controller.AddArea(null, sacramentoData, links, new List<IFeature>());

            mocks.VerifyAll();
        }
    }
}
