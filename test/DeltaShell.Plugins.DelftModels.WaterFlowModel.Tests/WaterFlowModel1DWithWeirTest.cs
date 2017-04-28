using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using GeometryFactory = SharpMap.Converters.Geometries.GeometryFactory;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DWithWeirTest
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaterFlowModel1DWithWeirTest));

        private double delta = 0.01;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestSimpleWeirFormula()
        {
            // init weir
            var weir = new Weir
                {
                    OffsetY = 150,
                    CrestWidth = 10.0,
                    CrestLevel = 6.0,
                    FlowDirection = FlowDirection.Both,
                    WeirFormula = new SimpleWeirFormula
                        {
                            LateralContraction = 0.9,
                            DischargeCoefficient = 1.0
                        }
                };

            var flowModel1D = GetFlowModel1D(weir);

            var bcLeft = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes[0]);
            var bcRight = flowModel1D.BoundaryConditions.First(bc => bc.Feature == flowModel1D.Network.Nodes[1]);

            bcLeft.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
            bcRight.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;

            bcLeft.Data[new DateTime(2000, 1, 1)] = 10.0;
            bcRight.Data[new DateTime(2000, 1, 1)] = 20.0;

            bcLeft.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            bcRight.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

            RunModelWithWeir(weir, flowModel1D);

            // check weir values for last time step
            var location = new NetworkLocation(weir.Branch, weir.Chainage);
            var depthValue = flowModel1D.OutputDepth.AddTimeFilter(flowModel1D.CurrentTime).Evaluate(location);
            var velocityValue = flowModel1D.OutputVelocity.AddTimeFilter(flowModel1D.CurrentTime).Evaluate(location);

            Assert.AreEqual(15.047714469049206, depthValue, delta);
            Assert.AreEqual(5.1147204455773938, velocityValue, delta);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ExecuteWithSimpleWeir()
        {
            // init weir
            var weir = new Weir
                           {
                               OffsetY = 150,
                               CrestWidth = 5,
                               CrestLevel = 1,
                               FlowDirection = FlowDirection.Both,
                               WeirFormula =
                                   new SimpleWeirFormula {LateralContraction = 0.9, DischargeCoefficient = 1.0}
                           };

            var flowModel1D = GetFlowModel1D(weir);

            RunModelWithWeir(weir, flowModel1D);

            // check weir values for last time step
            var location = new NetworkLocation(weir.Branch, weir.Chainage);
            var depthValue = flowModel1D.OutputDepth.AddTimeFilter(flowModel1D.CurrentTime).Evaluate(location);
            var velocityValue = flowModel1D.OutputVelocity.AddTimeFilter(flowModel1D.CurrentTime).Evaluate(location);

            Assert.AreEqual(5.1809339063220579, depthValue, delta);
            Assert.AreEqual(0.0, velocityValue, delta);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ExecuteWithRiverWeir()
        {
            var submergeReduction = FunctionHelper.Get1DFunction<double, double>("submergereductionpos", "S", "R");
            submergeReduction[0.82] = 1.0;
            submergeReduction[0.92] = 0.7;
            submergeReduction[0.94] = 0.3;
            submergeReduction[1.0] = 0.0;

            // init weir
            var weir = new Weir
                           {
                               OffsetY = 150,
                               CrestWidth = 10,
                               CrestLevel = 1.5,
                               WeirFormula = new RiverWeirFormula
                                                 {
                                                     CorrectionCoefficientPos = 1,
                                                     SubmergeLimitPos = 0.82,
                                                     CorrectionCoefficientNeg = 1,
                                                     SubmergeLimitNeg = 0.82,
                                                     SubmergeReductionPos = submergeReduction,
                                                     SubmergeReductionNeg = submergeReduction
                                                 }
                           };

            var flowModel1D = GetFlowModel1D(weir);

            RunModelWithWeir(weir, flowModel1D);

            // check weir values for last time step
            var location = new NetworkLocation(weir.Branch, weir.Chainage);
            var depthValue = flowModel1D.OutputDepth.AddTimeFilter(flowModel1D.CurrentTime).Evaluate(location);
            var velocityValue = flowModel1D.OutputVelocity.AddTimeFilter(flowModel1D.CurrentTime).Evaluate(location);

            Assert.AreEqual(5.2172189065512287, depthValue, delta);
            Assert.AreEqual(0.0, velocityValue, delta);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ExecuteWithPierWeir()
        {
            // init weir
            var weir = new Weir
                           {
                               OffsetY = 150,
                               CrestWidth = 10,
                               CrestLevel = 1.5,
                               WeirFormula = new PierWeirFormula()
                                                 {
                                                     AbutmentContractionNeg = 0.15,
                                                     AbutmentContractionPos = 0.1,
                                                     DesignHeadNeg = 2.5,
                                                     DesignHeadPos = 3,
                                                     NumberOfPiers = 3,
                                                     PierContractionPos = 0.01,
                                                     PierContractionNeg = 0.015,
                                                     UpstreamFacePos = 10,
                                                     UpstreamFaceNeg = 8
                                                 }
                           };


            var flowModel1D = GetFlowModel1D(weir);

            RunModelWithWeir(weir, flowModel1D);

            // check weir values for last time step
            var location = new NetworkLocation(weir.Branch, weir.Chainage);
            var depthValue = flowModel1D.OutputDepth.AddTimeFilter(flowModel1D.CurrentTime).Evaluate(location);
            var velocityValue = flowModel1D.OutputVelocity.AddTimeFilter(flowModel1D.CurrentTime).Evaluate(location);

            Assert.AreEqual(5.2350062931159584, depthValue, delta);
            Assert.AreEqual(0.0, velocityValue, delta);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ExecuteWithGatedWeir()
        {
            // init weir
            var weir = new Weir
                           {
                               OffsetY = 150,
                               CrestWidth = 2,
                               CrestLevel = 0.7,
                               FlowDirection = FlowDirection.Both,
                               WeirFormula = new GatedWeirFormula
                                                 {
                                                     ContractionCoefficient = 0.63,
                                                     GateOpening = 0.9,
                                                     MaxFlowPos = 0,
                                                     MaxFlowNeg = 0,
                                                     UseMaxFlowPos = false,
                                                     UseMaxFlowNeg = false,
                                                     LateralContraction = 1
                                                 }
                           };

            var flowModel1D = GetFlowModel1D(weir);

            RunModelWithWeir(weir, flowModel1D);

            // check weir values for last time step
            var location = new NetworkLocation(weir.Branch, weir.Chainage);
            var depthValue = flowModel1D.OutputDepth.AddTimeFilter(flowModel1D.CurrentTime).Evaluate(location);
            var velocityValue = flowModel1D.OutputVelocity.AddTimeFilter(flowModel1D.CurrentTime).Evaluate(location);

            Assert.AreEqual(5.1358349549772164, depthValue, delta);
            Assert.AreEqual(0.0, velocityValue, delta);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ExecuteWithGeneralStructure()
        {
            // init weir
            var weir = new Weir
            {
                OffsetY = 150,
                CrestWidth = 2,
                CrestLevel = 0.7,
                FlowDirection = FlowDirection.Both,
                WeirFormula = new GeneralStructureWeirFormula()
                {
                    WidthLeftSideOfStructure = 8.0,
                    BedLevelRightSideOfStructure = 1.3,
                    WidthRightSideOfStructure = 7.5,
                    BedLevelRightSideStructure = 1.4,
                    WidthStructureRightSide = 6.0,
                    BedLevelStructureCentre = 1.5,
                    WidthStructureCentre = 7.0,
                    BedLevelLeftSideStructure = 1.35,
                    WidthStructureLeftSide = 8,
                    BedLevelLeftSideOfStructure = 1.2,
                    
                    GateOpening = 12.0,
                    PositiveFreeGateFlow = 0.96,
                    PositiveDrownedGateFlow = 0.94,
                    PositiveFreeWeirFlow = 0.98,
                    PositiveDrownedWeirFlow = 0.96,
                    PositiveContractionCoefficient = 0.6,
                    NegativeFreeGateFlow = 0.86,
                    NegativeDrownedGateFlow = 0.84,
                    NegativeFreeWeirFlow = 0.88,
                    NegativeDrownedWeirFlow = 0.86,
                    NegativeContractionCoefficient = 0.5,
                    UseExtraResistance = true,
                    ExtraResistance = 0.01
                }
            };
            //values copied from Structuretests.TestStructGeneralStructure

            // create simplest network
            RunModelWithWeir(weir);
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void ExecuteWithFreeFormWeir()
        {
            double[] y = new[] { 0.0, 3.1, 6.2, 7.3, 9.4 };
            double[] z = new[] { 4.1, 3.2, 1.7, 2.3, 4.1 };
            // init weir
            var weir = new Weir { OffsetY = 150, CrestWidth = 2, CrestLevel = 0.7, FlowDirection = FlowDirection.Both };
            var formula = new FreeFormWeirFormula { DischargeCoefficient = 0.63 };
            formula.SetShape(y, z);
            weir.WeirFormula = formula;

            var flowModel1D = GetFlowModel1D(weir);

            RunModelWithWeir(weir, flowModel1D);

            // check weir values for last time step
            var location = new NetworkLocation(weir.Branch, weir.Chainage);
            var depthValue = flowModel1D.OutputDepth.AddTimeFilter(flowModel1D.CurrentTime).Evaluate(location);
            var velocityValue = flowModel1D.OutputVelocity.AddTimeFilter(flowModel1D.CurrentTime).Evaluate(location);

            Assert.AreEqual(5.1457181779698677, depthValue, delta);
            Assert.AreEqual(0.0, velocityValue, delta);
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void CloneModelWithRiverWeirFormula()
        {
            var submergeReduction = FunctionHelper.Get1DFunction<double, double>("submergereductionpos", "S", "R");
            submergeReduction[0.82] = 1.0;
            submergeReduction[0.92] = 0.7;
            submergeReduction[0.94] = 0.3;
            submergeReduction[1.0] = 0.0;

            // init weir and formula
            var weir = new Weir { OffsetY = 150, CrestWidth = 10, CrestLevel = 1.5 };
            weir.WeirFormula = new RiverWeirFormula
            {
                CorrectionCoefficientPos = 1,
                SubmergeLimitPos = 0.82,
                CorrectionCoefficientNeg = 1,
                SubmergeLimitNeg = 0.82,
                SubmergeReductionPos = submergeReduction,
                SubmergeReductionNeg = submergeReduction
            };

            WaterFlowModel1D flowModel1D = GetFlowModel1D(weir);
            WaterFlowModel1D clonedModel = (WaterFlowModel1D) flowModel1D.Clone();
            RiverWeirFormula clonedRiverWeirFormula = (RiverWeirFormula) clonedModel.Network.Weirs.First().WeirFormula;
            Assert.AreEqual(clonedRiverWeirFormula.CorrectionCoefficientNeg, 
                ((RiverWeirFormula)flowModel1D.Network.Weirs.First().WeirFormula).CorrectionCoefficientNeg);
            Assert.AreEqual(clonedRiverWeirFormula.CorrectionCoefficientNeg, 
                ((RiverWeirFormula)flowModel1D.Network.Weirs.First().WeirFormula).CorrectionCoefficientPos);
            Assert.AreEqual(clonedRiverWeirFormula.SubmergeReductionNeg.GetValues(),
                ((RiverWeirFormula)flowModel1D.Network.Weirs.First().WeirFormula).SubmergeReductionNeg.GetValues());
            Assert.AreEqual(clonedRiverWeirFormula.SubmergeReductionPos.GetValues(),
                ((RiverWeirFormula)flowModel1D.Network.Weirs.First().WeirFormula).SubmergeReductionPos.GetValues());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneModelWithPierWeirFormula()
        {
            // init weir
            var weir = new Weir {OffsetY = 150, CrestWidth = 10, CrestLevel = 1.5};
            weir.WeirFormula = new PierWeirFormula()
                                   {
                                       AbutmentContractionNeg = 0.15,
                                       AbutmentContractionPos = 0.1,
                                       DesignHeadNeg = 2.5,
                                       DesignHeadPos = 3,
                                       NumberOfPiers = 3,
                                       PierContractionPos = 0.01,
                                       PierContractionNeg = 0.015,
                                       UpstreamFacePos = 10,
                                       UpstreamFaceNeg = 8
                                   };

            // create simplest network
            WaterFlowModel1D flowModel1D = GetFlowModel1D(weir);
            WaterFlowModel1D clonedModel = (WaterFlowModel1D)flowModel1D.Clone();
            PierWeirFormula clonedPierWeirForlula = (PierWeirFormula) clonedModel.Network.Weirs.First().WeirFormula;
            Assert.AreEqual(clonedPierWeirForlula.AbutmentContractionNeg,
                            ((PierWeirFormula) flowModel1D.Network.Weirs.First().WeirFormula).AbutmentContractionNeg);
            
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneWithGatedWeirFormula()
        {
            // init weir
            var weir = new Weir {OffsetY = 150, CrestWidth = 2, CrestLevel = 0.7, FlowDirection = FlowDirection.Both};
            weir.WeirFormula = new GatedWeirFormula
                                   {
                                       ContractionCoefficient = 0.63,
                                       GateOpening = 0.9,
                                       MaxFlowPos = 0,
                                       MaxFlowNeg = 0,
                                       UseMaxFlowPos = false,
                                       UseMaxFlowNeg = false,
                                       LateralContraction = 1
                                   };

            
            WaterFlowModel1D flowModel1D = GetFlowModel1D(weir);
            WaterFlowModel1D clonedModel = (WaterFlowModel1D) flowModel1D.Clone();
            GatedWeirFormula clonedGatedWeirFormula = (GatedWeirFormula) clonedModel.Network.Weirs.First().WeirFormula;

            Assert.AreEqual(clonedGatedWeirFormula.LateralContraction,
                ((GatedWeirFormula)flowModel1D.Network.Weirs.First().WeirFormula).LateralContraction);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CloneModelWithFreeFormWeir()
        {
            double[] y = new[] {0.0, 3.1, 6.2, 7.3, 9.4};
            double[] z = new[] {4.1, 3.2, 1.7, 2.3, 4.1};
            // init weir
            var weir = new Weir {OffsetY = 150, CrestWidth = 2, CrestLevel = 0.7, FlowDirection = FlowDirection.Both};
            var formula = new FreeFormWeirFormula {DischargeCoefficient = 0.63};
            formula.SetShape(y, z);
            weir.WeirFormula = formula;

            WaterFlowModel1D flowModel1D = GetFlowModel1D(weir);
            WaterFlowModel1D clonedModel = (WaterFlowModel1D) flowModel1D.Clone();
            FreeFormWeirFormula clonedFormula = (FreeFormWeirFormula) clonedModel.Network.Weirs.First().WeirFormula;
            FreeFormWeirFormula origFormula = (FreeFormWeirFormula) flowModel1D.Network.Weirs.First().WeirFormula;
            for (int i = 0; i < clonedFormula.Shape.Coordinates.Length; i++)
            {
                Assert.AreEqual(clonedFormula.Shape.Coordinates[i].X, origFormula.Shape.Coordinates[i].X);
                Assert.AreEqual(clonedFormula.Shape.Coordinates[i].Y, origFormula.Shape.Coordinates[i].Y);
            }
        }
        
        /// <summary>
        /// Init a flowmodel with a weir
        /// </summary>
        /// <param name="weir"></param>
        /// <returns></returns>
        private WaterFlowModel1D GetFlowModel1D(IWeir weir)
        {
            var network = new HydroNetwork();
            var crossSectionType = new CrossSectionSectionType { Name = "Meen" };
            network.CrossSectionSectionTypes.Add(crossSectionType);

            // add nodes and branches
            var startCoordinate = new Coordinate(0, 0);
            var endCoordinate = new Coordinate(100, 0);

            IHydroNode node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(startCoordinate)};
            IHydroNode node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(endCoordinate)};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Channel("branch1", node1, node2, 100.0);
            
            var vertices = new List<Coordinate>
                               {
                                   startCoordinate,
                                   endCoordinate
                               };
            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());

            network.Branches.Add(branch1);

            // add cross-sections
            WaterFlowModel1DTestHelper.AddDefaultCrossSection(branch1, "crs1", 40.0);
            WaterFlowModel1DTestHelper.AddDefaultCrossSection(branch1, "crs2", 60.0);

            var compositeStructure = new CompositeBranchStructure { Chainage = 50 };
            NetworkHelper.AddBranchFeatureToBranch(compositeStructure, branch1, compositeStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeStructure, weir);

            // add discretization
            Discretization networkDiscretization = new Discretization
                                                       {
                                                           Network = network,
                                                           SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
                                                       };
            HydroNetworkHelper.GenerateDiscretization(networkDiscretization, branch1, 0, true, 5.0, true, false, false, branch1.Length / 10.0);

            // setup 1d flow model
            var flowModel1D = new WaterFlowModel1D();
            flowModel1D.NetworkDiscretization = networkDiscretization;
            //WaterFlowModel1D.TemplateDataZipFile =  WaterFlowModel1DTestHelper.TemplateDir;

            var t = DateTime.Now;
            // Round t down to nearest minute: (See TOOLS-23841)
            t = new DateTime(t.Ticks - (t.Ticks % (1000 * 1000 * 10 * 60)));
            flowModel1D.StartTime = t;
            flowModel1D.StopTime = t.AddMinutes(5);
            flowModel1D.TimeStep = new TimeSpan(0, 0, 1);
            flowModel1D.OutputTimeStep = new TimeSpan(0, 0, 1);

            // set network
            flowModel1D.Network = network;

            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue =  0.1 ;
            flowModel1D.InitialConditions.DefaultValue = 0.1;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == node1);
            boundaryConditionInflow.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;
            boundaryConditionInflow.Data[t] = 1.0;
            boundaryConditionInflow.Data[t.AddSeconds(30)] = 1.0;
            boundaryConditionInflow.Data[t.AddSeconds(60)] = 1.5;
            boundaryConditionInflow.Data[t.AddSeconds(120)] = 1.0;
            boundaryConditionInflow.Data[t.AddSeconds(180)] = 0.5;
            boundaryConditionInflow.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == node2);
            boundaryConditionOutflow.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
            boundaryConditionOutflow.Data[t] = 0.1;
            boundaryConditionOutflow.Data[t.AddSeconds(30)] = 0.1;
            boundaryConditionOutflow.Data[t.AddSeconds(60)] = 0.2;
            boundaryConditionOutflow.Data[t.AddSeconds(120)] = 0.3;
            boundaryConditionOutflow.Data[t.AddSeconds(180)] = 0.1;
            boundaryConditionOutflow.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

            flowModel1D.OutputSettings.LocationWaterDepth = AggregationOptions.Current;
            flowModel1D.OutputSettings.BranchVelocity = AggregationOptions.Current;

            return flowModel1D;
        }

        private void RunModelWithWeir(IWeir weir, WaterFlowModel1D flowModel = null)
        {
            var flowModel1D = flowModel ?? GetFlowModel1D(weir);
            WaterFlowModel1DDemoModelTestHelper.ReplaceStoreForOutputCoverages(flowModel1D);

            // Round t down to nearest minute: (See TOOLS-23841)
            var timeStepCount = 0;

            flowModel1D.StatusChanged += (sender, args) =>
            {
                if (flowModel1D.Status == ActivityStatus.Failed)
                {
                    log.DebugFormat("Model log: {0}", flowModel1D.LastRunLog);
                    Assert.Fail("Model run has failed");
                }

                if (flowModel1D.Status != ActivityStatus.Executed && flowModel1D.Status != ActivityStatus.Done) return;

                log.Debug(string.Format("timestep: {0}", timeStepCount));
                timeStepCount++;
            };

            ActivityRunner.RunActivity(flowModel1D);
            Assert.AreEqual(300, timeStepCount);

            Assert.AreEqual(ActivityStatus.Cleaned, flowModel1D.Status);
            Assert.IsTrue(flowModel1D.CurrentTime >= flowModel1D.StopTime);
        }

        [Test]
        public void CloneSimpleWeirFormula()
        {
            TestSimplePropertiesAreClonesForWeirFormula<SimpleWeirFormula>();
        }

        [Test]
        public void ClonePierWeirFormula()
        {
            TestSimplePropertiesAreClonesForWeirFormula<PierWeirFormula>();
        }

        [Test]
        public void CloneRiverWeirFormula()
        {
            TestSimplePropertiesAreClonesForWeirFormula<RiverWeirFormula>();
        }

        [Test]
        public void CloneFreeFormWeirFormula()
        {
            TestSimplePropertiesAreClonesForWeirFormula<FreeFormWeirFormula>();
        }

        [Test]
        public void CloneGatedWeirFormula()
        {
            TestSimplePropertiesAreClonesForWeirFormula<GatedWeirFormula>();
        }

        [Test]
        public void CloneGeneralStructureWeirFormula()
        {
            TestSimplePropertiesAreClonesForWeirFormula<GeneralStructureWeirFormula>();
        }

        private static void TestSimplePropertiesAreClonesForWeirFormula<TFormulaType>() where TFormulaType : IWeirFormula
        {
            var formula = Activator.CreateInstance(typeof(TFormulaType));
            if (typeof (TFormulaType) == typeof (GatedWeirFormula))
            {
                ReflectionTestHelper.FillRandomValuesForValueTypeProperties(formula, "UseLowerEdgeLevelTimeSeries");
            }
            else
            {
                ReflectionTestHelper.FillRandomValuesForValueTypeProperties(formula);
            }
            
            var clone = ((ICloneable) formula).Clone();
            ReflectionTestHelper.AssertPublicPropertiesAreEqual(formula, clone);
        }
    }
}