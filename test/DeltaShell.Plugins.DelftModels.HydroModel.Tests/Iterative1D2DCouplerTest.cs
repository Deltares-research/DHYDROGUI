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
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using GeoAPI.Extensions.CoordinateSystems;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;
using Point = NetTopologySuite.Geometries.Point;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

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
