using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Api;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Tools
{
    [TestFixture]
    public class AddNWRWCatchmentContextMenuMapToolTest
    {
        [Test]
        public void GivenAddNWRWCatchmentContextMenuMapTool_GettingContextMenu_ShouldGiveOptionToCreateNwrwCatchment()
        {
            //Arrange
            var manhole = new Manhole("Manhole1");
            manhole.Compartments.Add(new Compartment("Compartment1"));

            var pipe = new Pipe();
            manhole.IncomingBranches.Add(pipe);
            var catchments = new EventedList<Catchment>();
            var links = new EventedList<HydroLink>();

            var mocks = new MockRepository();
            var mapControl = mocks.StrictMock<IMapControl>();
            var map = mocks.StrictMock<IMap>();
            var hydroNetwork = mocks.StrictMock<IHydroNetwork>();
            var hydroRegion = mocks.StrictMock<IHydroRegion>();
            var basin = mocks.StrictMock<IDrainageBasin>();
            
            hydroRegion.Expect(r => r.SubRegions).Return(new EventedList<IRegion>{hydroNetwork, basin});
            hydroRegion.Expect(r => r.Links).Return(links);
            hydroNetwork.Expect(n => n.Parent).Return(hydroRegion).Repeat.Any();

            basin.Expect(b => b.Catchments).Return(catchments);

            map.Expect(m => m.PixelSize).Return(10);
            mapControl.Expect(c => c.SelectedFeatures).Return(new[] {manhole});
            mapControl.Expect(c => c.Map).Return(map);
            mocks.ReplayAll();

            var tool = new AddNWRWCatchmentContextMenuMapTool{MapControl = mapControl};
            manhole.Network = hydroNetwork;

            // Act
            var mapToolContextMenuItems = tool.GetContextMenuItems(new Coordinate(0, 0));
            var mainMenu = mapToolContextMenuItems.OfType<MapToolContextMenuItem>().FirstOrDefault();
            Assert.NotNull(mainMenu);

            var menu = mainMenu.MenuItem;
            Assert.AreEqual(3, menu.DropDownItems.Count);
            Assert.AreEqual("Manhole1 (Compartment1)", menu.DropDownItems[2].Text);

            menu.DropDownItems[2].PerformClick();

            // Assert
            Assert.AreEqual(1, catchments.Count(c => c.CatchmentType == CatchmentType.NWRW));
            Assert.AreEqual(1, links.Count);
            Assert.AreEqual(1, pipe.BranchFeatures.Count);
            mocks.VerifyAll();
        }
    }
}