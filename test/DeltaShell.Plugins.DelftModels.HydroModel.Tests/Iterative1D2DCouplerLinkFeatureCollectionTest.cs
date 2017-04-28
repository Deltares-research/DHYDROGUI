using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class Iterative1D2DCouplerLinkFeatureCollectionTest
    {
        [Test]
        public void CouplerRegionCoordinateSystemShouldBeInSyncWithIterative1D2DCouplerLinkFeatureCollectionCoordinateSystem()
        {
            var mocks =  new MockRepository();
            var coupler = mocks.Stub<Iterative1D2DCoupler>();
            var hydroModel = MockRepository.GenerateMock<IHydroModel, INotifyPropertyChanged>();
            var region = mocks.Stub<IHydroRegion>();
            var coordinateSystem1 = mocks.Stub<ICoordinateSystem>();
            var coordinateSystem2 = mocks.Stub<ICoordinateSystem>();

            region.CoordinateSystem = coordinateSystem1;
            hydroModel.Expect(h => h.Region).Return(region).Repeat.Any();
            coupler.HydroModel = hydroModel;
            
            mocks.ReplayAll();

            var fc = new Iterative1D2DCouplerLinkFeatureCollection(new List<IFeature>()) {Coupler = coupler};

            Assert.AreEqual(coordinateSystem1,fc.CoordinateSystem);

            region.CoordinateSystem = coordinateSystem2;

            // raise property changed of hydromodel (this is normally done by event bubbling)
            ((INotifyPropertyChanged)hydroModel).Raise(h => h.PropertyChanged += null, this, new PropertyChangedEventArgs("CoordinateSystem"));

            Assert.AreEqual(coordinateSystem2, fc.CoordinateSystem);
        }
    }
}