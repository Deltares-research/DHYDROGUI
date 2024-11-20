using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.Commands
{
    [TestFixture]
    public class RemoveRouteCommandTest
    {
        [Test]
        public void GivenRoutesAreEmpty_WhenRequestingEnabled_ThenReturnFalse()
        {
            // Arrange
            var routeSelectionFinder = Substitute.For<IRouteSelectionFinder>();
            routeSelectionFinder.IsRouteSelected(Arg.Any<IGui>()).Returns(false);

            // Act
            var command = new RemoveSelectedRouteCommand();
            command.RouteSelectionFinder = routeSelectionFinder;

            // Assert
            Assert.That(command.Enabled, Is.False);
        }

        [Test]
        public void GivenRoutesAreNotEmpty_WhenRequestingEnabled_ThenReturnTrue()
        {
            // Arrange
            var routeSelectionFinder = Substitute.For<IRouteSelectionFinder>();
            routeSelectionFinder.IsRouteSelected(Arg.Any<IGui>()).Returns(true);

            // Act
            var command = new RemoveSelectedRouteCommand();
            command.RouteSelectionFinder = routeSelectionFinder;

            // Assert
            Assert.That(command.Enabled, Is.True);
        }

        [Test]
        public void GivenRoute_WhenOnExecute_ThenRemoveRoute()
        {
            // Arrange
            HydroNetwork hydroNetwork = GetHydroNetworkWithBranch();
            Route route = AddNewRouteToHydroNetwork(hydroNetwork);

            var routeSelectionFinder = Substitute.For<IRouteSelectionFinder>();
            routeSelectionFinder.GetSelectedRoute(Arg.Any<IGui>()).Returns(route);

            var command = new RemoveSelectedRouteCommand();
            command.RouteSelectionFinder = routeSelectionFinder;

            Assert.That(hydroNetwork.Routes.Contains(route), Is.True);

            // Act
            command.Execute();

            // Assert
            Assert.That(hydroNetwork.Routes.Contains(route), Is.False);
        }

        [Test]
        public void GivenTwoRoutes_WhenOnExecute_ThenRemoveOneRoute()
        {
            // Arrange
            HydroNetwork hydroNetwork = GetHydroNetworkWithBranch();
            Route route = AddNewRouteToHydroNetwork(hydroNetwork);
            Route route2 = AddNewRouteToHydroNetwork(hydroNetwork);

            var routeSelectionFinder = Substitute.For<IRouteSelectionFinder>();
            routeSelectionFinder.GetSelectedRoute(Arg.Any<IGui>()).Returns(route);

            var command = new RemoveSelectedRouteCommand();
            command.RouteSelectionFinder = routeSelectionFinder;

            Assert.That(hydroNetwork.Routes.Contains(route), Is.True);

            // Act
            command.Execute();

            //Assert
            Assert.That(hydroNetwork.Routes.Contains(route), Is.False);
            Assert.That(hydroNetwork.Routes.Contains(route2), Is.True);
        }

        private static Route AddNewRouteToHydroNetwork(HydroNetwork hydroNetwork)
        {
            var route = new Route() { Network = hydroNetwork };
            hydroNetwork.Routes.Add(route);
            return route;
        }

        private static HydroNetwork GetHydroNetworkWithBranch()
        {
            var hydroNetwork = new HydroNetwork();
            Branch branch = GetNewBranch(hydroNetwork);
            hydroNetwork.Branches.Add(branch);
            return hydroNetwork;
        }

        private static Branch GetNewBranch(HydroNetwork hydroNetwork)
        {
            var branch = new Branch()
            {
                Name = "branch",
                Network = hydroNetwork,
                IsLengthCustom = true,
                Length = 100,
                OrderNumber = 1,
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(100, 0)
                })
            };
            return branch;
        }
    }
}