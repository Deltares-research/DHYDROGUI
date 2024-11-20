using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Gui.Forms.ViewManager;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using Rhino.Mocks;
using SharpMap.Converters.WellKnownText;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CompositeStructureView
{
    public class CompositeStructureViewTestHelper
    {
        public static HydroNetwork CreateDummyNetwork()
        {
            return CreateDummyNetwork(true);
        }

        public static HydroNetwork CreateDummyNetwork(bool createCrossSections)
        {
            // create network
            var network = new HydroNetwork();

            var node1 = new HydroNode("node1") {Geometry = new Point(0, 0)};
            var node2 = new HydroNode("node2") {Geometry = new Point(100, 0)};
            var node3 = new HydroNode("node3") {Geometry = new Point(100, 100)};

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node2)
                              {
                                  Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")
                              };
            //var branch2 = new Channel("branch2", node1, node2)
            //{
            //    Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")
            //};
            var branch2 = new Channel("branch2", node2, node3)
                              {
                                  Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 100 100)")
                              };

            //var branch1 = new Channel("branch1", node1, node2) { Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)") };
            //var branch2 = new Channel("branch2", node2, node3) { Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 300 0)") };


            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            if (createCrossSections)
            {
                AddCrossSections(branch1, branch2);
            }
            return network;
        }

        public static void AddCrossSections(Channel branch1, Channel branch2)
        {
            CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch1, 20.0, new List<Coordinate>
                                                                                      {
                                                                                          new Coordinate(0.0, 18.0),
                                                                                          new Coordinate(100.0, 18.0),
                                                                                          new Coordinate(150.0, 10.0),
                                                                                          new Coordinate(300.0, 10.0),
                                                                                          new Coordinate(350.0, 18.0),
                                                                                          new Coordinate(500.0, 18.0)
                                                                                      });
            CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch1, 80.0, new List<Coordinate>
                                                                                      {
                                                                                          new Coordinate(0.0, 19.0),
                                                                                          new Coordinate(100.0, 19.0),
                                                                                          new Coordinate(150.0, 9.0),
                                                                                          new Coordinate(300.0, 9.0),
                                                                                          new Coordinate(350.0, 19.0),
                                                                                          new Coordinate(500.0, 19.0)
                                                                                      });

            CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch2, 50, new List<Coordinate>
                                                                                    {
                                                                                        new Coordinate(0.0, 14.0),
                                                                                        new Coordinate(100.0, 14.0),
                                                                                        new Coordinate(150.0, 8.0),
                                                                                        new Coordinate(300.0, 8.0),
                                                                                        new Coordinate(350.0, 14.0),
                                                                                        new Coordinate(500.0, 14.0)
                                                                                    });
        }

        public static CompositeBranchStructure AddCompositeBranchStructureForStructureAtLocation(IStructure1D bridge, NetworkLocation location)
        {
            var compositeBranchStructure = new CompositeBranchStructure();
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, location.Branch, location.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, bridge);
            return compositeBranchStructure;
        }

        public static Bridge GetBridge()
        {
            var bridge = new Bridge("bridge");
            bridge.TabulatedCrossSectionDefinition.SetWithHfswData(new[]
                                                             {
                                                                 new HeightFlowStorageWidth(10, 50, 50),
                                                                 new HeightFlowStorageWidth(16, 100, 100)
                                                             });
            bridge.OffsetY = 100;
            bridge.Shift = 10;
            bridge.Width = 50;
            bridge.Height = 8;
            return bridge;
        }

        public static void ShowStructureAtFirstBranch(IStructure1D structure, HydroNetwork network, Action<Form> action)
        {
            var compositeBranchStructure = new CompositeBranchStructure();

            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);


            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, structure);
            ShowStructure(compositeBranchStructure,action);
        }
        public static void ShowStructureAtFirstBranch(IStructure1D structure, HydroNetwork network)
        {
            ShowStructureAtFirstBranch(structure,network,null);
        }
        
        public static void ShowStructure(ICompositeBranchStructure compositeBranchStructure,Action<Form> action)
        {
            var mocks = new MockRepository();
            var dockingManager = mocks.Stub<IDockingManager>();

            mocks.ReplayAll();

            var presenter = new CompositeStructureViewPresenter
            {
                CreateView = o =>
                {
                    using (var plugin = new NetworkEditorGuiPlugin())
                    {
                        var viewList = new ViewList(dockingManager, ViewLocation.Document);
                        var viewResolver = new ViewResolver(viewList, plugin.GetViewInfoObjects());
                        return viewResolver.CreateViewForData(o, info =>
                                info.CompositeViewType == typeof (Gui.Forms.CompositeStructureView.CompositeStructureView));
                    }
                },
                SelectionContainer = new SimpleSelectionContainer { Logging = true }
            };
            var view = new Gui.Forms.CompositeStructureView.CompositeStructureView
            {
                Presenter = presenter,
                Data = compositeBranchStructure
            };

            if (action != null)
            {
                WindowsFormsTestHelper.ShowModal(view,action);    
            }
            else
            {
                WindowsFormsTestHelper.ShowModal(view);    
            }
        }

        public static void ShowStructure(ICompositeBranchStructure compositeBranchStructure)
        {
            ShowStructure(compositeBranchStructure,null);
        }
    }
}