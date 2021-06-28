using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers.Concepts;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.ModelApiControllers.Concepts
{
    [TestFixture]
    public class PavedModelApiControllerTest
    {
        private readonly MockRepository mocks = new MockRepository();

        [Test]
        public void AddPaved()
        {
            var links = new List<ModelLink>();
            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var controller = new PavedModelController();
            var pavedData = CreatePavedArea();

            pavedData.CapacityMixedAndOrRainfall = 100;
            pavedData.CapacityDryWeatherFlow = 200;

            writer.Expect(fileWriter =>
                fileWriter.AddPaved(pavedData.Name, 2000, 1.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, SewerType.Mixed, true,
                             100.0 / 60.0,
                             200.0 / 60.0, LinkType.WasteWaterTreatmentPlant, LinkType.WasteWaterTreatmentPlant, 0,
                             DwfComputationOption.NumberOfInhabitantsTimesConstantDWF,
                             new[]
                                 {
                                     0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                     0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0}, 0.0, pavedData.Name, 1.0, 0d, 0d)).Return(1).Repeat.Once();

            mocks.ReplayAll();

            controller.Writer = writer;
            controller.AddArea(null, pavedData, links, new List<IFeature>());

            mocks.VerifyAll();
        }

        [Test]
        public void AddPavedWithMeteoPerStation()
        {
            var links = new List<ModelLink>();
            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var controller = new PavedModelController();
            var pavedData = CreatePavedArea();

            var model = new RainfallRunoffModel();

            pavedData.AreaAdjustmentFactor = 0.8;

            writer.Expect(fileWriter =>
                fileWriter.AddPaved(pavedData.Name, 2000, 1.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, SewerType.Mixed, true,
                             0,
                             0, LinkType.WasteWaterTreatmentPlant, LinkType.WasteWaterTreatmentPlant, 0,
                             DwfComputationOption.NumberOfInhabitantsTimesConstantDWF,
                             new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                     0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0}, 0.0, pavedData.Name, 1.0, 0d, 0d)).Return(1).Repeat.Once();
            writer.Expect(fileWriter =>
                            fileWriter.AddPaved(pavedData.Name, 2000, 1.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, SewerType.Mixed, true,
                                         0,
                                         0, LinkType.WasteWaterTreatmentPlant,
                                         LinkType.WasteWaterTreatmentPlant, 0,
                                         DwfComputationOption.NumberOfInhabitantsTimesConstantDWF,
                                         new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                                 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0}, 
                                                 0.0, "", 0.8, 0d, 0d)).Return(1).Repeat.Once();
            mocks.ReplayAll();

            controller.Writer = writer;
            controller.AddArea(model, pavedData, links, new List<IFeature>());

            model.Precipitation.DataDistributionType = MeteoDataDistributionType.PerStation;

            controller.AddArea(model, pavedData, links, new List<IFeature>());

            mocks.VerifyAll();
        }

        [Test]
        public void AddPavedWithStorage()
        {
            var links = new List<ModelLink>();
            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var controller = new PavedModelController();
            var pavedData = CreatePavedArea();

            pavedData.InitialStreetStorage = 5;
            pavedData.MaximumStreetStorage = 6;
            pavedData.InitialSewerDryWeatherFlowStorage = 7;
            pavedData.MaximumSewerDryWeatherFlowStorage = 8;

            writer.Expect(fileWriter =>
                fileWriter.AddPaved(pavedData.Name, 2000, 1.5, 2.5, 3, 0, 0, 3.5, 4, SewerType.Mixed, true,
                             0, 0, LinkType.WasteWaterTreatmentPlant, LinkType.WasteWaterTreatmentPlant, 0,
                             DwfComputationOption.NumberOfInhabitantsTimesConstantDWF,
                             new[]
                                 {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                  0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0}, 0.0, pavedData.Name, 1.0, 0d, 0d)).Return(1).Repeat.Once();

            mocks.ReplayAll();

            controller.Writer = writer;
            controller.AddArea(null, pavedData, links, new List<IFeature>());

            mocks.VerifyAll();
        }

        [Test]
        public void AddPavedWithVariablePumpCapacity()
        {
            var links = new List<ModelLink>();
            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var controller = new PavedModelController();
            var pavedData = CreatePavedArea();

            pavedData.SewerType = PavedEnums.SewerType.ImprovedSeparateSystem;
            pavedData.CapacityMixedAndOrRainfall = 100;
            pavedData.CapacityDryWeatherFlow = 200;
            pavedData.IsSewerPumpCapacityFixed = false;
            pavedData.SpillingDefinition = PavedEnums.SpillingDefinition.UseRunoffCoefficient;
            pavedData.RunoffCoefficient = 0.5;
            pavedData.MixedSewerPumpVariableCapacitySeries[new DateTime(2000, 1, 1)] = 1.0;
            pavedData.MixedSewerPumpVariableCapacitySeries[new DateTime(2001, 1, 1)] = 2.0;
            pavedData.MixedSewerPumpVariableCapacitySeries[new DateTime(2002, 1, 1)] = 3.0;
            pavedData.MixedSewerPumpVariableCapacitySeries[new DateTime(2003, 1, 1)] = 4.0;
            pavedData.MixedSewerPumpVariableCapacitySeries[new DateTime(2004, 1, 1)] = 5.0;
            pavedData.DwfSewerPumpVariableCapacitySeries[new DateTime(2000, 2, 1)] = 5.0;
            pavedData.DwfSewerPumpVariableCapacitySeries[new DateTime(2001, 2, 1)] = 4.0;
            pavedData.DwfSewerPumpVariableCapacitySeries[new DateTime(2002, 2, 1)] = 3.0;
            pavedData.DwfSewerPumpVariableCapacitySeries[new DateTime(2003, 2, 1)] = 2.0;
            pavedData.DwfSewerPumpVariableCapacitySeries[new DateTime(2004, 2, 1)] = 1.0;

            writer.Expect(fileWriter =>
                fileWriter.AddPaved(pavedData.Name, 2000, 1.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, SewerType.MixedSeparated, false,
                             0.0, 0.0, LinkType.WasteWaterTreatmentPlant, LinkType.WasteWaterTreatmentPlant, 0,
                             DwfComputationOption.NumberOfInhabitantsTimesConstantDWF,
                             new[]
                                 {
                                     0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                     0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0}, 0.5, pavedData.Name, 1.0, 0d, 0d)).Return(1).Repeat.Once();
            
            writer.Expect(fileWriter =>
                fileWriter.SetPavedVariablePumpCapacities(1, null, null, null, null)).IgnoreArguments().Repeat.Once().
                    WhenCalled(m =>
                                   {
                                       Assert.AreEqual(1, m.Arguments[0]);
                                       Assert.That(m.Arguments[1], Is.EqualTo(new[]
                                                                                  {
                                                                                      20000101, 20000201, 20010101,
                                                                                      20010201, 20020101, 20020201,
                                                                                      20030101, 20030201, 20040101,
                                                                                      20040201
                                                                                  }));
                                       Assert.That(m.Arguments[2], Is.EqualTo(new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}));
                                       Assert.That(m.Arguments[3], Is.EqualTo(new[]
                                                                                  {
                                                                                      1, 1.08469945355191, 2,
                                                                                      2.08493150684932, 3,
                                                                                      3.08493150684932, 4
                                                                                      , 4.08493150684932, 5, 5
                                                                                  }).Within(0.001));
                                       Assert.That(m.Arguments[4], Is.EqualTo(new[]
                                                                                  {
                                                                                      5, 5, 4.08469945355191, 4,
                                                                                      3.08493150684932, 3,
                                                                                      2.08493150684932
                                                                                      , 2, 1.08493150684932, 1
                                                                                  }).Within(0.001));
                                   });
               
            mocks.ReplayAll();

            controller.Writer = writer;
            controller.AddArea(null, pavedData, links, new List<IFeature>());

            mocks.VerifyAll();
        }

        private static PavedData CreatePavedArea()
        {
            return new PavedData(new Catchment()){CalculationArea = 2000};
        }
    }
}