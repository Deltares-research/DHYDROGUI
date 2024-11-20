using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Link1d2d;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class SetGeometry1D2DLinksTest
    {
        [Test]
        public void GivenLinkWithoutStartPointWhenSetGeometry1D2DLinksThenExpectZeroLogMessages()
        {
            var link = Substitute.For<ILink1D2D>();
            var locations = Substitute.For<IVariable<INetworkLocation>>();
            var cells = Substitute.For<IList<Cell>>();
            TestHelper.AssertLogMessagesCount(() => Links1D2DHelper.SetGeometry1D2DLinks(Enumerable.Repeat(link, 1), locations, cells), 0);
        }

        [Test]
        public void GivenLinkWithoutStartPointWhenSetGeometry1D2DLinksThenExpectLogErrorMessage()
        {
            var link = Substitute.For<ILink1D2D>();
            link.DiscretisationPointIndex = 1;
            var locations = Substitute.For<IVariable<INetworkLocation>>();
            var location = Substitute.For<INetworkLocation>();
            var values = new MultiDimensionalArray<INetworkLocation> { location };
            locations.Values.Returns(values);
            var cells = new List<Cell>();
            var cell = Substitute.For<Cell>();
            cells.Add(cell);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => Links1D2DHelper.SetGeometry1D2DLinks(Enumerable.Repeat(link, 1), locations, cells), Properties.Resources.Links1D2DHelper_SetGeometry1D2DLinks__1d2d_link_discretization_point_problem_);
        }
        
        [Test]
        public void GivenLinkWithoutEndCellWhenSetGeometry1D2DLinksThenExpectLogErrorMessage()
        {
            var link = Substitute.For<ILink1D2D>();
            link.FaceIndex = 1;
            var locations = Substitute.For<IVariable<INetworkLocation>>();
            var location = Substitute.For<INetworkLocation>();
            var values = new MultiDimensionalArray<INetworkLocation> { location };
            locations.Values.Returns(values);
            var cells = new List<Cell>();
            var cell = Substitute.For<Cell>();
            cells.Add(cell);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => Links1D2DHelper.SetGeometry1D2DLinks(Enumerable.Repeat(link, 1), locations, cells), Properties.Resources.Links1D2DHelper_SetGeometry1D2DLinks__1d2d_link_grid_cell_problem_);
        }

    }
}