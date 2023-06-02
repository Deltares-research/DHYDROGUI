using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers.Concepts;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.ModelApiControllers.Concepts
{
    [TestFixture]
    public class UnpavedModelApiControllerTest
    {
        private readonly MockRepository mocks = new MockRepository();
        
        [Test]
        public void AddUnpaved()
        {
            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var model = mocks.StrictMock<IRainfallRunoffModel>();
            model.Expect(m => m.StartTime).Return(new DateTime(2000, 1, 1)).Repeat.Any();
            model.Expect(m => m.CapSim).Return(false).Repeat.Once();

            var controller = new UnpavedModelController();
            var unpavedData = CreateUnpavedArea();

            var links = new List<ModelLink>();

            writer.Expect(fileWriter =>
                fileWriter.AddUnpaved(unpavedData.Name, new double[] { 1000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                               1000.0, 1.5, DrainageComputationOption.KrayenhoffVdLeur, 0.0, 0.0, 0.0,
                               5.0, 1, 0, 1.5, 5.0, unpavedData.Name, 1.0, 0d, 0d)).Repeat.Once().Return(1);

            writer.Expect(fileWriter => fileWriter.SetUnpavedConstantSeepage(1, 0.0)).Repeat.Once();

            mocks.ReplayAll();

            unpavedData.Catchment.Geometry = null;
            controller.Writer = writer;
            controller.AddArea(model, unpavedData, links, new List<IFeature>());

            mocks.VerifyAll();
        }
        
        [Test]
        public void AddUnpavedWithCapSimSoil()
        {
            var unpavedData = CreateUnpavedArea();
            unpavedData.SoilTypeCapsim = UnpavedEnums.SoilTypeCapsim.soiltype_capsim_20;

            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var model = mocks.StrictMock<IRainfallRunoffModel>();
            model.Expect(m => m.StartTime).Return(new DateTime(2000, 1, 1)).Repeat.Any();
            model.Expect(m => m.CapSim).Return(true).Repeat.Once();
            
            var controller = new UnpavedModelController();
            var links = new List<ModelLink>();

            writer.Expect(fileWriter =>
                fileWriter.AddUnpaved(unpavedData.Name, new double[] { 1000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                               1000.0, 1.5, DrainageComputationOption.KrayenhoffVdLeur, 0.0, 0.0, 0.0, 5.0, 
                               120, //capsim soil type
                               0, 1.5, 5.0, unpavedData.Name, 1.0, 0d, 0d)).Repeat.Once().Return(1);

            writer.Expect(fileWriter => fileWriter.SetUnpavedConstantSeepage(1, 0.0)).Repeat.Once();

            mocks.ReplayAll();

            unpavedData.Catchment.Geometry = null;
            controller.Writer = writer;
            controller.AddArea(model, unpavedData, links, new List<IFeature>());

            mocks.VerifyAll();
        }

        [Test]
        public void AddUnpavedWithMeteoStation()
        {
            var unpavedData = CreateUnpavedArea();
            unpavedData.AreaAdjustmentFactor = 0.945;
            unpavedData.MeteoStationName = "blah";

            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var model = new RainfallRunoffModel();
            model.Precipitation.DataDistributionType = MeteoDataDistributionType.PerStation;
            model.MeteoStations.Add("blah");

            var controller = new UnpavedModelController();
            var links = new List<ModelLink>();

            writer.Expect(fileWriter =>
                fileWriter.AddUnpaved(unpavedData.Name, new double[] { 1000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                               1000.0, 1.5, DrainageComputationOption.KrayenhoffVdLeur, 0.0, 0.0, 0.0, 5.0,
                               1, 0, 1.5, 5.0, "blah", 0.945, 0d, 0d)).Repeat.Once().Return(1);

            writer.Expect(fileWriter => fileWriter.SetUnpavedConstantSeepage(1, 0.0)).Repeat.Once();

            mocks.ReplayAll();

            unpavedData.Catchment.Geometry = null;
            controller.Writer = writer;
            controller.AddArea(model, unpavedData, links, new List<IFeature>());

            mocks.VerifyAll();
        }

        [Test]
        public void AddUnpavedInitialGroundwaterLevelFromBoundary()
        {
            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var model = mocks.StrictMock<IRainfallRunoffModel>();
            var rootController = mocks.StrictMock<IRainfallRunoffModelController>();
            var flowWaterLevel = mocks.Stub<IFeatureCoverage>();
            var startTime = new DateTime(2000, 1, 1);

            flowWaterLevel.Evaluate(default(DateTime), null); LastCall.IgnoreArguments().Return(RainfallRunoffModelDataSet.UndefinedWaterLevel); 

            var unpavedData = CreateUnpavedArea();
            unpavedData.Catchment.Links.Add(new HydroLink(unpavedData.Catchment, new LateralSource())); //linked to a lateral
            unpavedData.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.FromLinkedNode;
            unpavedData.BoundarySettings.BoundaryData.Data[startTime] = -5.0;

            model.Expect(m => m.StartTime).Return(startTime).Repeat.Any(); 
            model.Expect(m => m.CapSim).Return(false).Repeat.Once();
            
            rootController.Expect(c => c.GetWaterLevelAtBoundary(unpavedData.Catchment)).Return(-5);

            var controller = new UnpavedModelController(); 
            var links = new List<ModelLink>();

            writer.Expect(fileWriter =>
                fileWriter.AddUnpaved(unpavedData.Name, new double[] { 1000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                               1000.0, 1.5, DrainageComputationOption.KrayenhoffVdLeur, 0.0, 0.0, 0.0,
                               5.0, 1, 5.0 + 1.5, 1.5, 5.0, unpavedData.Name, 1.0, 0d, 0d)).Repeat.Once().Return(1);

            writer.Expect(fileWriter => fileWriter.SetUnpavedConstantSeepage(1, 0.0)).Repeat.Once();

            mocks.ReplayAll();

            unpavedData.Catchment.Geometry = null;
            controller.Writer = writer;
            controller.RootController = rootController;
            controller.AddArea(model, unpavedData, links, new List<IFeature>());

            mocks.VerifyAll();
        }


        private static UnpavedData CreateUnpavedArea()
        {
            var unpaved = new UnpavedData(new Catchment()) {CalculationArea = 1000};
            unpaved.SwitchDrainageFormula<KrayenhoffVanDeLeurDrainageFormula>();
            return unpaved;
        }

        [Test]
        public void SendErnst()
        {
            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var model = mocks.StrictMock<IRainfallRunoffModel>();
            model.Expect(m => m.StartTime).Return(new DateTime(2000, 1, 1)).Repeat.Any();
            model.Expect(m => m.CapSim).Return(false).Repeat.Once();

            var controller = new UnpavedModelController();

            var unpavedData = CreateUnpavedArea();
            unpavedData.SwitchDrainageFormula<ErnstDrainageFormula>();
            var ernst = (ErnstDrainageFormula)unpavedData.DrainageFormula;

            ernst.LevelOneEnabled = true;
            ernst.LevelTwoEnabled = true;
            ernst.LevelThreeEnabled = false;
            ernst.LevelOneValue = 1;
            ernst.LevelTwoValue = 2;
            ernst.LevelThreeValue = 3;
            ernst.LevelOneTo = 5;
            ernst.LevelTwoTo = 6;
            ernst.LevelThreeTo = 7;
            ernst.InfiniteDrainageLevelRunoff = 66;
            ernst.HorizontalInflow = 33;
            ernst.SurfaceRunoff = 22;

            var links = new List<ModelLink>();

            var iref = 3;

            writer.Expect(
                fileWriter => fileWriter.AddUnpaved(null, null, 0, 0, DrainageComputationOption.Ernst, 0, 0, 0,
                               0, 0, 0, 0, 0, null, 1.0, 0d, 0d)).IgnoreArguments().Repeat.Once().Return(iref);
            writer.Expect(fileWriter => fileWriter.SetUnpavedConstantSeepage(iref, 0.0)).Repeat.Once();

            writer.Expect(fileWriter => fileWriter.SetErnst(iref, 22, 66, 33, new double[] { 0, 5, 6 }, new double[] { 0, 1, 2 })).
                Repeat.Once().Return(1);

            mocks.ReplayAll();

            controller.Writer = writer;
            controller.AddArea(model, unpavedData, links, new List<IFeature>());

            mocks.VerifyAll();
        }

        [Test]
        public void SendDeZeeuwHellinga()
        {
            var writer = mocks.StrictMock<IRRModelHybridFileWriter>();
            var model = mocks.StrictMock<IRainfallRunoffModel>();
            model.Expect(m => m.StartTime).Return(new DateTime(2000, 1, 1)).Repeat.Any();
            model.Expect(m => m.CapSim).Return(false).Repeat.Once();

            var controller = new UnpavedModelController();

            var unpavedData = CreateUnpavedArea();
            unpavedData.SwitchDrainageFormula<DeZeeuwHellingaDrainageFormula>();
            var deZeeuw = (DeZeeuwHellingaDrainageFormula)unpavedData.DrainageFormula;

            deZeeuw.LevelOneEnabled = true;
            deZeeuw.LevelTwoEnabled = true;
            deZeeuw.LevelThreeEnabled = false;
            deZeeuw.LevelOneValue = 1;
            deZeeuw.LevelTwoValue = 2;
            deZeeuw.LevelThreeValue = 3;
            deZeeuw.LevelOneTo = 5;
            deZeeuw.LevelTwoTo = 6;
            deZeeuw.LevelThreeTo = 7;
            deZeeuw.InfiniteDrainageLevelRunoff = 66;
            deZeeuw.HorizontalInflow = 33;
            deZeeuw.SurfaceRunoff = 22;

            var links = new List<ModelLink>();

            var iref = 3;

            writer.Expect(
                fileWriter => fileWriter.AddUnpaved(null, null, 0, 0, DrainageComputationOption.DeZeeuwHellinga, 0, 0, 0,
                               0, 0, 0, 0, 0, null, 1.0, 0d, 0d)).IgnoreArguments().Repeat.Once().Return(iref);
            writer.Expect(fileWriter => fileWriter.SetUnpavedConstantSeepage(iref, 0.0)).Repeat.Once();

            writer.Expect(
                fileWriter => fileWriter.SetDeZeeuwHellinga(iref, 22, 66, 33, new double[] {0, 5, 6}, new double[] {0, 1, 2})).
                Repeat.Once().Return(1);

            mocks.ReplayAll();

            controller.Writer = writer;
            controller.AddArea(model, unpavedData, links, new List<IFeature>());

            mocks.VerifyAll();
        }
    }
}