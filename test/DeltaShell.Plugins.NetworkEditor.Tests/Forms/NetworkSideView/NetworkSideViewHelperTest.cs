using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Functions;
using DelftTools.Functions.Binding;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using Color = System.Drawing.Color;

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

        [Test]
        public void AddPipeSurfaceLevelsInRoute_WithExistingLocationOnCoverage_ReplacesLocation()
        {
            Pipe pipe = CreatePipe();

            var network = Substitute.For<INetwork>();
            network.Branches.Returns(new EventedList<IBranch>(new[]
            {
                pipe
            }));

            var startRouteLocation = new NetworkLocation(pipe, 0);
            var endRouteLocation = new NetworkLocation(pipe, 100);

            var route = new Route { Network = network };
            route.SetLocations(new INetworkLocation[]
            {
                startRouteLocation,
                endRouteLocation
            });

            var networkCoverage = new NetworkCoverage();
            networkCoverage.SetLocations(new INetworkLocation[]
            {
                startRouteLocation,
                endRouteLocation
            });

            // Call
            NetworkSideViewHelper.AddPipeSurfaceLevelsInRoute(route, networkCoverage);

            // Assert
            Assert.That(networkCoverage.Locations.Values, Has.Count.EqualTo(2));
            Assert.That(networkCoverage.Locations.Values[0], Is.EqualTo(startRouteLocation));
            Assert.That(networkCoverage.Locations.Values[0], Is.Not.SameAs(startRouteLocation));
            Assert.That(networkCoverage.Locations.Values[1], Is.EqualTo(endRouteLocation));
            Assert.That(networkCoverage.Locations.Values[1], Is.Not.SameAs(endRouteLocation));
        }

        [Test]
        public void UpdateMinMaxToEnsureVerticalResolution_MinAndMaxHaveTheSameNonZeroValue_UpdatesMinAndMax()
        {
            // Setup
            const double fakeRangeConstant = 0.10;
            double min = 1.23;
            double max = min;
            double fakeRange = fakeRangeConstant * min;
            double expectedMinValue = min - fakeRange;
            double expectedMaxValue = max + fakeRange;
            
            // Call
            NetworkSideViewHelper.UpdateMinMaxToEnsureVerticalResolution(ref min, ref max);
            
            // Assert
            Assert.That(min, Is.EqualTo(expectedMinValue));
            Assert.That(max, Is.EqualTo(expectedMaxValue));
        }
        
        [Test]
        public void UpdateMinMaxToEnsureVerticalResolution_MinAndMaxHaveDifferentValues_DoesNotChangeMinOrMax()
        {
            // Setup
            const double minValue = 1.23;
            const double maxValue = 4.56;
            double min = minValue;
            double max = maxValue;
            
            // Call
            NetworkSideViewHelper.UpdateMinMaxToEnsureVerticalResolution(ref min, ref max);
            
            // Assert
            Assert.That(min, Is.EqualTo(minValue));
            Assert.That(max, Is.EqualTo(maxValue));
        }
        
        [Test]
        public void UpdateMinMaxToEnsureVerticalResolution_MinAndMaxZero_SetsRangeTo5()
        {
            // Setup
            const double fakeRange = 5;
            double min = 0;
            double max = 0;

            double expectedMin = min - fakeRange;
            double expectedMax = max + fakeRange;

            // Call
            NetworkSideViewHelper.UpdateMinMaxToEnsureVerticalResolution(ref min, ref max);
            
            // Assert
            Assert.That(min, Is.EqualTo(expectedMin));
            Assert.That(max, Is.EqualTo(expectedMax));
        }

        [Test]
        public void GetReversed_RouteHasNoSegment_ReturnsFalse()
        {
            // Setup
            Pipe pipe = CreatePipe();
            
            var structure = Substitute.For<IStructure1D>();
            structure.Branch.Returns(pipe);
            structure.Chainage.Returns(123);

            var route = new Route();

            // Precondition
            Assert.That(route.Segments.Values, Is.Empty);

            // Call
            bool result = NetworkSideViewHelper.GetReversed(route, structure);

            // Assert
            Assert.That(result, Is.False);
        }
        
        [Test]
        public void GetReversed_RouteSegmentEndChainageSmallerThanChainage_ReturnsTrue()
        {
            // Setup
            const double structureChainage = 100;
            const double segmentEndChainage = structureChainage - 50;
            const double segmentChainage = structureChainage + 50;
            
            Pipe pipe = CreatePipe();
            
            var segment = Substitute.For<INetworkSegment>();
            segment.EndChainage.Returns(segmentEndChainage);
            segment.Chainage.Returns(segmentChainage);
            segment.Branch.Returns(pipe);
            INetworkSegment[] segments = { segment };

            var structure = Substitute.For<IStructure1D>();
            structure.Branch.Returns(pipe);
            structure.Chainage.Returns(structureChainage);

            var route = new Route();
            route.Segments.SetValues(segments);

            // Call
            bool result = NetworkSideViewHelper.GetReversed(route, structure);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetReversed_RouteSegmentEndChainageLargerThanChainage_ReturnsFalse()
        {
            // Setup
            const double structureChainage = 100;
            const double segmentEndChainage = structureChainage + 50;
            const double segmentChainage = structureChainage - 50;
            
            Pipe pipe = CreatePipe();
            
            var segment = Substitute.For<INetworkSegment>();
            segment.EndChainage.Returns(segmentEndChainage);
            segment.Chainage.Returns(segmentChainage);
            segment.Branch.Returns(pipe);
            INetworkSegment[] segments = { segment };

            var structure = Substitute.For<IStructure1D>();
            structure.Branch.Returns(pipe);
            structure.Chainage.Returns(structureChainage);

            var route = new Route();
            route.Segments.SetValues(segments);

            // Call
            bool result = NetworkSideViewHelper.GetReversed(route, structure);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetLineSeries_ReturnsCorrectLineSeries()
        {
            // Setup
            var function = Substitute.For<IFunction>();
            function.Name.Returns("Function Name");

            var xVariable = Substitute.For<IVariable>();
            xVariable.DisplayName.Returns("x DisplayName");

            var yComponent = Substitute.For<IVariable>();
            yComponent.DisplayName.Returns("y DisplayName");

            var functionBindingList = new FunctionBindingList(function);
            Color penColor = Color.Aqua;

            // Call
            ILineChartSeries lineSeries = NetworkSideViewHelper.GetLineSeries(function, xVariable, yComponent, 
                                                                              functionBindingList, penColor);

            // Assert
            Assert.That(lineSeries, Is.Not.Null);
            
            Assert.That(lineSeries.DataSource, Is.EqualTo(functionBindingList));
            Assert.That(lineSeries.XValuesDataMember, Is.EqualTo(xVariable.DisplayName));
            Assert.That(lineSeries.YValuesDataMember, Is.EqualTo(yComponent.DisplayName));
            Assert.That(lineSeries.Color, Is.EqualTo(penColor));
            Assert.That(lineSeries.PointerStyle, Is.EqualTo(PointerStyles.Nothing));
            Assert.That(lineSeries.UpdateASynchronously, Is.True);
        }
        
        [Test]
        public void GetPointSeries_ReturnsCorrectPointSeries()
        {
            // Setup
            var function = Substitute.For<IFunction>();
            function.Name.Returns("Function Name");

            var xVariable = Substitute.For<IVariable>();
            xVariable.DisplayName.Returns("x DisplayName");

            var yComponent = Substitute.For<IVariable>();
            yComponent.DisplayName.Returns("y DisplayName");

            var functionBindingList = new FunctionBindingList(function);
            Color fillColor = Color.Aqua;
            const PointerStyles randomPointerStyle = PointerStyles.Hexagon;
            const int randomPointerSize = 123;

            // Call
            IPointChartSeries pointSeries = NetworkSideViewHelper.GetPointSeries(function, xVariable, yComponent, 
                                                                                functionBindingList, fillColor, randomPointerStyle, 
                                                                                randomPointerSize);

            // Assert
            Assert.That(pointSeries, Is.Not.Null);
            
            Assert.That(pointSeries.DataSource, Is.EqualTo(functionBindingList));
            Assert.That(pointSeries.XValuesDataMember, Is.EqualTo(xVariable.DisplayName));
            Assert.That(pointSeries.YValuesDataMember, Is.EqualTo(yComponent.DisplayName));
            Assert.That(pointSeries.Color, Is.EqualTo(fillColor));
            Assert.That(pointSeries.LineColor, Is.EqualTo(Color.Black));
            Assert.That(pointSeries.Style, Is.EqualTo(randomPointerStyle));
            Assert.That(pointSeries.Size, Is.EqualTo(randomPointerSize));
        }
        
        [Test]
        public void GetAreaSeries_ReturnsCorrectAreaSeries()
        {
            // Setup
            var function = Substitute.For<IFunction>();
            function.Name.Returns("Function Name");

            var xVariable = Substitute.For<IVariable>();
            xVariable.DisplayName.Returns("x DisplayName");

            var yComponent = Substitute.For<IVariable>();
            yComponent.DisplayName.Returns("y DisplayName");

            var functionBindingList = new FunctionBindingList(function);
            Color fillColor = Color.Aqua;

            // Call
            IAreaChartSeries areaSeries = NetworkSideViewHelper.GetAreaSeries(function, xVariable, yComponent, 
                                                                               functionBindingList, fillColor);

            // Assert
            Assert.That(areaSeries, Is.Not.Null);
            
            Assert.That(areaSeries.DataSource, Is.EqualTo(functionBindingList));
            Assert.That(areaSeries.XValuesDataMember, Is.EqualTo(xVariable.DisplayName));
            Assert.That(areaSeries.YValuesDataMember, Is.EqualTo(yComponent.DisplayName));
            Assert.That(areaSeries.Color, Is.EqualTo(fillColor));
            Assert.That(areaSeries.LineColor, Is.EqualTo(TeeChartHelper.DarkenColor(fillColor, 60)));
            Assert.That(areaSeries.PointerStyle, Is.EqualTo(PointerStyles.Nothing));
            Assert.That(areaSeries.UpdateASynchronously, Is.True);
        }

        private static Pipe CreatePipe()
        {
            var pipe = new Pipe
            {
                Source = Substitute.For<IManhole>(),
                Target = Substitute.For<IManhole>(),
                Length = 100,
                SourceCompartment = Substitute.For<ICompartment>(),
                TargetCompartment = Substitute.For<ICompartment>(),
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(100, 0)
                })
            };

            pipe.SourceCompartment.SurfaceLevel = 1.23;
            pipe.TargetCompartment.SurfaceLevel = 4.56;

            return pipe;
        }
    }
}