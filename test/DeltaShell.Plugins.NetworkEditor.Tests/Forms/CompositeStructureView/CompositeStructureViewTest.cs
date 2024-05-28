using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Gui.Forms.ViewManager;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CompositeStructureView
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class CompositeStructureViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowCompositeViewWithPumpAndWeir()
        {
            var mocks = new MockRepository();
            var dockingManager = mocks.Stub<IDockingManager>();
            mocks.ReplayAll();

            var network = CompositeStructureViewTestHelper.CreateDummyNetwork();
            var compositeBranchStructure = new CompositeBranchStructure();
            var pump = new Pump("pump1") {OffsetY = 1000,StopDelivery = 18,StartDelivery = 12,StopSuction = 12,StartSuction = 15};
            var weir = new Weir("weri1"){CrestLevel = 15,CrestWidth = 50};
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);


            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);
            var presenter = new CompositeStructureViewPresenter
                {
                    CreateView = o =>
                        {
                            var plugin = new NetworkEditorGuiPlugin();
                            var viewList = new ViewList(dockingManager, ViewLocation.Document);
                            var viewResolver = new ViewResolver(viewList, plugin.GetViewInfoObjects());

                            return viewResolver.CreateViewForData(o, info => info.CompositeViewType == typeof(Gui.Forms.CompositeStructureView.CompositeStructureView));
                        },
                    SelectionContainer = new SimpleSelectionContainer {Logging = true}
                };
            var view =  new Gui.Forms.CompositeStructureView.CompositeStructureView
                            {
                                Presenter = presenter,
                                Data = compositeBranchStructure
                            };

            WindowsFormsTestHelper.ShowModal(view);
        }
       
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowBridge()
        {
            var network = CompositeStructureViewTestHelper.CreateDummyNetwork();
            var bridge = CompositeStructureViewTestHelper.GetBridge();

            CompositeStructureViewTestHelper.ShowStructureAtFirstBranch(bridge, network);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowCulvert()
        {
            var network = CompositeStructureViewTestHelper.CreateDummyNetwork();
            
            var culvert = new Culvert("culvert");
            //bridge.OffsetY = 100;
            CompositeStructureViewTestHelper.ShowStructureAtFirstBranch(culvert, network);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTabulatedGatedCulvertWithEmptyGeometryShouldNotThrow_TOOLS10076()
        {
            var network = CompositeStructureViewTestHelper.CreateDummyNetwork();

            var culvert = new Culvert("culvert")
                {
                    GeometryType = CulvertGeometryType.Tabulated,
                    IsGated = true,
                };

            Action<Form> action = delegate
                {
                    culvert.Name = "Banaan";
                };

            CompositeStructureViewTestHelper.ShowStructureAtFirstBranch(culvert, network, action);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowBridgeWithDisabledStructesNearby()
        {
            var network = CompositeStructureViewTestHelper.CreateDummyNetwork();
            
            var bridge = CompositeStructureViewTestHelper.GetBridge();
            bridge.Length = 10;

            CompositeBranchStructure compositeBranchStructure = CompositeStructureViewTestHelper.AddCompositeBranchStructureForStructureAtLocation(bridge,new NetworkLocation(network.Branches[0],50));

            var otherBridge = CompositeStructureViewTestHelper.GetBridge();
            otherBridge.Length = 10;

            CompositeStructureViewTestHelper.AddCompositeBranchStructureForStructureAtLocation(otherBridge, new NetworkLocation(network.Branches[0], 70));

            var pump = new Pump("pump1") {OffsetY = 1000,StopDelivery = 18,StartDelivery = 12,StopSuction = 12,StartSuction = 15};
            CompositeStructureViewTestHelper.AddCompositeBranchStructureForStructureAtLocation(pump, new NetworkLocation(network.Branches[0], 40));

            var weir = new Weir("weri1") { CrestLevel = 15, CrestWidth = 50 };
            CompositeStructureViewTestHelper.AddCompositeBranchStructureForStructureAtLocation(weir, new NetworkLocation(network.Branches[0], 30));
            CompositeStructureViewTestHelper.ShowStructure(compositeBranchStructure);
        }


        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowBridgeWithoutCrossSection()
        {
            var network = CompositeStructureViewTestHelper.CreateDummyNetwork(false);
            Bridge bridge = CompositeStructureViewTestHelper.GetBridge();
            

            CompositeStructureViewTestHelper.ShowStructureAtFirstBranch(bridge, network);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowSimpleWeir()
        {
            var network = CompositeStructureViewTestHelper.CreateDummyNetwork();
            var weir = new Weir("simpleWeir")
                           {
                               CrestLevel = 3,
                               CrestWidth = 50,
                               WeirFormula = new SimpleWeirFormula()
                           };
            CompositeStructureViewTestHelper.ShowStructureAtFirstBranch(weir, network);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowGatedWeir()
        {
            var network = CompositeStructureViewTestHelper.CreateDummyNetwork();
            var weir = new Weir("gatedWeir")
                           {
                               CrestLevel = 3,
                               CrestWidth = 50,
                               WeirFormula = new GatedWeirFormula {GateOpening = 1.3}
                           };
            CompositeStructureViewTestHelper.ShowStructureAtFirstBranch(weir,network);
        }
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowCompositeViewWithWeirAndOnlyOneCrossSection()
        {
            var mocks = new MockRepository();
            var dockingManager = mocks.Stub<IDockingManager>();
            mocks.ReplayAll();

            var network = CompositeStructureViewTestHelper.CreateDummyNetwork();
            var fisrtCrossection = network.CrossSections.First();
            network.Branches[0].BranchFeatures.Remove(fisrtCrossection);
            var compositeBranchStructure = new CompositeBranchStructure();
            var pump = new Pump("pump1") {OffsetY = 150,StopDelivery = 18,StartDelivery = 12,StopSuction = 12,StartSuction = 15};
            var weir = new Weir("weri1"){CrestLevel = 15,CrestWidth = 50};
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);

            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);
            var presenter = new CompositeStructureViewPresenter
                                {
                                    CreateView = o =>
                                    {
                                        var plugin = new NetworkEditorGuiPlugin();
                                        var viewList = new ViewList(dockingManager, ViewLocation.Document);
                                        var viewResolver = new ViewResolver(viewList, plugin.GetViewInfoObjects());
                                        return viewResolver.CreateViewForData(o, info => info.CompositeViewType == typeof(Gui.Forms.CompositeStructureView.CompositeStructureView));
                                    },
                                    SelectionContainer = new SimpleSelectionContainer {Logging = true}
                                };
            var view =  new Gui.Forms.CompositeStructureView.CompositeStructureView
                            {
                                Presenter = presenter,
                                Data = compositeBranchStructure
                            };

            WindowsFormsTestHelper.ShowModal(view);
            
        }
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ChildViewsAreStructureViews()
        {
            var mocks = new MockRepository();
            var dockingManager = mocks.Stub<IDockingManager>();
            mocks.ReplayAll();

            var network = CompositeStructureViewTestHelper.CreateDummyNetwork();
            var compositeBranchStructure = new CompositeBranchStructure();
            var pump = new Pump("pump1") { OffsetY = 1000, StopDelivery = 18, StartDelivery = 12, StopSuction = 12, StartSuction = 15 };
            var weir = new Weir("weri1") { CrestLevel = 15, CrestWidth = 50 };
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);


            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);
            var presenter = new CompositeStructureViewPresenter
                {
                    CreateView = o =>
                        {
                            var plugin = new NetworkEditorGuiPlugin();
                            var viewList = new ViewList(dockingManager, ViewLocation.Document);
                            var viewResolver = new ViewResolver(viewList, plugin.GetViewInfoObjects());
                            return viewResolver.CreateViewForData(o, info => info.CompositeViewType == typeof(Gui.Forms.CompositeStructureView.CompositeStructureView));
                        },
                    SelectionContainer = new SimpleSelectionContainer {Logging = true}
                };
            var view = new Gui.Forms.CompositeStructureView.CompositeStructureView
                {
                    Presenter = presenter,
                    Data = compositeBranchStructure
                };
            presenter.SetModelIntoView();
            WindowsFormsTestHelper.Show(view);

            Assert.IsTrue(view.ChildViews.First() is PumpView);
            Assert.IsTrue(view.ChildViews.ElementAt(1) is WeirView);

            WindowsFormsTestHelper.CloseAll();
        }
        [Test]        
        public void ShowCompositeStructureViewWithoutCrossSection()
        {
            var network = CompositeStructureViewTestHelper.CreateDummyNetwork(false);
            
            var compositeBranchStructure = new CompositeBranchStructure();
            var pump = new Pump("pump1"){OffsetY = 150,StopDelivery = 15,StartDelivery = 10};
            var weir = new Weir("Weir1"){CrestLevel = 15};

            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, network.Branches[0], 50);
            
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);

            // setup mocks
            var mocks = new MockRepository();

            var documentViews = mocks.Stub<IViewList>();
            var dockingManager = mocks.Stub<IDockingManager>();
            var gui = mocks.Stub<IGui>();

            gui.Expect(g => g.DocumentViews).Return(documentViews);
            gui.Expect(g => g.SelectedModel).Return(null).Repeat.Any();
            gui.Expect(g => g.Plugins).Return(null).Repeat.Any();

            // setup event
            gui.SelectionChanged += null;
            LastCall.IgnoreArguments().Repeat.Any();
            
            // construct everything
            var compositeStructureViewPresenter = new CompositeStructureViewPresenter
                {
                    CreateView = o =>
                    {
                        var plugin = new NetworkEditorGuiPlugin();
                        var viewList = new ViewList(dockingManager, ViewLocation.Document);
                        var viewResolver = new ViewResolver(viewList, plugin.GetViewInfoObjects());
                        return viewResolver.CreateViewForData(o, info => info.CompositeViewType == typeof(Gui.Forms.CompositeStructureView.CompositeStructureView));
                    }
                };
            gui.SelectionChanged += delegate
                                        {
                                            compositeStructureViewPresenter.SelectObjectInViews((IStructure1D) gui.Selection);
                                        };
            
            mocks.ReplayAll();

            var compositeStructureView = new Gui.Forms.CompositeStructureView.CompositeStructureView
                                             {
                                                 Presenter = compositeStructureViewPresenter,
                                                 Data = compositeBranchStructure
                                             };
            compositeStructureViewPresenter.View = compositeStructureView;
            
            WindowsFormsTestHelper.ShowModal(compositeStructureView);
        }
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void AddWeirWhileShowingComposite()
        {
            var network = CompositeStructureViewTestHelper.CreateDummyNetwork();
            var weir = new Weir("gatedWeir")
            {
                CrestLevel = 3,
                CrestWidth = 50,
                WeirFormula = new GatedWeirFormula { GateOpening = 1.3 }
            };
            Action<Form> onShown = (form) =>
                                       {
                                           //set network in editing state just like in the app
                                           network.BeginEdit("go!");
                                           var composite = weir.ParentStructure;
                                           var weir2 = new Weir();
                                           HydroNetworkHelper.AddStructureToComposite(composite, weir2);
                                           network.EndEdit();
                                       };
            CompositeStructureViewTestHelper.ShowStructureAtFirstBranch(weir, network,onShown);
        }

        [Test]
        public void UpdateMinMaxForBranchFeaturesShouldNotReturnNaN()
        {
            // test shaky configuration
            var minValue = double.NaN;
            var maxValue = double.NaN;

            var weir = new Weir();
            var pump = new Pump();
            var bridge = new Bridge();
            bridge.TabulatedCrossSectionDefinition = new CrossSectionDefinitionZW();
            bridge.BridgeType = BridgeType.Tabulated;
            var culvert = new Culvert();
            var crossSection =
                CrossSectionHelper.CreateNewCrossSectionXYZ(new List<Coordinate>(
                new[]{
                    new Coordinate(-1.11d, -1.11d, -1.11d),
                    new Coordinate(3.33d, 3.33d, 3.33d)
                }));

            weir.CrestLevel = double.NaN;
            pump.StartDelivery = double.NaN;
            bridge.TabulatedCrossSectionDefinition.ZWDataTable.Select(v => v.Z = double.NaN);
            bridge.YZCrossSectionDefinition.YZDataTable.Select(v => v.Z = double.NaN);
            culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable.Select(v => v.Z = double.NaN);
            crossSection.Definition.GetProfile().ForEach(c => c.Y = double.NaN);

            CompositeStructureViewHelper.UpdateMinMaxForBranchFeatures(new IBranchFeature[]
            {
                new CompositeBranchStructure { Structures = new EventedList<IStructure1D>(new [] {weir}) }, 
                new CompositeBranchStructure { Structures = new EventedList<IStructure1D>(new [] {pump}) },
                new CompositeBranchStructure { Structures = new EventedList<IStructure1D>(new [] {bridge}) },
                new CompositeBranchStructure { Structures = new EventedList<IStructure1D>(new [] {culvert}) },
                crossSection
            }, ref minValue, ref maxValue);

            Assert.AreEqual(-1.11d, minValue);
            Assert.AreEqual(3.33d, maxValue);

            minValue = maxValue = double.NaN;

            weir.CrestLevel = 4.56d;
            pump.StartDelivery = -5.67d;

            CompositeStructureViewHelper.UpdateMinMaxForBranchFeatures(new IBranchFeature[]
            {
                new CompositeBranchStructure { Structures = new EventedList<IStructure1D>(new [] {weir}) }, 
                new CompositeBranchStructure { Structures = new EventedList<IStructure1D>(new [] {pump}) },
                new CompositeBranchStructure { Structures = new EventedList<IStructure1D>(new [] {bridge}) },
                new CompositeBranchStructure { Structures = new EventedList<IStructure1D>(new [] {culvert}) },
                crossSection
            }, ref minValue, ref maxValue);

            Assert.AreEqual(-5.67d, minValue);
            Assert.AreEqual(4.56d, maxValue);
        }
    }
}
