using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;
using DeltaShell.Plugins.DeveloperTools.Builders;
using DeltaShell.Plugins.FMSuite.FlowFM;
using GeoAPI.Extensions.CoordinateSystems;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;
using Point = NetTopologySuite.Geometries.Point;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.WaterFlowFMModel;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class Iterative1D2DCouplerTest
    {
        [Test]
        public void Iterative1D2DCouplerSetFlagsInFlow2DModel()
        {
            var twoDimModel = new WaterFlowFMModel();
            var oneDimModel = MockRepository.GenerateStrictMock<ITimeDependentModel>();

            oneDimModel.Expect(m => m.AllDataItems).Return(Enumerable.Empty<IDataItem>());
            var isPartOf1D2DModelGuiProperty = twoDimModel.ModelDefinition.GetModelProperty(GuiProperties.PartOf1D2DModel);
            isPartOf1D2DModelGuiProperty.Value = false;

            twoDimModel.DisableFlowNodeRenumbering = false;

            Assert.IsFalse((bool)isPartOf1D2DModelGuiProperty.Value);
            Assert.IsFalse(twoDimModel.DisableFlowNodeRenumbering);

            var coupler = new Iterative1D2DCoupler
            {
                Name = "testCoupling",
                Flow1DModel = oneDimModel,
                Flow2DModel = twoDimModel,
            };

            Assert.IsTrue((bool)isPartOf1D2DModelGuiProperty.Value);
            Assert.IsTrue(twoDimModel.DisableFlowNodeRenumbering);
        }

        [Test]
        public void Iterative1D2DCouplerAsksFlow2DModelForLinkOutput()
        {
            var twoDimModel = (ITimeDependentModel) MockRepository.GenerateStrictMock(typeof(ITimeDependentModel), new[] {typeof(IDimrModel),typeof(INotifyPropertyChanged)});
            var oneDimModel = MockRepository.GenerateStrictMock<ITimeDependentModel>();

            oneDimModel.Expect(m => m.AllDataItems).Return(Enumerable.Empty<IDataItem>());
            
            ((INotifyPropertyChanged) twoDimModel).Expect(m => m.PropertyChanged += null).IgnoreArguments();
            ((IDimrModel)twoDimModel).Expect(m => m.SetVar(null, string.Empty, string.Empty, string.Empty)).IgnoreArguments().Repeat.Twice();

            ((IDimrModel)twoDimModel).Expect(m => m.GetVar(Iterative1D2DCoupler.CellsToFeaturesName)).Return(Enumerable.Empty<ITimeSeries>().ToArray());
            ((IDimrModel)twoDimModel).Expect(m => m.GetVar(Iterative1D2DCoupler.GridPropertyName)).Return(null);
            
            var coupler = new Iterative1D2DCoupler
            {
                Name = "testCoupling",
                Flow1DModel = oneDimModel,
                Flow2DModel = twoDimModel,
            };

            var linkCoverages = coupler.LinkCoverages;
            twoDimModel.VerifyAllExpectations();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.WorkInProgress)]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void Iterative1D2DCouplerDataItemsTest()
        {
            var mduPath = TestHelper.GetTestFilePath(@"fmFloodingBoundary1\fmDemo.mdu");
            var path = TestHelper.CreateLocalCopy(mduPath);

            var flow1d = WaterFlowModel1DBuilder.CreateModelWithDemoNetwork();
            var fm = new WaterFlowFMModel(path);
            
            var coupled = new Iterative1D2DCoupler
            {
                Name = "testCoupling",
                Flow1DModel = flow1d,
                Flow2DModel = fm,
                HydroModel = new HydroModel() { Name = "myHydroModelName" }
            };

            Assert.IsNull(((Iterative1D2DCouplerData)coupled.Data).OutputDataItems.FirstOrDefault(), "Iterative1D2DCoupler DataItems collection should be empty");

            var numberOfValues = 3;
            var numberOfLinks = 5;
            var variableNames = new[] {"1d2d_blah1", "1d2d_blah2", "1d2d_blah3"};

            var timeSeriesList = variableNames.Select(n =>
                {
                    var timeSeries = new TimeSeries {Name = n};
                    timeSeries.Arguments.Add(new Variable<FlowLink>("FlowLink"));
                    timeSeries.Components.Add(new Variable<double>("value"));
                    timeSeries.Time.SetValues(Enumerable.Range(0, numberOfValues).Select(i => DateTime.Now.AddHours(i)));
                    timeSeries.Arguments[1].SetValues(Enumerable.Repeat(new FlowLink(0,1,new Edge(0,1)), numberOfLinks));
                    timeSeries.SetValues(Enumerable.Range(0, numberOfValues*numberOfLinks).Select(Convert.ToDouble));
                    return timeSeries;
                }).OfType<ITimeSeries>().ToList();

            fm.DataItems.Add(new DataItem(timeSeriesList) { Name = Iterative1D2DCoupler.CellsToFeaturesName });

            IDataItem testData = ((Iterative1D2DCouplerData)coupled.Data).OutputDataItems.FirstOrDefault();
            Assert.IsNotNull(testData, "Iterative1D2DCoupler DataItems collection should not be empty");
            Assert.AreEqual(((HydroModel)testData.Owner).Name, "myHydroModelName", "Iterative1D2DCoupler must assign an Owner (its HydroModel) to created DataItems");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        [NUnit.Framework.Category(TestCategory.WorkInProgress)] // This test somehow doesn't work, but will be obsolete when DIMR arrives. 
        public void Run1D2DMappingTest()
        {
            var mduPath = TestHelper.GetTestFilePath(@"fmFloodingBoundary1\fmDemo.mdu");
            var path = TestHelper.CreateLocalCopy(mduPath);
            using (var fm = new WaterFlowFMModel(path)
            {
                Name = "flowFm"
            })
            {
                const double shift = 50.0;
                using (var flow1d = new WaterFlowModel1D("flow1d"))
                {
                    var from = new Node("from")
                    {
                        Geometry = new Point(fm.GridExtent.MinX - shift, fm.GridExtent.MinY - shift)
                    };
                    var to = new Node("to")
                    {
                        Geometry = new Point(fm.GridExtent.MinX - shift, fm.GridExtent.MaxY + shift)
                    };

                    var channel = new Channel(from, to)
                    {
                        Name = "channel",
                        Geometry = new LineString(new[] { from.Geometry.Coordinate, to.Geometry.Coordinate })
                    };

                    NetworkHelper.AddChannelToHydroNetwork(flow1d.Network, channel);
                    flow1d.Network.Nodes.Add(from);
                    flow1d.Network.Nodes.Add(to);
                    CrossSectionHelper.AddCrossSection(channel, 500.0, 0.0);

                    HydroNetworkHelper.GenerateDiscretization(flow1d.NetworkDiscretization, channel, 1.0, false, 1.0,
                        false,
                        false, true, 100.0);
                    Assert.AreEqual(52, flow1d.NetworkDiscretization.Locations.GetValues().Count);

                    flow1d.StartTime = fm.StartTime = fm.ReferenceTime.AddDays(1);
                    flow1d.StopTime = fm.StopTime = fm.StartTime.AddHours(1);
                    flow1d.TimeStep = fm.TimeStep = new TimeSpan(0, 0, 1, 0);

                    var fmReport = fm.Validate();
                    Assert.AreEqual(0, fmReport.ErrorCount);

                    var flowReport = new WaterFlowModel1DModelValidator().Validate(flow1d);
                    Assert.AreEqual(0, flowReport.ErrorCount);

                    using (var coupler = new Iterative1D2DCoupler
                    {
                        Name = "testMapping",
                        Flow1DModel = flow1d,
                        Flow2DModel = fm
                    })
                    {
                        ActivityRunner.RunActivity(coupler);

                        Assert.AreEqual(ActivityStatus.Cleaned, coupler.Status);
                    }
                }
            }
        }

        [Test]
        public void CoordinateSystemShouldBeInSyncWithHydroModelRegion()
        {
            var mocks = new MockRepository();
            var hydroModel = MockRepository.GenerateMock<IHydroModel, INotifyPropertyChanged>();
            var region = mocks.Stub<IHydroRegion>();

            var coordinateSystem1 = mocks.Stub<ICoordinateSystem>();
            var coordinateSystem2 = mocks.Stub<ICoordinateSystem>();

            region.CoordinateSystem = coordinateSystem1;
            hydroModel.Expect(h => h.Region).Return(region).Repeat.Any();

            mocks.ReplayAll();

            var coupler = new Iterative1D2DCoupler{HydroModel = hydroModel};
            var featureCoverage = new FeatureCoverage("test") {CoordinateSystem = coordinateSystem1};

            // fake link coverages
            TypeUtils.SetField(coupler, "linkCoverages", new List<FeatureCoverage>{featureCoverage});

            Assert.AreEqual(coordinateSystem1, coupler.CoordinateSystem);
            Assert.AreEqual(coordinateSystem1, featureCoverage.CoordinateSystem);

            region.CoordinateSystem = coordinateSystem2;

            // raise property changed of hydromodel (this is normally done by event bubbling)
            ((INotifyPropertyChanged)hydroModel).Raise(h => h.PropertyChanged += null, region, new PropertyChangedEventArgs("CoordinateSystem"));

            Assert.AreEqual(coordinateSystem2, coupler.CoordinateSystem);
            Assert.AreEqual(coordinateSystem2, featureCoverage.CoordinateSystem);
        }
    }
}
