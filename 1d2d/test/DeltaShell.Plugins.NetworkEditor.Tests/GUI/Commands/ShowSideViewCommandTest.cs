using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.Commands
{
    [TestFixture]
    public class ShowSideViewCommandTest
    {
        [Test]
        public void GivenRoutesAreEmpty_WhenRequestingEnabled_ThenReturnFalse()
        {
            // Arrange
            var routeSelectionFinder = Substitute.For<IRouteSelectionFinder>();
            routeSelectionFinder.IsRouteSelected(Arg.Any<IGui>()).Returns(false);

            // Act
            var command = new ShowSideViewCommand();
            command.RouteSelectionFinder = routeSelectionFinder;
            command.Log = Substitute.For<ILog>();
            
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
            var command = new ShowSideViewCommand();
            command.RouteSelectionFinder = routeSelectionFinder;
            command.Log = Substitute.For<ILog>();
            
            // Assert
            Assert.That(command.Enabled, Is.True);
        }

        [Test]
        public void GivenRoute_WhenOnExecute_ThenOpenSideViewCalled()
        {
            // Arrange
            var hydroNetwork = new HydroNetwork();
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

            hydroNetwork.Branches.Add(branch);
            var route = new Route() { Network = hydroNetwork };
            hydroNetwork.Routes.Add(route);

            var routeSelectionFinder = Substitute.For<IRouteSelectionFinder>();
            routeSelectionFinder.GetSelectedRoute(Arg.Any<IGui>()).Returns(route);
            var gui = Substitute.For<IGui>();
            gui.CommandHandler = Substitute.For<IGuiCommandHandler>();

            var command = new ShowSideViewCommand();
            command.RouteSelectionFinder = routeSelectionFinder;
            command.Log = Substitute.For<ILog>();
            command.Gui = gui;

            // Act
            command.Execute();

            // Assert
            gui.CommandHandler.Received(1).OpenView(route, typeof(NetworkSideView));
        }

        [Test]
        public void GivenNoRoute_WhenOnExecute_ThenLogError()
        {
            // Arrange
            var routeSelectionFinder = Substitute.For<IRouteSelectionFinder>();
            routeSelectionFinder.GetSelectedRoute(Arg.Any<IGui>()).ReturnsNull();
            var log = Substitute.For<ILog>();
            var command = new ShowSideViewCommand();
            command.RouteSelectionFinder = routeSelectionFinder;
            command.Log = log;

            // Act
            command.Execute();

            // Assert
            log.Received(1).ErrorFormat("No active route found in active map; can not display sideview.");
        }
    }
}