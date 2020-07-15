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

        [Test]
        public void GivenNetworkSideViewHelper_GettingFunctionsForPipes_ShouldWork()
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
                        Length = (i + 1) * 20,
                        LevelTarget = 2,
                        LevelSource = 4
                    };
                    pipe.CrossSection = CrossSection.CreateDefault(CrossSectionType.Standard, pipe, pipe.Length / 2);
                    return pipe;
                }).ToArray();

            network.Nodes.AddRange(manholes);
            network.Branches.AddRange(pipes);

            var route = RouteHelper.CreateRoute(new NetworkLocation(pipes[0], 10), new NetworkLocation(pipes[0], 15), new NetworkLocation(pipes[2], 40));

            // Act
            var functions = NetworkSideViewHelper.GetPipeSideViewFunctions(route).ToList();

            // Assert
            Assert.AreEqual(2, functions.Count);
            var top = functions[0];
            var bottom = functions[1];

            var topXValues = top.Arguments[0].Values;
            var bottomXValues = bottom.Arguments[0].Values;

            var topYValues = top.Components[0].Values;
            var bottomYValues = bottom.Components[0].Values;

            Assert.AreEqual(topXValues.Count, bottomXValues.Count,
                "Number of Top and Bottom values should be the same");

            Assert.AreEqual(topXValues, bottomXValues,
                "X locations should be the same (begin and end for each pipe in route)");

            var defaultCrossSectionHeight = CrossSectionDefinitionStandard.CreateDefault().HighestPoint;

            var expectedYValuesBottom = new double[] { 3.5, 2, 4, 2, 4, 3 };
            var expectedYValuesTop = expectedYValuesBottom.Select(v => v + defaultCrossSectionHeight).ToArray();

            Assert.AreEqual(expectedYValuesBottom, bottomYValues);
            Assert.AreEqual(expectedYValuesTop, topYValues);
        }

        [Test]
        public void GivenNetworkSideViewHelper_GetBranchChainageFromRelativeLength_ShouldReturnCorrectSegmentsAndChainage()
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


            // Act
            var resultsStart = NetworkSideViewHelper.GetBranchChainageFromRelativeLength(route, 0);
            var resultsEnd = NetworkSideViewHelper.GetBranchChainageFromRelativeLength(route, route.Segments.Values.Sum(s => s.Length));
            
            var firstSegment = route.Segments.Values[0];
            var secondSegment = route.Segments.Values[1]; 

            var resultOnSegmentEnd = NetworkSideViewHelper.GetBranchChainageFromRelativeLength(route, firstSegment.EndChainage- firstSegment.Chainage);

            var resultOnSegment1 = NetworkSideViewHelper.GetBranchChainageFromRelativeLength(route, firstSegment.Length /2);
            var resultOnSegment2 = NetworkSideViewHelper.GetBranchChainageFromRelativeLength(route, firstSegment.Length + secondSegment.Length / 2);

            // Assert
            Assert.AreEqual(resultsStart.Item1, route.Segments.Values[0]);
            Assert.AreEqual(resultsStart.Item2, route.Segments.Values[0].Chainage);

            Assert.AreEqual(resultsEnd.Item1, route.Segments.Values.Last());
            Assert.AreEqual(resultsEnd.Item2, route.Segments.Values.Last().EndChainage);

            Assert.AreEqual(resultOnSegmentEnd.Item1, secondSegment);
            Assert.AreEqual(resultOnSegmentEnd.Item2, 0);

            Assert.AreEqual(resultOnSegment1.Item1, route.Segments.Values[0]);
            Assert.AreEqual(resultOnSegment1.Item2, firstSegment.Chainage + firstSegment.Length /2);

            Assert.AreEqual(resultOnSegment2.Item1, route.Segments.Values[1]);
            Assert.AreEqual(resultOnSegment2.Item2, secondSegment.Chainage + secondSegment.Length / 2);
        }
    }
}