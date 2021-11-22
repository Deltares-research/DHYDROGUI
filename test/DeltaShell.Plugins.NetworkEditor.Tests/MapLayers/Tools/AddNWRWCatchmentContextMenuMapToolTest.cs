using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Tools
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class AddNWRWCatchmentContextMenuMapToolTest
    {
        [Test]
        public void GivenAddNWRWCatchmentContextMenuMapTool_GettingContextMenu_ShouldGiveOptionToCreateNwrwCatchment()
        {
            // Setup
            var basin = new DrainageBasin();
            var hydroNetwork = new HydroNetwork();
            IHydroRegion hydroRegion = CreateHydroRegionWith(basin, hydroNetwork);

            var pipe = new Pipe();
            hydroNetwork.Branches.Add(pipe);

            var manhole = new Manhole("Manhole1") { Network = hydroNetwork };
            manhole.Compartments.Add(new Compartment("Compartment1"));
            manhole.IncomingBranches.Add(pipe);

            IMapControl mapControl = CreateMapControlWithSelection(manhole);

            var tool = new AddNWRWCatchmentContextMenuMapTool { MapControl = mapControl };

            // Act
            IEnumerable<MapToolContextMenuItem> mapToolContextMenuItems = tool.GetContextMenuItems(new Coordinate(0, 0));
            MapToolContextMenuItem mainMenu = mapToolContextMenuItems.OfType<MapToolContextMenuItem>().FirstOrDefault();
            Assert.NotNull(mainMenu);

            ToolStripMenuItem menu = mainMenu.MenuItem;
            Assert.AreEqual(3, menu.DropDownItems.Count);
            Assert.AreEqual("Manhole1 (Compartment1)", menu.DropDownItems[2].Text);

            menu.DropDownItems[2].PerformClick();

            // Assert
            Assert.AreEqual(1, basin.Catchments.Count(c => c.CatchmentType == CatchmentType.NWRW));
            Assert.AreEqual(1, hydroRegion.Links.Count);
            Assert.AreEqual(1, pipe.BranchFeatures.Count);
        }

        [Test]
        [TestCaseSource(nameof(ManholeWithExistingLateralSourceCases))]
        public void GivenAManholeWithLateralSource_WhenSelectingManholeToAddNwrwCatchment_ThenCatchmentIsNotAdded(
            IBranch incomingBranch,
            IBranch outgoingBranch,
            double expChainage)
        {
            // Setup
            var basin = new DrainageBasin();
            var hydroNetwork = new HydroNetwork();
            IHydroRegion hydroRegion = CreateHydroRegionWith(basin, hydroNetwork);

            hydroNetwork.Branches.Add(incomingBranch);
            hydroNetwork.Branches.Add(outgoingBranch);

            var manhole = new Manhole("some_manhole") { Network = hydroNetwork };
            manhole.Compartments.Add(new Compartment("some_compartment"));
            manhole.IncomingBranches.Add(incomingBranch);
            manhole.OutgoingBranches.Add(outgoingBranch);

            IMapControl mapControl = CreateMapControlWithSelection(manhole);

            var tool = new AddNWRWCatchmentContextMenuMapTool { MapControl = mapControl };

            ToolStripMenuItem item = GetContentMenuItem(tool, "some_manhole (some_compartment)");

            // Call
            void Call() => item.PerformClick();

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call).Single();
            Assert.That(error, Is.EqualTo($"A lateral source already exists at branch Pipe ({expChainage})"));
            Assert.That(basin.Catchments, Is.Empty);
            Assert.That(hydroRegion.Links, Is.Empty);
        }

        private static IEnumerable<TestCaseData> ManholeWithExistingLateralSourceCases()
        {
            const double branchLength = 100d;

            var lateralSource1 = new LateralSource { Chainage = branchLength };
            var incomingPipe1 = new Pipe { Length = branchLength };
            var outgoingPipe1 = new Pipe { Length = branchLength };
            incomingPipe1.BranchFeatures.Add(lateralSource1);

            yield return new TestCaseData(incomingPipe1, outgoingPipe1, branchLength);

            var lateralSource2 = new LateralSource { Chainage = 0d };
            var incomingPipe2 = new Pipe { Length = branchLength };
            var outgoingPipe2 = new Pipe { Length = branchLength };
            outgoingPipe2.BranchFeatures.Add(lateralSource2);

            yield return new TestCaseData(incomingPipe2, outgoingPipe2, 0d);
        }

        private static ToolStripMenuItem GetContentMenuItem(IMapTool tool, string text)
        {
            ToolStripMenuItem addNwrwMenu = tool.GetContextMenuItems(new Coordinate(0, 0)).Single().MenuItem;
            return addNwrwMenu.DropDownItems.OfType<ToolStripMenuItem>().Single(i => i.Text == text);
        }

        private static IMapControl CreateMapControlWithSelection(IFeature feature)
        {
            var map = Substitute.For<IMap>();
            map.PixelSize.Returns(10);

            var mapControl = Substitute.For<IMapControl>();
            mapControl.Map.Returns(map);
            mapControl.SelectedFeatures.Returns(new[]
            {
                feature
            });

            return mapControl;
        }

        private static IHydroRegion CreateHydroRegionWith(IRegion firstSubRegion, IRegion secondSubRegion)
        {
            var hydroRegion = Substitute.For<IHydroRegion>();

            hydroRegion.SubRegions.Returns(new EventedList<IRegion>
            {
                firstSubRegion,
                secondSubRegion
            });
            hydroRegion.Links.Returns(new EventedList<HydroLink>());

            firstSubRegion.Parent = hydroRegion;
            secondSubRegion.Parent = hydroRegion;

            return hydroRegion;
        }
    }
}