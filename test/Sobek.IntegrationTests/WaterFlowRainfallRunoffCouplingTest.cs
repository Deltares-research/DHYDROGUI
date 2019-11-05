using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using AggregationOptions = DeltaShell.NGHS.IO.DataObjects.Model1D.AggregationOptions;
using Point = NetTopologySuite.Geometries.Point;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class WaterFlowRainfallRunoffCouplingTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]
        public void RunSimultaneousExpectRROutputToEqualFlowInflowNoWaterlevelFeedback()
        {
            var integratedModel = CreateSimpleCoupledModelWithOneCatchment(CatchmentType.OpenWater);
            var rr = integratedModel.Activities.OfType<RainfallRunoffModel>().First();
            var flow = integratedModel.Activities.OfType<WaterFlowModel1D>().First();
            // run parallel/simultaneous:
            ActivityRunner.RunActivity(integratedModel);

            AssertOutAndInflowAreEqual(rr, flow);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category("DIMR_Introduction")]
        [Category(TestCategory.WorkInProgress)]

        public void RunSequentialExpectRROutputToEqualFlowInflowNoWaterlevelFeedback()
        {
            var integratedModel = CreateSimpleCoupledModelWithOneCatchment(CatchmentType.OpenWater);
            var rr = integratedModel.Activities.OfType<RainfallRunoffModel>().First();
            var flow = integratedModel.Activities.OfType<WaterFlowModel1D>().First();
            
            // run sequential:
            ActivityRunner.RunActivity(rr);
            ActivityRunner.RunActivity(flow);
            
            
            Assert.AreEqual(ActivityStatus.Cleaned, rr.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, flow.Status);
            
            AssertOutAndInflowAreEqual(rr, flow);
        }
        private static IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new WaterFlowModel1DExporter();
            yield return new RainfallRunoffModelExporter();
            
        }
        [Test]
        [Category(TestCategory.Integration)]
        [Category("DIMR_Introduction")]
        [Category(TestCategory.WorkInProgress)]
        public void RunSequentialTwoCatchmentsExpectRROutputToEqualFlowInflowNoWaterlevelFeedback()
        {
            var integratedModel = CreateSimpleCoupledModelWithOneCatchment(CatchmentType.OpenWater);
            var rr = integratedModel.Activities.OfType<RainfallRunoffModel>().First();
            var flow = integratedModel.Activities.OfType<WaterFlowModel1D>().First();

            // add another catchment & link it to the same lateral
            var l1 = flow.Network.LateralSources.First();
            var c2 = new Catchment
            {
                Name = "C2",
                CatchmentType = CatchmentType.OpenWater,
                Geometry = new Point(1000, 500),
                IsGeometryDerivedFromAreaSize = true
            };
            c2.SetAreaSize(1000.0);
            rr.Basin.Catchments.Add(c2);
            c2.LinkTo(l1);

            // run sequential:
            ActivityRunner.RunActivity(rr);
            ActivityRunner.RunActivity(flow);
            
            Assert.AreEqual(ActivityStatus.Cleaned, rr.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, flow.Status);

            var rrOutflow = rr.BoundaryDischarge.GetValues().OfType<double>().ToArray();
            var summedRROutflow = new double[rrOutflow.Length / 2];
            for (int i = 0; i < rrOutflow.Length; i++)
                summedRROutflow[(int) (i/2)] += rrOutflow[i];
            
            AssertOutAndInflowAreEqual(rr, flow, summedRROutflow);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]
        public void RunWithFeedbackSequentialAndSimultaneousAndExpectDifferentResults()
        {
            var integratedModel = CreateSimpleCoupledModelWithOneCatchment(CatchmentType.Unpaved, bedlevel: 14.0);
            var diff = RunSequentialAndSimultaneousAndReturnDiffInLateralInflow(integratedModel).ToArray();

            // due to water level feedback (information from Flow to RR), we expect a difference in
            // results between sequential and simultaneous runs.
            var expectedDiff = new[] {0.0, 0.0, 2.6416, -2.5979, -0.039026, 0.00066328};

            Assert.AreEqual(expectedDiff.Length, diff.Length);

            for (int i = 0; i < expectedDiff.Length; i++)
                Assert.AreEqual(expectedDiff[i], diff[i], 0.01, "index: " + i);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WorkInProgress)]
        public void RunWithoutFeedbackSequentialAndSimultaneousAndExpectSameResults()
        {
            var integratedModel = CreateSimpleCoupledModelWithOneCatchment(CatchmentType.OpenWater);

            var diff = RunSequentialAndSimultaneousAndReturnDiffInLateralInflow(integratedModel);

            Assert.IsTrue(diff.All(d => Math.Abs(d) < 0.00001));
        }

        private static void AssertOutAndInflowAreEqual(RainfallRunoffModel rr, WaterFlowModel1D flow, double[] rrOutflow=null)
        {
            // gather results
            rrOutflow = rrOutflow ?? rr.BoundaryDischarge.GetValues().OfType<double>().ToArray();
            var flowInflow = flow.OutputFunctions.First(c => c.Name == "Discharge (l)").GetValues().OfType<double>().ToArray();

            // check we have the right data
            Assert.Greater(rrOutflow.Length, 0);
            Assert.AreEqual(rrOutflow.Length, flowInflow.Length);

            // compare
            Console.WriteLine("RR-out ==== Flow-in");
            for (int i = 0; i < rrOutflow.Length; i++)
                Console.WriteLine("{0,-6:G5} ==== {1,-6:G5}", rrOutflow[i], flowInflow[i]);

            for (int i = 0; i < rrOutflow.Length; i++)
                Assert.AreEqual(rrOutflow[i], flowInflow[i], 0.0001, "diff at index " + i);
        }

        private static IEnumerable<double> RunSequentialAndSimultaneousAndReturnDiffInLateralInflow(HydroModel integratedModel)
        {
            var rr = integratedModel.Activities.OfType<RainfallRunoffModel>().First();
            var flow = integratedModel.Activities.OfType<WaterFlowModel1D>().First();

            var l1 = flow.Network.LateralSources.First();
            var inflow = flow.OutputFunctions.OfType<ICoverage>().First(c => c.Name == "Discharge (l)");
            
            // run sequential:
            ActivityRunner.RunActivity(rr);
            ActivityRunner.RunActivity(flow);

            Assert.AreEqual(ActivityStatus.Cleaned, rr.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, flow.Status);

            var seqResults = inflow.GetTimeSeries(l1).GetValues().OfType<double>().ToArray();
            var rrOutSeq = rr.BoundaryDischarge.GetValues<double>().ToList();

            // run parallel/simultaneous:
            ActivityRunner.RunActivity(integratedModel);

            Assert.AreEqual(ActivityStatus.Cleaned, rr.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, flow.Status);

            var simulResults = inflow.GetTimeSeries(l1).GetValues().OfType<double>().ToArray();
            var rrOutSim = rr.BoundaryDischarge.GetValues<double>().ToList();

            Console.WriteLine("Seq (RR) - Simul (RR)");
            for (int i = 0; i < rrOutSeq.Count; i++)
                Console.WriteLine("{0,-6:G5}", rrOutSeq[i] - rrOutSim[i]);
            Console.WriteLine("");

            if (seqResults.Length != simulResults.Length)
                    Assert.Fail("Expected results to have same length");
            
            var diff = new List<double>();
            Console.WriteLine("Seq    ==== Simul");
            for (var i = 0; i < seqResults.Length; i++)
            {
                Console.WriteLine("{0,-6:G5} ==== {1,-6:G5}", seqResults[i], simulResults[i]);
                diff.Add(seqResults[i] - simulResults[i]);
            }

            return diff;
        }

        private static HydroModel CreateSimpleCoupledModelWithOneCatchment(CatchmentType catchmentType, double bedlevel=1.3)
        {
            // create full hydro model
            var builder = new HydroModelBuilder();
            var hydroModel = builder.BuildModel(ModelGroup.All);

            // remove non Flow/RR activities
            foreach (var activity in hydroModel.Activities.ToList())
            {
                if (!(activity is WaterFlowModel1D) && !(activity is RainfallRunoffModel))
                {
                    hydroModel.Activities.Remove(activity);
                }
            }

            var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();
            var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

            // add catchment to rr
            var c1 = new Catchment
                                {
                                    Name = "C1",
                                    CatchmentType = catchmentType,
                                    Geometry = new Point(100, 500),
                                    IsGeometryDerivedFromAreaSize = true
                                };
            c1.SetAreaSize(1000.0);
            rr.Basin.Catchments.Add(c1);

            // add channel to flow
            var n1 = new HydroNode("N1") { Geometry = new Point(0, 0) };
            var n2 = new HydroNode("N2") { Geometry = new Point(200, 0) };
            flow.Network.Nodes.Add(n1);
            flow.Network.Nodes.Add(n2);
            var channel = new Channel(n1, n2)
                              {
                                  Name = "B1",
                                  Geometry = new LineString(new[] { n1.Geometry.Coordinate, n2.Geometry.Coordinate })
                              };
            flow.Network.Branches.Add(channel);

            // add simple cross section to channel
            CrossSectionHelper.AddCrossSection(channel, 50, bedlevel);

            // add lateral to flow
            var l1 = new LateralSource { Name = "L1", Geometry = new Point(100, 0) };
            channel.BranchFeatures.Add(l1);

            // link catchment c1 to lateral l1
            c1.LinkTo(l1);
            
            // define flow computational grid (every 10 m)
            var numGridPoints = ((int) channel.Length/10) + 1;
            HydroNetworkHelper.GenerateDiscretization(flow.NetworkDiscretization, channel,
                                                      Enumerable.Range(0, numGridPoints)
                                                                .Select(o => o*10.0));

            // enable flow lateral discharge output
            flow.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Laterals, DataItemRole.Output).AggregationOptions = AggregationOptions.Current;

            // set model times to 5 x 1hour
            hydroModel.OverrideStartTime = true;
            hydroModel.OverrideStopTime = true;
            hydroModel.OverrideTimeStep = true;
            hydroModel.StartTime = new DateTime(2000, 1, 1, 0, 0, 0);
            hydroModel.StopTime = new DateTime(2000, 1, 1, 5, 0, 0); // 5h
            hydroModel.TimeStep = new TimeSpan(1, 0, 0); // 1h (5 timesteps)

            // initialize precipitation & evaporation
            var generator = new TimeSeriesGenerator();
            generator.GenerateTimeSeries(rr.Precipitation.Data, rr.StartTime, rr.StopTime, rr.TimeStep);
            generator.GenerateTimeSeries(rr.Evaporation.Data, rr.StartTime, rr.StopTime, rr.TimeStep);

            // set 5400mm/h precipitation uniform & global (=1.5 m3/s)
            var numValues = rr.Precipitation.Data.Components[0].Values.Count;
            rr.Precipitation.Data.Components[0].SetValues(Enumerable.Range(0, numValues).Select(i => (i + 1)*5400.0));

            return hydroModel;
        }
    }
}