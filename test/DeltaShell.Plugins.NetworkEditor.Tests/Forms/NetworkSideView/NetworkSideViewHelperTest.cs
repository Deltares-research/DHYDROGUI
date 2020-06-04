using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.NetworkSideView
{
    [TestFixture]
    public class NetworkSideViewHelperTest
    {
        [Test]
        public void GivenNetworkSideView_GettingManholeChainagesAlongRoute_ShouldGiveCorrectRelativeDistance()
        {
            //Arrange
            var network = new HydroNetwork();

            var numberOfPipes = 5;
            var manholes = Enumerable.Range(1, numberOfPipes + 1)
                .Select(i => new Manhole($"Manhole{i}")
                {
                    Geometry = new Point(i * 100, 0),
                    Compartments = new EventedList<ICompartment>
                    {
                        new Compartment{Name = $"Compartment{i}" }
                    }
                }).ToArray();

            var pipes = Enumerable.Range(1, numberOfPipes)
                .Select(i =>
                {
                    var pipe = new Pipe
                    {
                        Name = $"Pipe{i}",
                        Source = manholes[i - 1],
                        SourceCompartment = manholes[i - 1].Compartments[0],
                        Target = manholes[i],
                        TargetCompartment = manholes[i].Compartments[0],
                        Length = 100
                    };
                    pipe.CrossSection = CrossSection.CreateDefault(CrossSectionType.Standard, pipe, pipe.Length / 2);
                    return pipe;
                }).ToArray();

            network.Nodes.AddRange(manholes);
            network.Branches.AddRange(pipes);

            var route = RouteHelper.CreateRoute(new NetworkLocation(pipes[0], 20), new NetworkLocation(pipes[2], 40));

            // Act & Assert
            var nodesAndOffset = NetworkSideViewHelper.GetNodesInRouteWithChainage<IManhole>(route).ToList();
            Assert.AreEqual(2, nodesAndOffset.Count);
            Assert.AreEqual(manholes[1], nodesAndOffset[0].Item1);
            Assert.AreEqual(80, nodesAndOffset[0].Item2);

            Assert.AreEqual(manholes[2], nodesAndOffset[1].Item1);
            Assert.AreEqual(180, nodesAndOffset[1].Item2);
        }
    }
}