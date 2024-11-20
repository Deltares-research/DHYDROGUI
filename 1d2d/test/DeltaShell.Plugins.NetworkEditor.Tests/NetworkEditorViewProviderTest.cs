using System;
using System.Collections.Generic;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui.Forms.ViewManager;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class NetworkEditorViewProviderTest
    {
        [Test]
        public void CreateCompositeStructureViewForPump()
        {
            var mocks = new MockRepository();
            var gui = mocks.Stub<IGui>();
            var plugin = new NetworkEditorGuiPlugin();

            var dockingManager = mocks.Stub<IDockingManager>();

            // avoid gui event registration
            TypeUtils.SetField(plugin, "gui", gui);

            var viewList = new ViewList(dockingManager, ViewLocation.Document);
            var viewResolver = new ViewResolver(viewList, plugin.GetViewInfoObjects());

            gui.Expect(g => g.DocumentViewsResolver).Return(viewResolver).Repeat.Any();
            gui.Expect(g => g.DocumentViews).Return(viewList).Repeat.Any();
            gui.Expect(g => g.Plugins).Return(new List<GuiPlugin> { plugin }).Repeat.Any();
            gui.Expect(g => g.SelectedModel).Return(null).Repeat.Any();
            gui.Expect(g => g.SelectionChanged += Arg<EventHandler<SelectedItemChangedEventArgs>>.Is.Anything).Repeat.Any();

            mocks.ReplayAll();

            //setup a pump with a parent
            var pump = new Pump();
            var compositeBranchStructure = new CompositeBranchStructure();
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);

            viewResolver.OpenViewForData(pump);
            var view = viewList.ActiveView;

            //should return a composite structure view with the parent structure of the pump inside
            Assert.AreEqual(typeof(CompositeStructureView), view.GetType());
            Assert.AreEqual(compositeBranchStructure, view.Data);
        }
    }
};