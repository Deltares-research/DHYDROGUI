using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Helpers
{
    [TestFixture]
    public class RouteSelectionFinderTest
    {
        [Test]
        public void GivenSelectedRoute_IsRouteSelected_ReturnTrue()
        {
            var selectedRoute = new Route();
            EventedList<IView> list = AddRouteCorrectlyToNetwork(selectedRoute);
            IGui guiMock = CreateGuiMock(list);

            var routeSelectionFinder = new RouteSelectionFinder();
            Assert.That(routeSelectionFinder.IsRouteSelected(guiMock), Is.True);
        }

        [Test]
        public void GivenNoRoute_IsRouteSelected_ReturnFalse()
        {
            EventedList<IView> list = HaveMapViewReturnNull();

            IGui guiMock = CreateGuiMock(list);

            var routeSelectionFinder = new RouteSelectionFinder();
            Assert.That(routeSelectionFinder.IsRouteSelected(guiMock), Is.False);
        }

        [Test]
        public void GivenSelectedRoute_WhenGetSelectedRoute_ReturnSelectedRoute()
        {
            var selectedRoute = new Route();
            EventedList<IView> list = AddRouteCorrectlyToNetwork(selectedRoute);
            IGui guiMock = CreateGuiMock(list);

            var routeSelectionFinder = new RouteSelectionFinder();
            Assert.That(routeSelectionFinder.GetSelectedRoute(guiMock), Is.EqualTo(selectedRoute));
        }

        [Test]
        public void GivenNoRoute_WhenGetSelectedRouteWithMapViewIsNull_ReturnNull()
        {
            EventedList<IView> list = HaveMapViewReturnNull();

            IGui guiMock = CreateGuiMock(list);

            var routeSelectionFinder = new RouteSelectionFinder();
            Assert.That(routeSelectionFinder.GetSelectedRoute(guiMock), Is.Null);
        }

        [Test]
        public void GivenNoRoute_WhenGetSelectedRouteWithHydroNetworkEditorMapToolIsNull_ReturnNull()
        {
            EventedList<IView> list = HaveHydroNetworkEditorMapToolReturnNull();

            IGui guiMock = CreateGuiMock(list);

            var routeSelectionFinder = new RouteSelectionFinder();
            Assert.That(routeSelectionFinder.GetSelectedRoute(guiMock), Is.Null);
        }

        [Test]
        public void GivenNoRoute_WhenGetSelectedRouteWithActiveNetworkCoverageGroupLayerIsNull_ReturnNull()
        {
            EventedList<IView> list = HaveActiveNetworkCoverageGroupLayerReturnNull();

            IGui guiMock = CreateGuiMock(list);

            var routeSelectionFinder = new RouteSelectionFinder();
            Assert.That(routeSelectionFinder.GetSelectedRoute(guiMock), Is.Null);
        }

        private static EventedList<IView> AddRouteCorrectlyToNetwork(Route selectedRoute)
        {
            var activeNetworkCoverageGroupLayer = Substitute.For<INetworkCoverageGroupLayer>();
            activeNetworkCoverageGroupLayer.NetworkCoverage.Returns(selectedRoute);

            var hydroNetworkEditorMapTool = Substitute.For<IHydroNetworkEditorMapTool>();
            hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer.Returns(activeNetworkCoverageGroupLayer);

            var mapview = new MapView();
            mapview.MapControl.Tools.Add(hydroNetworkEditorMapTool);

            var list = new EventedList<IView>();
            list.Add(mapview);
            return list;
        }

        private static EventedList<IView> HaveActiveNetworkCoverageGroupLayerReturnNull()
        {
            var hydroNetworkEditorMapTool = Substitute.For<IHydroNetworkEditorMapTool>();
            hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer.ReturnsNull();

            var mapview = new MapView();
            mapview.MapControl.Tools.Add(hydroNetworkEditorMapTool);

            var list = new EventedList<IView>();
            list.Add(mapview);
            return list;
        }

        private static EventedList<IView> HaveHydroNetworkEditorMapToolReturnNull()
        {
            var mapview = new MapView();

            var list = new EventedList<IView>();
            list.Add(mapview);
            return list;
        }

        private static EventedList<IView> HaveMapViewReturnNull()
        {
            var list = new EventedList<IView>();
            list.Add(null);
            return list;
        }

        private static IGui CreateGuiMock(EventedList<IView> list)
        {
            ICompositeView compViewMock = Substitute.For<ICompositeView, IView>();
            compViewMock.ChildViews.Returns(list);

            var viewListMock = Substitute.For<IViewList>();
            viewListMock.ActiveView.Returns(compViewMock);

            var guiMock = Substitute.For<IGui>();
            guiMock.DocumentViews.Returns(viewListMock);
            return guiMock;
        }
    }
}