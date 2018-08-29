using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DWithCrossSectionTest
    {
        [SetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RunWithCrossSectionStandard()
        {
            var crossSectionStandardRectangle = new CrossSectionStandardShapeRectangle();
            new CrossSectionStandardShapeCircle{Diameter = 4.0};
            var crossSection = new CrossSection(new CrossSectionDefinitionStandard(crossSectionStandardRectangle));
            crossSection.Name = "Crsdef";
            var flowModel1D = GetModelWithBranchFeature(crossSection);
            //add a roughness section
            crossSection.Definition.Sections.Add(new CrossSectionSection{MinY = 0,MaxY = 4.0,SectionType = flowModel1D.Network.CrossSectionSectionTypes.First()});//,SectionType = flowModel1D.
            
            flowModel1D.StatusChanged += (sender, args) =>
            {
                if (flowModel1D.Status != ActivityStatus.Executed) return;

                var reportItem = flowModel1D.DataItems.FirstOrDefault(di => di.Tag == "lastRunLogFileDataItem");
                if (reportItem == null) return;

                var report = (TextDocument)reportItem.Value;
                Trace.WriteLine(report.Content);
            };

            ActivityRunner.RunActivity(flowModel1D);

            Assert.AreEqual(ActivityStatus.Cleaned, flowModel1D.Status);
            IList<double> values = flowModel1D.OutputDepth.GetValues<double>(new VariableValueFilter<DateTime>(flowModel1D.OutputDepth.Arguments[0], flowModel1D.CurrentTime));
            Assert.IsTrue(values.Count > 0);
        }

        /// <summary>
        /// Init a flowmodel with a weir
        /// </summary>
        /// <param name="crossSection"></param>
        /// <returns></returns>
        [Category(TestCategory.Integration)]
        private static WaterFlowModel1D GetModelWithBranchFeature(IBranchFeature crossSection)
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var channel = network.Channels.First();
            var node1 = network.Nodes.First();
            var node2 = network.Nodes.Last();
            var crossSectionType = new CrossSectionSectionType { Name = "Meen" };
            network.CrossSectionSectionTypes.Add(crossSectionType);
  
            NetworkHelper.AddBranchFeatureToBranch(crossSection,channel,50);
            
            // add discretization
            var networkDiscretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };
            HydroNetworkHelper.GenerateDiscretization(networkDiscretization, channel, 0, true, 5.0, true, false, false, channel.Length / 10.0);

            DateTime t = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);


            // setup 1d flow model
            var flowModel1D = new WaterFlowModel1D
                                  {
                                      NetworkDiscretization = networkDiscretization,
                                      StartTime = t,
                                      StopTime = t.AddMinutes(5),
                                      TimeStep = new TimeSpan(0, 0, 1),
                                      OutputTimeStep = new TimeSpan(0, 0, 1),
                                      Network = network
                                  };
            

            
            // set initial conditions
            flowModel1D.InitialFlow.DefaultValue = 0.1;
            flowModel1D.InitialConditions.DefaultValue = 0.1;

            // set boundary conditions
            var boundaryConditionInflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == node1);
            boundaryConditionInflow.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            boundaryConditionInflow.Flow = 1.0;

            var boundaryConditionOutflow = flowModel1D.BoundaryConditions.First(bc => bc.Feature == node2);
            boundaryConditionOutflow.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            boundaryConditionOutflow.WaterLevel = 0;
            
            flowModel1D.OutputSettings.LocationWaterDepth = AggregationOptions.Current;

            return flowModel1D;
        }
    }
}