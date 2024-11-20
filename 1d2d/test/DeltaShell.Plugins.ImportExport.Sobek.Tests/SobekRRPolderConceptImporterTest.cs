using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class SobekRRPolderConceptImporterTest
    {
        RainfallRunoffModel tholenModel;
        
        private static HydroModel CreateHydroModelWithRR()
        {
            var hydroModel = new HydroModel();
            var network = new HydroNetwork();
            hydroModel.Region.SubRegions.Add(network);
            var basin = new DrainageBasin();
            hydroModel.Region.SubRegions.Add(basin);

            var rainfallRunoffModel = new RainfallRunoffModel();
            hydroModel.Activities.Add(rainfallRunoffModel);

            rainfallRunoffModel.GetDataItemByValue(rainfallRunoffModel.Basin).LinkTo(hydroModel.GetDataItemByValue(basin));
            return hydroModel;
        }

        private void SetUpTholen()
        {
            if (tholenModel != null)
                return; //already loaded: speeds up tests

            string file = TestHelper.GetTestDataDirectory() + @"\Tholen.lit\29\NETWORK.TP";
            var hydroModel = CreateHydroModelWithRR();
            tholenModel = hydroModel.Activities.OfType<RainfallRunoffModel>().First();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(file, hydroModel);
            importer.Import();
        }

        [Test]
        public void ImportMiniModelCheckErnst()
        {
            string file = TestHelper.GetTestDataDirectory() + @"\RRMiniTestModels\DRRSA.lit\8\NETWORK.TP";

            var simpleModel = new RainfallRunoffModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(file, simpleModel);

            importer.Import();
            Assert.AreEqual(2, simpleModel.GetAllModelData().Count());
            var firstUnpaved = simpleModel.GetAllModelData().OfType<UnpavedData>().First();

            var ernst = firstUnpaved.DrainageFormula as ErnstDrainageFormula;

            Assert.IsNotNull(ernst);
            Assert.AreEqual(true, ernst.LevelOneEnabled);
            Assert.AreEqual(true, ernst.LevelTwoEnabled);
            Assert.AreEqual(true, ernst.LevelThreeEnabled);
            
            Assert.AreEqual(0.4, ernst.LevelOneTo);
            Assert.AreEqual(0.55, ernst.LevelTwoTo);
            Assert.AreEqual(0.86, ernst.LevelThreeTo);

            Assert.AreEqual(4.895, ernst.LevelOneValue);
            Assert.AreEqual(6.234, ernst.LevelTwoValue);
            Assert.AreEqual(7.143, ernst.LevelThreeValue);
        }

        [Test]
        public void ImportMiniModelCheckPaved()
        {
            string file = TestHelper.GetTestDataDirectory() + @"\RRMiniTestModels\DRRSA.lit\15\NETWORK.TP";

            var simpleModel = new RainfallRunoffModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(file, simpleModel);
            importer.Import();
            var areas = simpleModel.GetAllModelData();
            Assert.AreEqual(2, areas.Count());
            var firstPaved = simpleModel.GetAllModelData().OfType<PavedData>().First();
            var secondPaved = simpleModel.GetAllModelData().OfType<PavedData>().Skip(1).First();
            
            Assert.AreEqual(0, firstPaved.RunoffCoefficient);
            Assert.AreEqual(PavedEnums.SpillingDefinition.NoDelay, firstPaved.SpillingDefinition);
            Assert.AreEqual(PavedEnums.SewerType.SeparateSystem, firstPaved.SewerType);
            Assert.IsNotNull(firstPaved.MixedSewerPumpVariableCapacitySeries);
            Assert.IsNotNull(firstPaved.DwfSewerPumpVariableCapacitySeries);

            Assert.AreEqual("Station1", firstPaved.MeteoStationName);
            Assert.AreEqual(1.0, firstPaved.AreaAdjustmentFactor);

            Assert.AreEqual(63, firstPaved.MixedSewerPumpVariableCapacitySeries.Time.Values.Count);
            Assert.AreEqual(63, firstPaved.DwfSewerPumpVariableCapacitySeries.Time.Values.Count);

            Assert.IsTrue(firstPaved.MixedSewerPumpVariableCapacitySeries.Time.Values.OfType<DateTime>().
                All(t => t >= new DateTime(2005, 12, 1) && t <= new DateTime(2006, 2, 1)));
            Assert.IsTrue(firstPaved.MixedSewerPumpVariableCapacitySeries.Components[0].Values.OfType<double>().
                All(v => v > 0 && v < 0.02));
            Assert.IsTrue(firstPaved.DwfSewerPumpVariableCapacitySeries.Components[0].Values.OfType<double>().
                All(v => v > 0.6 && v < 0.7));

            Assert.AreEqual(0.5, secondPaved.RunoffCoefficient);
            Assert.AreEqual(PavedEnums.SpillingDefinition.UseRunoffCoefficient, secondPaved.SpillingDefinition);
        }

        [Test]
        public void ImportMiniModelCheckSewerTypeImproved()
        {
            string file = TestHelper.GetTestDataDirectory() + @"\RRMiniTestModels\DRRSA.lit\4\NETWORK.TP";

            var simpleModel = new RainfallRunoffModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(file, simpleModel);
            importer.Import();
            var areas = simpleModel.GetAllModelData();
            Assert.AreEqual(2, areas.Count());
            var firstPaved = simpleModel.GetAllModelData().OfType<PavedData>().First();
            var secondPaved = simpleModel.GetAllModelData().OfType<PavedData>().Skip(1).First();

            Assert.AreEqual(0, firstPaved.RunoffCoefficient);
            Assert.AreEqual(PavedEnums.SpillingDefinition.NoDelay, firstPaved.SpillingDefinition);
            Assert.AreEqual(PavedEnums.SewerType.ImprovedSeparateSystem, firstPaved.SewerType);

            Assert.AreEqual(0.5, secondPaved.RunoffCoefficient);
            Assert.AreEqual(PavedEnums.SpillingDefinition.UseRunoffCoefficient, secondPaved.SpillingDefinition);
        }

        [Test]
        public void ImportMiniModelCheckGreenhouseArea()
        {
            string file = TestHelper.GetTestDataDirectory() + @"\RRMiniTestModels\DRRSA.lit\16\NETWORK.TP";

            var simpleModel = new RainfallRunoffModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(file, simpleModel);
            importer.Import();
            var areas = simpleModel.GetAllModelData();
            Assert.AreEqual(2, areas.Count());
            var secondGreenhouse = simpleModel.GetAllModelData().OfType<GreenhouseData>().Skip(1).First();
            
            Assert.AreEqual(1605.8, secondGreenhouse.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.moreThan6000], 0.001);
        }

        [Test]
        public void ImportMiniModelCheckGreenhouseStorageAndCapacities()         // fix for JIRA issues 6372 and 6373
        {
            string file = TestHelper.GetTestDataDirectory() + @"\RRMiniTestModels\DRRSA.lit\16\NETWORK.TP";

            var simpleModel = new RainfallRunoffModel();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(file, simpleModel);
            importer.Import();
            var areas = simpleModel.GetAllModelData();
            Assert.AreEqual(2, areas.Count());
            var firstGreenhouse = simpleModel.GetAllModelData().OfType<GreenhouseData>().First();
            var secondGreenhouse = simpleModel.GetAllModelData().OfType<GreenhouseData>().Skip(1).First();

            Assert.AreEqual(1.0, firstGreenhouse.MaximumRoofStorage, 1E-5);
            Assert.AreEqual(0.100000001490116, firstGreenhouse.InitialRoofStorage, 1E-5);
            Assert.IsTrue(firstGreenhouse.UseSubsoilStorage);
            Assert.AreEqual(4000.0, firstGreenhouse.SubSoilStorageArea, 1E-5);
            Assert.AreEqual(205.0, firstGreenhouse.SiloCapacity, 1E-5);
            Assert.AreEqual(0.021, firstGreenhouse.PumpCapacity, 1E-5);
            Assert.AreEqual(200.0, secondGreenhouse.SiloCapacity, 1E-5);
            Assert.AreEqual(0.02, secondGreenhouse.PumpCapacity, 1E-5);
            Assert.AreEqual(1.18302595615387, secondGreenhouse.MaximumRoofStorage, 1E-5);
            Assert.AreEqual(5.91512992978096E-02, secondGreenhouse.InitialRoofStorage, 1E-5);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportTholenCheckUnpavedDataCount()
        {
            SetUpTholen();
            Assert.AreEqual(328, tholenModel.GetAllModelData().OfType<UnpavedData>().Count());
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportTholenCheckUnpavedDataOfFirstItem()
        {
            //UNPV id 'upGFE820' na 16 ga 99826 ar 99392 0 0 0 196 238 0 0 0 0 0 0 0 0 0 0 su 0 '' lv -0.18 co 3 ad '' rc 0 ed 'Drain1' sp 'GFE820' ic 'INF1' sd 'STOR1' ig 0  1.07 gl 2 bt 115 is 2566.2 ms 'Station1' aaf 1 unpv

            //ERNS id 'Drain1' nm 'Drain1' cvi 400 cvs 0.2 cvo 0 0 7.5 4000 lv 0 0 1.2 erns

            //SEEP id 'GFE820' nm 'Kwel GFE820' co 5  PDIN 1 0  pdin ss 'GFE820'
            //TBLE
            //'1998/01/01;00:00:00' .132 <
            //'1998/01/11;00:00:00' .141 <
            //'1998/01/21;00:00:00' .34 <
            //'1998/02/01;00:00:00' .211 <

            //INFC id 'INF1' nm 'INF1' ic 99 infc

            //STDF id 'STOR1' nm 'STOR1' ml 10 il 0 stdf

            SetUpTholen();
            var firstUnpavedItem = tholenModel.GetAllModelData().OfType<UnpavedData>().First();

            //cropareas
            Assert.AreEqual(99826.0, firstUnpavedItem.TotalAreaForGroundWaterCalculations);
            Assert.AreEqual(99392.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)0]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)1]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)2]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)3]);
            Assert.AreEqual(196.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)4]);
            Assert.AreEqual(238.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)5]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)6]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)7]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)8]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)9]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)10]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)11]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)12]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)13]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)14]);
            Assert.AreEqual(0.0, firstUnpavedItem.AreaPerCrop[(UnpavedEnums.CropType)15]);

            Assert.AreEqual(-0.18,firstUnpavedItem.SurfaceLevel);
            Assert.IsTrue(firstUnpavedItem.DrainageFormula is ErnstDrainageFormula);

            Assert.AreEqual(2.0, firstUnpavedItem.GroundWaterLayerThickness);
            
            Assert.AreEqual("Station1", firstUnpavedItem.MeteoStationName);
            Assert.AreEqual(1.0, firstUnpavedItem.AreaAdjustmentFactor);

            //ernst parameters
            var  ernstDrainageFormula = (ErnstDrainageFormula)firstUnpavedItem.DrainageFormula;

            Assert.AreEqual(true, ernstDrainageFormula.IsErnst);
            Assert.AreEqual(400.0, ernstDrainageFormula.HorizontalInflow);
            Assert.AreEqual(4000.0, ernstDrainageFormula.InfiniteDrainageLevelRunoff);
            Assert.AreEqual(true, ernstDrainageFormula.LevelOneEnabled);
            Assert.AreEqual(1.2, ernstDrainageFormula.LevelOneTo);
            Assert.AreEqual(7.5, ernstDrainageFormula.LevelOneValue);
            Assert.AreEqual(false, ernstDrainageFormula.LevelTwoEnabled);
            Assert.AreEqual(false, ernstDrainageFormula.LevelThreeEnabled);
            Assert.AreEqual(0.2, ernstDrainageFormula.SurfaceRunoff);

            //seepage
            Assert.AreEqual(UnpavedEnums.SeepageSourceType.Series, firstUnpavedItem.SeepageSource);
            Assert.IsNotNull(firstUnpavedItem.SeepageSeries);
            Assert.AreEqual(432,firstUnpavedItem.SeepageSeries.Time.Values.Count());
            Assert.AreEqual(new DateTime(1998,1,1), firstUnpavedItem.SeepageSeries.Time.Values[0]);
            Assert.AreEqual(0.132, firstUnpavedItem.SeepageSeries.Components[0].Values[0]);

            //soil type 115
            Assert.AreEqual(UnpavedEnums.SoilTypeCapsim.soiltype_capsim_15, firstUnpavedItem.SoilTypeCapsim);

        }
        
        [Test]
        [Category(TestCategory.Slow)]
        public void ImportTholenCheckPavedDataCount()
        {
            SetUpTholen();
            Assert.AreEqual(48, tholenModel.GetAllModelData().OfType<PavedData>().Count());
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportTholenCheckPavedDataOfFirstItem()
        {
            //PAVE id 'GS01' ar 5400 lv 9.99 ss 1 sd 'PAV1' qc 0 0 0.0315 qo 2 0 ms 'GFE1000' aaf 1 is 0 np 70 dw '1' ro 0 ru 0 qh '' pave

            //STDF id 'PAV1' nm 'PAV1' ms 1 is 0 mr 0 0 ir 0 0 stdf

            //DWA id '1' nm 'DWF2' do 2 wc 0 wd 125 wh 1.5 1.5 1.5 1.5 1.5 3 4 5 6 6.5 7.5 8.5 7.5 6.5 6 5 5 5 4 3.5 3 2.5 2 2 dwa

            SetUpTholen();
            var pavedData = tholenModel.GetAllModelData().OfType<PavedData>().First();

            Assert.AreEqual(5400, pavedData.CalculationArea);
            Assert.AreEqual(9.99, pavedData.SurfaceLevel);
            Assert.AreEqual(PavedEnums.SewerType.SeparateSystem, pavedData.SewerType);
            Assert.AreEqual(0.0, pavedData.CapacityMixedAndOrRainfall);
            Assert.AreEqual(0.0315, pavedData.CapacityDryWeatherFlow, 0.00001);
            Assert.AreEqual(PavedEnums.SewerPumpDischargeTarget.BoundaryNode, pavedData.MixedAndOrRainfallSewerPumpDischarge);
            Assert.AreEqual(PavedEnums.SewerPumpDischargeTarget.WWTP, pavedData.DryWeatherFlowSewerPumpDischarge);
            Assert.AreEqual(70.0, pavedData.NumberOfInhabitants);
            Assert.AreEqual(0.0, pavedData.RunoffCoefficient);

            //storage
            Assert.AreEqual(1.0, pavedData.MaximumStreetStorage);
            Assert.AreEqual(0.0, pavedData.InitialStreetStorage);
            Assert.AreEqual(0.0, pavedData.MaximumSewerMixedAndOrRainfallStorage);
            Assert.AreEqual(0.0, pavedData.MaximumSewerDryWeatherFlowStorage);
            Assert.AreEqual(0.0, pavedData.InitialSewerMixedAndOrRainfallStorage);
            Assert.AreEqual(0.0, pavedData.InitialSewerDryWeatherFlowStorage);

            Assert.AreEqual("GFE1000", pavedData.MeteoStationName);
            Assert.AreEqual(1.0, pavedData.AreaAdjustmentFactor);

            //dwa
            Assert.AreEqual(PavedEnums.DryWeatherFlowOptions.NumberOfInhabitantsTimesVariableDWF, pavedData.DryWeatherFlowOptions);
            Assert.AreEqual(125.0, pavedData.WaterUse);

            Assert.AreEqual(1.5, pavedData.VariableWaterUseFunction[0]);
            Assert.AreEqual(1.5, pavedData.VariableWaterUseFunction[1]);
            Assert.AreEqual(1.5, pavedData.VariableWaterUseFunction[2]);
            Assert.AreEqual(1.5, pavedData.VariableWaterUseFunction[3]);
            Assert.AreEqual(1.5, pavedData.VariableWaterUseFunction[4]);
            Assert.AreEqual(3.0, pavedData.VariableWaterUseFunction[5]);
            Assert.AreEqual(4.0, pavedData.VariableWaterUseFunction[6]);
            Assert.AreEqual(5.0, pavedData.VariableWaterUseFunction[7]);
            Assert.AreEqual(6.0, pavedData.VariableWaterUseFunction[8]);
            Assert.AreEqual(6.5, pavedData.VariableWaterUseFunction[9]); 
            Assert.AreEqual(7.5, pavedData.VariableWaterUseFunction[10]);
            Assert.AreEqual(8.5, pavedData.VariableWaterUseFunction[11]);
            Assert.AreEqual(7.5, pavedData.VariableWaterUseFunction[12]);
            Assert.AreEqual(6.5, pavedData.VariableWaterUseFunction[13]);
            Assert.AreEqual(6.0, pavedData.VariableWaterUseFunction[14]);
            Assert.AreEqual(5.0, pavedData.VariableWaterUseFunction[15]);
            Assert.AreEqual(5.0, pavedData.VariableWaterUseFunction[16]);
            Assert.AreEqual(5.0, pavedData.VariableWaterUseFunction[17]);
            Assert.AreEqual(4.0, pavedData.VariableWaterUseFunction[18]);
            Assert.AreEqual(3.5, pavedData.VariableWaterUseFunction[19]);
            Assert.AreEqual(3.0, pavedData.VariableWaterUseFunction[20]);
            Assert.AreEqual(2.5, pavedData.VariableWaterUseFunction[21]);
            Assert.AreEqual(2.0, pavedData.VariableWaterUseFunction[22]);
            Assert.AreEqual(2.0, pavedData.VariableWaterUseFunction[23]);
        }
    }
}
