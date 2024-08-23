using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.Extensions;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffModelTest
    {
        [Test]
        public void EnablingParameterAddsOutputCoverages()
        {
            var model = new RainfallRunoffModel();

            var unpavedRainfallParameter = model.OutputSettings.EngineParameters.First(
                ep => ep.ElementSet == ElementSet.UnpavedElmSet && ep.QuantityType == QuantityType.Rainfall);

            //precondition:
            Assert.AreEqual(false, unpavedRainfallParameter.IsEnabled);

            var numCoverages = model.OutputCoverages.Count();

            unpavedRainfallParameter.IsEnabled = true;

            Assert.AreEqual(numCoverages + 1, model.OutputCoverages.Count());
        }
        
    [Test]
        public void LinkCoverageIsAdded()
        {
            var model = new RainfallRunoffModel();

            var linkParameter = model.OutputSettings.EngineParameters.First(
                ep => ep.ElementSet == ElementSet.LinkElmSet && ep.QuantityType == QuantityType.Flow);

            //precondition:
            Assert.AreEqual(false, linkParameter.IsEnabled);

            var numCoverages = model.OutputCoverages.Count();

            linkParameter.IsEnabled = true;

            Assert.AreEqual(numCoverages + 1, model.OutputCoverages.Count());
        }

        [Test]
        public void AddingModelDataBubblesEvents()
        {
            var model = new RainfallRunoffModel();

            var callCount = 0;
            model.CollectionChanged += (s, e) => { callCount++; };

            model.ModelData.Add(new UnpavedData(new Catchment()));

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void CloneModel()
        {
            var c1 = Catchment.CreateDefault();
            var c2 = Catchment.CreateDefault();
            var c3 = Catchment.CreateDefault();
            var c4 = Catchment.CreateDefault();

            c1.CatchmentType = CatchmentType.Unpaved;
            c2.CatchmentType = CatchmentType.Paved;
            c3.CatchmentType = CatchmentType.GreenHouse;
            c4.CatchmentType = CatchmentType.OpenWater;

            var runoffBoundary = new RunoffBoundary();
            
            // create model and add catchments to its basin
            var model = new RainfallRunoffModel();
            model.Basin.Catchments.AddRange(new[] {c1, c2, c3, c4});
            model.Basin.Boundaries.Add(runoffBoundary);
            Assert.AreEqual(4, model.GetAllModelData().Count(), "before clone");

            var c1Data = ((UnpavedData) model.GetCatchmentModelData(c1));

            var clonedModel = (RainfallRunoffModel) model.DeepClone();
            
            Assert.AreEqual(4, clonedModel.GetAllModelData().Count(), "after clone");
            Assert.AreEqual(1, clonedModel.BoundaryData.Count, "after clone bd");
            Assert.AreEqual(model.AllDataItems.Count(), clonedModel.AllDataItems.Count(), "all data items");
            
            // see if we can find the old catchment still in this model
            var hits = TestReferenceHelper.SearchObjectInObjectGraph(c1, clonedModel);
            hits.ForEach(Console.WriteLine);
            Assert.AreEqual(0, hits.Count, "hit catchment");

            // see if we can find the old unpaved boundary data still in this model
            var hits2 = TestReferenceHelper.SearchObjectInObjectGraph(c1Data.BoundarySettings.BoundaryData, clonedModel);
            hits2.ForEach(Console.WriteLine);
            Assert.AreEqual(0, hits2.Count, "hits unpaved boundary data");

            // see if we can find the old unpaved boundary data still in this model
            var hits3 = TestReferenceHelper.SearchObjectInObjectGraph(runoffBoundary, clonedModel);
            hits3.ForEach(Console.WriteLine);
            Assert.AreEqual(0, hits3.Count, "runoff boundary");
        }

        [Test]
        public void CloneModelCheckSyncing()
        {
            var c1 = Catchment.CreateDefault();
            c1.CatchmentType = CatchmentType.Unpaved;
            
            var model = new RainfallRunoffModel();
            model.Basin.Catchments.Add(c1);

            // do this manually for test..bleh
            model.InputWaterLevel.Features.Add(c1);
            model.InputWaterLevel.FeatureVariable.Values.Add(c1);

            var clonedModel = (RainfallRunoffModel)model.DeepClone();

            Assert.AreEqual(model.InputWaterLevel.FeatureVariable.Values.Count,
                            clonedModel.InputWaterLevel.FeatureVariable.Values.Count, "input wlvl");
            
            var clonedCatchmentInInputWaterLevel = clonedModel.InputWaterLevel.FeatureVariable.Values[0];
            Assert.AreNotSame(c1, clonedModel.Basin.Catchments.First());
            Assert.AreNotSame(c1, clonedCatchmentInInputWaterLevel, "input wlvl: feature cloned");
            Assert.AreSame(clonedModel.Basin.Catchments.First(), clonedCatchmentInInputWaterLevel);
        }

        [Test]
        public void CloneModelFixedFiles()
        {
            // create model and add catchments to its basin
            var model = new RainfallRunoffModel();
            model.FixedFiles.GreenhouseClassesFile.Content = "";

            var clonedModel = (RainfallRunoffModel)model.DeepClone();

            Assert.AreNotSame(model.FixedFiles, clonedModel.FixedFiles);
            Assert.AreNotSame(model.FixedFiles.GreenhouseClassesFile, clonedModel.FixedFiles.GreenhouseClassesFile);
            Assert.AreEqual("", clonedModel.FixedFiles.GreenhouseClassesFile.Content);
        }

        [Test]
        public void CloneModelWithMeteoPerStation()
        {
            // create model and add catchments to its basin
            var model = new RainfallRunoffModel();
            model.MeteoStations.Add("Station_A");
            model.MeteoStations.Add("Station_B");
            model.Precipitation.DataDistributionType = MeteoDataDistributionType.PerStation;
            model.Precipitation.Data[new DateTime(2000, 1, 1), "Station_B"] = 123.0;

            var clonedModel = (RainfallRunoffModel)model.DeepClone();

            Assert.AreNotSame(model.MeteoStations, clonedModel.MeteoStations);
            Assert.AreEqual(model.MeteoStations, clonedModel.MeteoStations);
            Assert.AreEqual(MeteoDataDistributionType.PerStation, clonedModel.Precipitation.DataDistributionType);
            Assert.AreEqual(123.0, clonedModel.Precipitation.Data[new DateTime(2000, 1, 1), "Station_B"]);
        }

        [Test]
        public void CloneModelOutputSettings()
        {
            // create model and add catchments to its basin
            var model = new RainfallRunoffModel();
            var outputTimeStep = new TimeSpan(8, 0, 0);
            model.OutputSettings.AggregationOption = AggregationOptions.Maximum;
            
            model.OutputSettings.OutputTimeStep = outputTimeStep;

            var clonedModel = (RainfallRunoffModel)model.DeepClone();

            Assert.AreNotSame(model.OutputSettings, clonedModel.OutputSettings);
            Assert.AreEqual(outputTimeStep, clonedModel.OutputSettings.OutputTimeStep);
            Assert.AreEqual(AggregationOptions.Maximum, model.OutputSettings.AggregationOption);
        }

        [Test]
        public void NewRainfallRunoffModelHasCorrectDefaultEvaporationData()
        {
            var model = new RainfallRunoffModel();

            var expectedDates = new List<DateTime>();
            var startDate = new DateTime(1980, 01, 01);
            var endDate = new DateTime(2030, 01, 01);
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                expectedDates.Add(currentDate);
                currentDate = currentDate.AddYears(1);
            }
            Assert.That(expectedDates.Count, Is.EqualTo(51));
            Assert.That(expectedDates.First(), Is.EqualTo(new DateTime(1980,1,1)));
            Assert.That(expectedDates.Last(), Is.EqualTo(new DateTime(2030,1,1)));


            var actualEvaporationTimeData = model.Evaporation.Data.Arguments[0].Values;
            var actualEvaporationValues = model.Evaporation.Data.Components[0].Values;

            // assert that all expected dates are present in the actual evaporation data
            Assert.That(actualEvaporationTimeData.Count, Is.EqualTo(expectedDates.Count));
            foreach (var expectedDate in expectedDates)
            {
                Assert.That(actualEvaporationTimeData.Contains(expectedDate));
            }

            // assert that the component is set to 0 for each date
            Assert.That(actualEvaporationValues.Count, Is.EqualTo(expectedDates.Count));
            Assert.That(actualEvaporationValues, Is.All.EqualTo(0));
        }

        [Test]
        public void Parse_Owner_Workflow_Changed_Event()
        {
            var owner = new MyCompositeActivity();
            var counter = 0;
            
            var model = new RainfallRunoffModel();
            model.WorkflowChanged += (s, b) =>
            {
                counter++;
            };

            model.Owner = owner;
            
            Assert.AreEqual(1,counter);

            owner.CurrentWorkflow = new MyCompositeActivity();
            
            Assert.AreEqual(2,counter);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void UpdateUnpavedDataExtended(bool useLocalBoundaryData)
        {
            var rrModel = new RainfallRunoffModel();
            var catchmentName = "Hihihihi";
            var catchment = new Catchment() { Name = catchmentName };

            var unpavedData = new UnpavedData(catchment);
            unpavedData.BoundarySettings.UseLocalBoundaryData = useLocalBoundaryData;
            rrModel.ModelData.Add(unpavedData);

            Assert.AreEqual(0, rrModel.SaveUnpavedDataExtended.Count());
            
            //action & assert
            rrModel.UpdateUnpavedDataExtended();
            
            Assert.AreEqual(1, rrModel.SaveUnpavedDataExtended.Count());
            var unpavedDataExtended = rrModel.SaveUnpavedDataExtended.First();
            Assert.AreEqual(catchmentName, unpavedDataExtended.CatchmentName);
            Assert.AreEqual(useLocalBoundaryData, unpavedDataExtended.UseLocalBoundaryData);
        }
        
        [Test]
        public void UpdateUnpavedDataWithExtendedData()
        {
            var rrModel = new RainfallRunoffModel();
            var catchmentName = "Hihihihi";
            var catchment = new Catchment() { Name = catchmentName };
            var unpavedData = new UnpavedData(catchment);
            rrModel.ModelData.Add(unpavedData);
            unpavedData.BoundarySettings.UseLocalBoundaryData = false;

            rrModel.SaveUnpavedDataExtended.Add(new UnpavedDataExtended(catchmentName, true));
            
            rrModel.UpdateUnpavedDataWithExtendedData();
            
            Assert.IsTrue(rrModel.ModelData.OfType<UnpavedData>().First().BoundarySettings.UseLocalBoundaryData);
        }

        [Test]
        public void GivenNewRRModelCreationWhenCreatedThenDefaultNwrwSurfaceSettingsLoaded()
        {
            var rrModel = new RainfallRunoffModel();
            Assert.That(rrModel.NwrwDefinitions.Last().SurfaceStorage, Is.EqualTo(6.0).Within(0.1));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WhenExportingRRModelWithWWTPLinkedToBoundary_WritesBoundaryCorrectlyToBoundaryConditionsFile()
        {
            // Setup
            const string boundaryName = "boundary";

            using (RainfallRunoffModel rrModel = CreateRRModelWithWWTPLinkedToBoundary(boundaryName))
            using (var tempDir = new TemporaryDirectory())
            {
                var exporter = new RainfallRunoffModelExporter();

                // Call
                exporter.Export(rrModel, tempDir.Path);
                
                // Assert
                string boundaryFilepath = Path.Combine(tempDir.Path, "BoundaryConditions.bc");
                AssertThatBoundaryFileContainsWWTPBoundary(boundaryFilepath, boundaryName);
            }
        }

        private static RainfallRunoffModel CreateRRModelWithWWTPLinkedToBoundary(string boundaryName)
        {
            var rrModel = new RainfallRunoffModel();

            var wwtp = new WasteWaterTreatmentPlant { Name = "wwtp" };
            rrModel.Basin.WasteWaterTreatmentPlants.Add(wwtp);

            var boundary = new RunoffBoundary { Name = boundaryName };
            rrModel.Basin.Boundaries.Add(boundary);

            wwtp.LinkTo(boundary);

            return rrModel;
        }

        private static void AssertThatBoundaryFileContainsWWTPBoundary(string boundaryFilepath, string boundaryName)
        {
            List<BcIniSection> iniSections = new BcReader(new FileSystem()).ReadBcFile(boundaryFilepath).ToList();
            
            Assert.That(iniSections.Count, Is.EqualTo(2)); // 1 general section and 1 boundary section

            bool containsWWTPBoundary = iniSections
                .Any(bcSection => bcSection.Section.Properties.Any(
                         p => p.Key.EqualsCaseInsensitive("name") && p.Value.EqualsCaseInsensitive(boundaryName)));

            Assert.That(containsWWTPBoundary, Is.True);
        }
    }

    [Entity] public class MyCompositeActivity : ICompositeActivity
    {
        public MyCompositeActivity()
        {
            Activities = new EventedList<IActivity>();
        }
        
        public IEventedList<IActivity> Activities { get; }
        public bool ReadOnly { get; set; }
        public virtual ICompositeActivity CurrentWorkflow { get; set; }

        public Type GetEntityType() { return null; }
        public long Id { get; set; }
        public IEnumerable<object> GetDirectChildren() { return null;}

        public string Name { get; set; }
        public object DeepClone() { return null; }
        public void Initialize(){ }
        public void Execute() {}
        public void Cancel() {}
        public void Finish() {}
        public void Cleanup() {}
        public void ResetActivity() {}
        public IEventedList<IActivity> DependsOn { get; set; }
        public ActivityStatus Status { get; }
        public string ProgressText { get; }
        public event EventHandler ProgressChanged;
        public event EventHandler<ActivityStatusChangedEventArgs> StatusChanged;
    }
}