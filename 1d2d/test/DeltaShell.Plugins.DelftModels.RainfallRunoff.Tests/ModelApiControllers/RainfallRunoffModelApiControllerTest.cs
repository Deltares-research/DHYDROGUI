using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.ModelApiControllers
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class RainfallRunoffModelApiControllerTest
    {
        [OneTimeSetUp]
        public void AssemblySetUp()
        {
            TestHelper.SetDeltaresLicenseToEnvironmentVariable();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Directory.GetDirectories(TestHelper.GetTestWorkingDirectory(), nameof(RainfallRunoffModelApiControllerTest) + "*").ForEach(FileUtils.DeleteIfExists);
        }
        
        private RainfallRunoffModel CreateModel(bool setMeteoData = false)
        {
            var model = new RainfallRunoffModel
            {
                WorkingDirectoryPathFunc = () => TestHelper.GetTestWorkingDirectory(TestHelper.GetCurrentMethodName())
            };
            model.OutputTimeStep = model.TimeStep; // reset
            model.Basin.Catchments.Add(new Catchment());

            if (setMeteoData)
            {
                SetGlobalMeteoDataForTesting(model);
            }

            return model;
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void RunModelTwiceAndGetBoundariesOutputCoverage()
        {
            using (var model = CreateModel())
            {
                model.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.BoundaryElmSet).IsEnabled = true;
                model.OutputSettings.GetEngineParameter(QuantityType.CumInNonLinks_m3, ElementSet.BalanceNodeElmSet).IsEnabled = true;
                model.OutputSettings.AggregationOption = AggregationOptions.Current;

                var catchment = new Catchment
                    {
                        Name = "c1",
                        IsGeometryDerivedFromAreaSize = true,
                        CatchmentType = CatchmentType.Unpaved
                    };
                catchment.SetAreaSize(3000000);
                model.Basin.Catchments.Add(catchment);
                
                var fakeBoundary = new RunoffBoundary();
                model.Basin.Boundaries.Add(fakeBoundary);

                catchment.LinkTo(fakeBoundary);
                
                SetGlobalMeteoDataForTesting(model);

                ActivityRunner.RunActivity(model);
                System.Threading.Thread.Sleep(15); // Give kernel a chance to die and release file handles

                ActivityRunner.RunActivity(model);
                System.Threading.Thread.Sleep(15); // Give kernel a chance to die and release file handles

                var boundaryTag = RainfallRunoffModelParameterNames.BoundaryDischarge;
                var coverage = (IFeatureCoverage)model.OutputCoverages.First(c => c.Name.StartsWith(boundaryTag));
                Assert.AreEqual(25, coverage.Components[0].Values.Count);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void RunModelWithoutBoundaryAndExpectFakeBoundaryAndLink()
        {
            using (var model = CreateModel())
            {
                model.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.BoundaryElmSet).IsEnabled = true;
                model.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.LinkElmSet).IsEnabled = true;
                model.OutputSettings.AggregationOption = AggregationOptions.Current;

                var catchment = new Catchment
                {
                    Name = "c1",
                    IsGeometryDerivedFromAreaSize = true,
                    CatchmentType = CatchmentType.Unpaved
                };
                catchment.SetAreaSize(3000000);
                model.Basin.Catchments.Add(catchment);

                var fakeBoundary = new RunoffBoundary();
                model.Basin.Boundaries.Add(fakeBoundary);
                catchment.LinkTo(fakeBoundary);

                SetGlobalMeteoDataForTesting(model);

                ActivityRunner.RunActivity(model);
                System.Threading.Thread.Sleep(15); // Give kernel a chance to die and release file handles

                if (model.Status == ActivityStatus.Failed)
                {
                    Assert.Fail("Model crashed");
                }

                var linkFlowTag = RainfallRunoffModelParameterNames.LinkFlowOut;
                var coverage = (IFeatureCoverage)model.OutputCoverages.First(c => c.Name.StartsWith(linkFlowTag));
                Assert.AreEqual(25, coverage.Components[0].Values.Count);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void RunModelWithMeteoPerStation()
        {
            using (var model = CreateModel())
            {
                // add first catchment
                var c1 = new Catchment
                {
                    Name = "c1",
                    IsGeometryDerivedFromAreaSize = true,
                    CatchmentType = CatchmentType.Unpaved
                };
                c1.SetAreaSize(3000000);
                model.Basin.Catchments.Add(c1);

                var fakeBoundaryOne = new RunoffBoundary() { Name = "B1"};
                model.Basin.Boundaries.Add(fakeBoundaryOne);
                c1.LinkTo(fakeBoundaryOne);

                // add 2nd catchment
                var c2 = new Catchment
                {
                    Name = "c2",
                    IsGeometryDerivedFromAreaSize = true,
                    CatchmentType = CatchmentType.Unpaved
                };
                c2.SetAreaSize(3000000);
                model.Basin.Catchments.Add(c2);

                var fakeBoundaryTwo = new RunoffBoundary() { Name = "B2" };
                model.Basin.Boundaries.Add(fakeBoundaryTwo);
                c2.LinkTo(fakeBoundaryTwo);

                // both catchments use the default station (=first one)

                // set adjustment factors (to see if they work)
                var c1Data = model.GetCatchmentModelData(c1);
                c1Data.AreaAdjustmentFactor = 0.5;
                var c2Data = model.GetCatchmentModelData(c2);
                c2Data.AreaAdjustmentFactor = 2.0;

                // set some precipitation per station
                model.Precipitation.DataDistributionType = MeteoDataDistributionType.PerStation;
                model.MeteoStations.Add("Station_A");
                var generator = new TimeSeriesGenerator();
                generator.GenerateTimeSeries(model.Precipitation.Data, model.StartTime, model.StopTime,
                                             model.TimeStep);
                generator.GenerateTimeSeries(model.Evaporation.Data, model.StartTime, model.StopTime,
                                             new TimeSpan(1, 0, 0, 0));
                model.Precipitation.Data[model.StartTime, "Station_A"] = 500.0; //set some precipitation

                // run model
                ActivityRunner.RunActivity(model);
                System.Threading.Thread.Sleep(15); // Give kernel a chance to die and release file handles

                if (model.Status == ActivityStatus.Failed)
                {
                    Assert.Fail("Model crashed");
                }

                var sampleTime = model.StartTime.Add(model.OutputTimeStep);
                var boundaries = (IFeatureCoverage) model.OutputCoverages.First();
                Assert.AreEqual(50, boundaries.Components[0].Values.Count);

                Assert.AreEqual(boundaries[sampleTime, c1], boundaries[sampleTime, c2]);
            }
        }

        [Test]
        public void RunModelForSeveralCatchmentsAndGetOutputCoverage()
        {
            using (var model = CreateModel())
            {
                model.OutputSettings.GetEngineParameter(QuantityType.Rainfall, ElementSet.UnpavedElmSet).
                    IsEnabled = true;

                model.OutputSettings.AggregationOption = AggregationOptions.Current;

                ConfigureSimpleModel(model);
                var catchment = model.GetAllModelData().ElementAt(0).Catchment;
                var catchment2 = model.GetAllModelData().ElementAt(1).Catchment;

                ActivityRunner.RunActivity(model);
                System.Threading.Thread.Sleep(15); // Give kernel a chance to die and release file handles

                const string rainfallUnp = RainfallRunoffModelParameterNames.UnpavedRainfall;
                var coverage = model.OutputCoverages.First(c => c.Name.StartsWith(rainfallUnp)) as IFeatureCoverage;
                Assert.AreEqual(25*2, coverage.Components[0].Values.Count);

                foreach(var time in coverage.Time.Values.Skip(1))
                {
                    Console.WriteLine(coverage[time, catchment] + " - " + coverage[time, catchment2]);
                    Assert.AreNotEqual(coverage[time, catchment], coverage[time, catchment2]);
                }
            }
        }

        [Test]
        public void RunModelWithWWTPAndGetOutputCoverage()
        {
            using (var model = CreateModel())
            {
                var runoffBoundary = new RunoffBoundary() { Name = "WWTP_boundary", Basin = model.Basin};
                model.BoundaryData.Add(new RunoffBoundaryData(runoffBoundary) {Series = new RainfallRunoffBoundaryData() {IsConstant = true, IsTimeSeries = false, Value = 123.4} });
                
                model.OutputSettings.GetEngineParameter(QuantityType.FlowIn, ElementSet.WWTPElmSet).IsEnabled = true;
                model.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.WWTPElmSet).IsEnabled = true;
                model.OutputSettings.AggregationOption = AggregationOptions.Current;

                model.StartTime = new DateTime(2000, 1, 1);
                model.StopTime = new DateTime(2000, 1, 2, 0, 0, 0);

                // create two catchments
                var catchment = new Catchment { Name = "c1", IsGeometryDerivedFromAreaSize = true, CatchmentType = CatchmentType.Paved };
                catchment.SetAreaSize(3000000);
                model.Basin.Catchments.Add(catchment);
                var catchment2 = new Catchment { Name = "c2", IsGeometryDerivedFromAreaSize = true, CatchmentType = CatchmentType.Paved };
                catchment2.SetAreaSize(9000000);
                model.Basin.Catchments.Add(catchment2);

                var wwtp = new WasteWaterTreatmentPlant();
                model.Basin.WasteWaterTreatmentPlants.Add(wwtp);
                runoffBoundary.LinkTo(wwtp);

                var wwtpLinkTarget = Substitute.For<IHydroObject>();
                wwtpLinkTarget.Region.Returns(wwtp.Region);
                wwtp.LinkTo(wwtpLinkTarget);

                catchment.LinkTo(wwtp);
                catchment2.LinkTo(wwtp);

                SetGlobalMeteoDataForTesting(model);

                ActivityRunner.RunActivity(model);
                System.Threading.Thread.Sleep(15); // Give kernel a chance to die and release file handles

                var coverage = (IFeatureCoverage)model.OutputCoverages.First(
                        c => c.Name.StartsWith(RainfallRunoffModelParameterNames.WWTPInFlow));
                Assert.AreEqual(25, coverage.Components[0].Values.Count);
            }
        }

        [Test]
        public void RunModelForSeveralCatchmentsAndGetWaterBalance()
        {
            using (var model = CreateModel())
            {
                model.OutputSettings.GetEngineParameter(QuantityType.BalanceError_m3, ElementSet.BalanceModelElmSet).IsEnabled = true;
                model.OutputSettings.GetEngineParameter(QuantityType.BalanceError_m3, ElementSet.BalanceNodeElmSet).IsEnabled = true;

                model.OutputSettings.AggregationOption = AggregationOptions.Current;

                ConfigureSimpleModel(model);

                ActivityRunner.RunActivity(model);
                System.Threading.Thread.Sleep(15); // Give kernel a chance to die and release file handles

                var balansFunction =
                    model.OutputDataItems.Where(di => di.Value is ITimeSeries).Select(di => (ITimeSeries)di.Value).First(
                        c => c.Name.StartsWith(RainfallRunoffModelParameterNames.ModelBalanceError));
                var balansPerNode = (IFeatureCoverage) model.OutputCoverages.First(
                    c => c.Name.StartsWith(RainfallRunoffModelParameterNames.NodeBalanceBalanceError));
                Assert.AreEqual(25 * 3, balansPerNode.Components[0].Values.Count);
                Assert.AreEqual(25, balansFunction.Components[0].Values.Count);
            }
        }

        [Test]
        public void RunModelForSeveralCatchmentsAndGetOutputCoverageOnSingleBoundary()
        {
            using (var model = CreateModel())
            {
                ConfigureSimpleModel(model);
                var boundary = model.Basin.Boundaries.First();
                
                model.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.BoundaryElmSet).IsEnabled = true;

                model.OutputSettings.AggregationOption = AggregationOptions.Current;

                ActivityRunner.RunActivity(model);
                System.Threading.Thread.Sleep(15); // Give kernel a chance to die and release file handles

                var boundaryDischarge = RainfallRunoffModelParameterNames.BoundaryDischarge;
                var coverage = model.OutputCoverages.First(c => c.Name.StartsWith(boundaryDischarge)) as IFeatureCoverage;
                Assert.AreEqual(25, coverage.Components[0].Values.Count);

                foreach (var time in coverage.Time.Values.Skip(1))
                {
                    Console.WriteLine(coverage[time, boundary]);
                }
            }
        }

        [Test]
        public void InitializeModelWritesUserCustomizedCropFile()
        {
            const string contents = "Test contents";
            var model = CreateModel(true);
            model.FixedFiles.UnpavedCropFactorsFile.Content = contents;

            try
            {
                model.Initialize();
            }
            catch (Exception){} //model run fails, this is expected
            
            Assert.AreEqual(contents, File.ReadAllText(Path.Combine(model.ModelController.WorkingDirectory , "CROPFACT")));
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void InitializeModelWritesCropFile()
        {
            var model = CreateModel(true);
            try
            {
                model.Initialize();
            }
            catch (Exception) {} //expected

            var contents = File.ReadAllText(Path.Combine(model.ModelController.WorkingDirectory , "CROPFACT"));
            Assert.Greater(contents.Length, 50000); //just check something is in it

            model.Dispose();
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ModelWritesGreenhouseYearToFile()
        {
            var model = new RainfallRunoffModel();

            string testWorkingDirectory = Path.Combine(TestHelper.GetTestWorkingDirectory(), TestHelper.GetCurrentMethodName());
            model.ModelController.GetWorkingDirectoryDelegate = () => testWorkingDirectory;
            model.ModelController.WriteFiles();
            model.Dispose();

            var contents = File.ReadAllText(Path.Combine(testWorkingDirectory, "DELFT_3B.INI"));
            
            Assert.True(contents.Contains("GreenhouseYear=1994"));
        }

        [Test]
        public void InitializeModelWritesCapsimToIniFile()
        {
            var model = new RainfallRunoffModel
            {
                WorkingDirectoryPathFunc = () => TestHelper.GetTestWorkingDirectory(TestHelper.GetCurrentMethodName())
            };
            model.CapSim = true;
            model.CapSimCropAreaOption = RainfallRunoffEnums.CapsimCropAreaOptions.PerCropArea;
            model.CapSimInitOption = RainfallRunoffEnums.CapsimInitOptions.AtMoistureContentpF2;
            model.Basin.Catchments.Add(new Catchment());
            SetGlobalMeteoDataForTesting(model);

            try
            {
                model.Initialize();
            }
            catch (Exception) { } //model run fails, this is expected
            
            var contents = File.ReadAllText(Path.Combine(model.ModelController.WorkingDirectory , "DELFT_3B.INI"));
            Assert.Greater(contents.Length, 100);

            var stringsToMatch = new[] { @"UnsaturatedZone=1",
                                         @"InitCapsimOption=2",
                                         @"CapsimPerCropArea=-1"
                                       };

            foreach (var s in stringsToMatch)
            {
                var match = Regex.Match(contents, s);
                Assert.IsTrue(match.Success, s + " not found in file");
            }
        }

        [Test]
        public void InitializeModelWritesModelSpecificSettingsToIniFile()
        {
            var model = new RainfallRunoffModel
            {
                WorkingDirectoryPathFunc = () => TestHelper.GetTestWorkingDirectory(TestHelper.GetCurrentMethodName()),
                MinimumFillingStoragePercentage = 22,
                EvaporationStartActivePeriod = 9,
                EvaporationEndActivePeriod = 21
            };

            model.Basin.Catchments.Add(new Catchment());
            SetGlobalMeteoDataForTesting(model);

            try
            {
                model.Initialize();
            }
            catch (Exception) { } //model run fails, this is expected

            var contents = File.ReadAllText(Path.Combine(model.ModelController.WorkingDirectory , "DELFT_3B.INI"));
            Assert.Greater(contents.Length, 100);

            var stringsToMatch = new[] { @"MinFillingPercentage=22",
                                         @"EvaporationFromHrs=9",
                                         @"EvaporationToHrs=21"
                                       };

            foreach (var s in stringsToMatch)
            {
                var match = Regex.Match(contents, s);
                Assert.IsTrue(match.Success, s + " not found in file");
            }
        }

        [Test]
        public void RunModelForSeveralCatchmentsAndGetOutputOnLinks()
        {
            using (var model = CreateModel())
            {
                ConfigureSimpleModel(model);
                
                model.OutputSettings.GetEngineParameter(QuantityType.Flow, ElementSet.LinkElmSet).IsEnabled = true;
                model.OutputSettings.AggregationOption = AggregationOptions.Current;

                ActivityRunner.RunActivity(model);
                System.Threading.Thread.Sleep(15); // Give kernel a chance to die and release file handles

                var flowOnLinks = RainfallRunoffModelParameterNames.LinkFlowOut;
                var coverage = (IFeatureCoverage)model.OutputCoverages.First(c => c.Name.StartsWith(flowOnLinks));
                Assert.AreEqual(25 * 2, coverage.Components[0].Values.Count);

                var link1 = coverage.Features[0];
                var link2 = coverage.Features[1];

                foreach (var time in coverage.Time.Values.Skip(1))
                {
                    Console.WriteLine(coverage[time, link1] + " - " + coverage[time, link2]);
                    Assert.AreNotEqual(coverage[time, link1], coverage[time, link2]);
                }
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void RunModelForSeveralCatchmentsAndGetBalanceOutputOnNodes()
        {
            using (var model = CreateModel())
            {
                model.OutputSettings.GetEngineParameter(QuantityType.CumInNonLinks_m3, ElementSet.BalanceNodeElmSet).IsEnabled = true;
                model.OutputSettings.AggregationOption = AggregationOptions.Current;

                ConfigureSimpleModel(model);

                ActivityRunner.RunActivity(model);
                System.Threading.Thread.Sleep(15); // Give kernel a chance to die and release file handles

                var coverage =
                    (IFeatureCoverage)
                    model.OutputCoverages.First(
                        c => c.Name.StartsWith(RainfallRunoffModelParameterNames.NodeBalanceCumFlowInNonLinks));

                Assert.AreEqual(25 * 3, coverage.Components[0].Values.Count); //2 catchments, 1 boundary

                var feature1 = coverage.Features[0];
                var feature2 = coverage.Features[1];
                var feature3 = coverage.Features[2];

                foreach (var time in coverage.Time.Values.Skip(1))
                {
                    Console.WriteLine(coverage[time, feature1] + " - " +
                                      coverage[time, feature2] + " - " +
                                      coverage[time, feature3]);
                    Assert.AreNotEqual(coverage[time, feature1], coverage[time, feature2]);
                }
            }
        }

        private static void ConfigureSimpleModel(RainfallRunoffModel model)
        {
            model.StartTime = new DateTime(2000, 1, 1);
            model.StopTime = new DateTime(2000, 1, 2, 0, 0, 0);

            // create two catchments
            var catchment = new Catchment { Name = "c1", IsGeometryDerivedFromAreaSize = true, CatchmentType = CatchmentType.Unpaved };
            catchment.SetAreaSize(3000000);
            model.Basin.Catchments.Add(catchment);

            var catchment2 = new Catchment { Name = "c2", IsGeometryDerivedFromAreaSize = true, CatchmentType = CatchmentType.Unpaved };
            catchment2.SetAreaSize(9000000);
            model.Basin.Catchments.Add(catchment2);

            var boundary = new RunoffBoundary();
            model.Basin.Boundaries.Add(boundary);
            catchment.LinkTo(boundary);
            catchment2.LinkTo(boundary);

            SetGlobalMeteoDataForTesting(model);
        }

        [Test]
        public void RunModelWithEmptyEvapAndBigPrecipitation()
        {
            using (var model = CreateModel())
            {
                ConfigureSimpleModel(model);

                model.StartTime = new DateTime(2005, 12, 30, 0, 0, 0);
                model.StopTime = new DateTime(2006, 1, 4, 0, 0, 0);

                var precipitationEnd = model.StopTime.AddDays(2);

                var precipitationTime = model.StartTime;

                while(precipitationTime <= precipitationEnd)
                {
                    model.Precipitation.Data[precipitationTime] = 1.0;
                    model.Evaporation.Data[precipitationTime] = 1.0;
                    precipitationTime = precipitationTime.AddHours(1);
                }

                ActivityRunner.RunActivity(model);
                System.Threading.Thread.Sleep(15); // Give kernel a chance to die and release file handles
            }
        }
        
        private static void SetGlobalMeteoDataForTesting(RainfallRunoffModel rrModel)
        {
            OverwriteDefaultData(rrModel.Evaporation.Data);
            rrModel.Precipitation.DataDistributionType = MeteoDataDistributionType.Global;
            rrModel.Evaporation.DataDistributionType = MeteoDataDistributionType.Global;

            var days = Math.Ceiling(rrModel.StopTime.Subtract(rrModel.StartTime).TotalDays);

            for (int i = 0; i <= days; i++ )
            {
                rrModel.Evaporation.Data[rrModel.StartTime.AddDays(i)] = 0.0;
            }

            var j = 1.0;
            for (var current = rrModel.StartTime; current <= rrModel.StopTime; current += rrModel.TimeStep)
            {
                rrModel.Precipitation.Data[current] = j++;
            }
        }
        
        private static void OverwriteDefaultData(IFunction data)
        {
            var timeArgument = data.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
            if (timeArgument != null)
            {
                
                var startDate = new DateTime(1980, 01, 01);
                var endDate = new DateTime(2030, 01, 01);
                var dates = new List<DateTime>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    dates.Add(currentDate);
                    currentDate = currentDate.AddDays(1);
                }

                timeArgument.Clear();
                timeArgument.SetValues(dates);
            }
        }
    }
}
