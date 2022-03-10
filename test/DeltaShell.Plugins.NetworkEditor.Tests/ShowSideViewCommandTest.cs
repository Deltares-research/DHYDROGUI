using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ShowSideViewCommandTest
    {
        private static readonly MockRepository mocks = new MockRepository();

        [Test]
        public void ExecuteShowOpenViewForCorrectData()
        {
            //test the command issues a command to the commandhandler to open a view with the correct coverages..
            //quite a lot of mocking but important to get this fragile class under test.
            
            //mock the needs
            var pluginGui = mocks.DynamicMock<GuiPlugin>();
            var gui = mocks.DynamicMock<IGui>();
            var documentViews = mocks.DynamicMock<IViewList>();
            using(var mapView = new MapView())
            {
                var hydroNetworkEditorMapTool = mocks.DynamicMock<IHydroNetworkEditorMapTool>();
                var application = mocks.DynamicMock<IApplication>();
                var project = new Project();//project is pretty lightweight don't need to mock here
                var sideViewDataBuilder = new MockSideViewDataBuilder();
                var guiCommandHandler = mocks.DynamicMock<IGuiCommandHandler>();
            
                //create a route
                IHydroNetwork snakeHydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
                var route = RouteHelper.CreateRoute(new NetworkLocation(snakeHydroNetwork.Branches[0], 0),
                                        new NetworkLocation(snakeHydroNetwork.Branches[0], 10));
                var networkCoverageGroupLayer = new NetworkCoverageGroupLayer { Coverage = route };
            
                var testModelWithHydroNetwork = new TestModelWithHydroNetwork();
                testModelWithHydroNetwork.HydroNetwork = (HydroNetwork) route.Network;
            
                var addedCoverage = new NetworkCoverage{Network = route.Network};
            
                //setup expectations
                Expect.Call(hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer).Return(networkCoverageGroupLayer);
                var activeView = mocks.DynamicMock<ICompositeView>();
                Expect.Call(activeView.ChildViews).Return(new EventedList<IView>() { mapView }).Repeat.Any();
                Expect.Call(documentViews.ActiveView).Return(activeView);

                Expect.Call(gui.DocumentViews).Return(documentViews).Repeat.Any();
                Expect.Call(pluginGui.Gui).Return(gui).Repeat.Any();
                Expect.Call(gui.Application).Return(application).Repeat.Any();
                Expect.Call(application.Project).Return(project).Repeat.Any();
                Expect.Call(gui.CommandHandler).Return(guiCommandHandler).Repeat.Any();
                Expect.Call(hydroNetworkEditorMapTool.MapControl).SetPropertyAndIgnoreArgument().Repeat.Any();
                Expect.Call(()=>guiCommandHandler.OpenView(null,null)).IgnoreArguments();
                //this call is the proof the command found everything.. :) ..Rhino just doesn't get it again...:(
                //Expect.Call(() => sideViewDataBuilder.GetSideViewData(route, testModelWithHydroNetwork.WaterLevel, new[] {addedCoverage},new INetworkCoverage[0]));
                sideViewDataBuilder.GetSideViewDataFunction = (theRoute, waterLevel, allCoverages, allFeatureCoverages, renderedCoverages) =>
                                                                  {
                                                                      Assert.AreEqual(route, theRoute);
                                                                      Assert.AreEqual(testModelWithHydroNetwork.WaterLevel,
                                                                                      waterLevel);
                                                                      Assert.AreEqual(new[] {addedCoverage}, allCoverages.ToArray());
                                                                      Assert.AreEqual(0, renderedCoverages.Count());
                                                                      return null;//return something..
                                                                  };


                //get in on..
                mocks.ReplayAll();
                var command = new ShowSideViewCommand
                {
                    Gui = pluginGui.Gui,
                    //SideViewDataBuilder = sideViewDataBuilder
                };
            
            
                mapView.MapControl.Tools.Add(hydroNetworkEditorMapTool);
                project.RootFolder.Add(testModelWithHydroNetwork);
            
                //this one should be added to allcoverages of the sideviewdata.
                project.RootFolder.Add(addedCoverage);
                //go!
                command.Execute();

                //verify we got the calls
                mocks.VerifyAll();
            }
        }

        [Test]
        public void SideViewIsOpenedWithCoveragesInTheMapView()
        {
            //mock the needs
            var pluginGui = mocks.DynamicMock<GuiPlugin>();
            var gui = mocks.DynamicMock<IGui>();
            var documentViews = mocks.DynamicMock<IViewList>();
            using (var mapView = new MapView())
            {
                var hydroNetworkEditorMapTool = mocks.DynamicMock<IHydroNetworkEditorMapTool>();
                var application = mocks.DynamicMock<IApplication>();
                var project = new Project(); //project is pretty lightweight don't need to mock here
                var sideViewDataBuilder = new MockSideViewDataBuilder();

                var guiCommandHandler = mocks.DynamicMock<IGuiCommandHandler>();

                //create a route
                IHydroNetwork snakeHydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
                var route = RouteHelper.CreateRoute(
                    new NetworkLocation(snakeHydroNetwork.Branches[0], 0),
                    new NetworkLocation(snakeHydroNetwork.Branches[0], 10));
                var networkCoverageGroupLayer = new NetworkCoverageGroupLayer { Coverage = route };

                var testModelWithHydroNetwork = new TestModelWithHydroNetwork();
                testModelWithHydroNetwork.HydroNetwork = (HydroNetwork)route.Network;

                var addedCoverage = new NetworkCoverage { Network = route.Network, Name = "addedCoverage" };
                var coverageInMapView = new NetworkCoverage { Network = route.Network, Name = "coverageInMapView" };
                //setup expectations
                Expect.Call(hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer).Return(networkCoverageGroupLayer);
                var activeView = mocks.DynamicMock<ICompositeView>();
                Expect.Call(activeView.ChildViews).Return(new EventedList<IView>() { mapView }).Repeat.Any();
                Expect.Call(documentViews.ActiveView).Return(activeView);

                Expect.Call(gui.DocumentViews).Return(documentViews).Repeat.Any();
                Expect.Call(pluginGui.Gui).Return(gui).Repeat.Any();
                Expect.Call(gui.Application).Return(application).Repeat.Any();
                Expect.Call(application.Project).Return(project).Repeat.Any();
                Expect.Call(gui.CommandHandler).Return(guiCommandHandler).Repeat.Any();
                Expect.Call(hydroNetworkEditorMapTool.MapControl).SetPropertyAndIgnoreArgument().Repeat.Any();
                Expect.Call(() => guiCommandHandler.OpenView(null, null)).IgnoreArguments();
                //this call is the proof the command found everything.. :)..can't get rhino to test it right :(
                //Expect.Call(() => sideViewDataBuilder.GetSideViewData(route, testModelWithHydroNetwork.WaterLevel, new[] { addedCoverage,coverageInMapView }, new[] { coverageInMapView }));
                sideViewDataBuilder.GetSideViewDataFunction =
                    (theRoute, waterLevel, allCoverages, allFeatureCoverages, renderedCoverages) =>
                        {
                            Assert.AreEqual(route, theRoute);
                            Assert.AreEqual(testModelWithHydroNetwork.WaterLevel, waterLevel);
                            Assert.AreEqual(new[] { addedCoverage, coverageInMapView }, allCoverages.ToArray());
                            Assert.AreEqual(new[] { coverageInMapView }, renderedCoverages.ToArray());
                            //just return a dummy for now
                            return null;
                        };

                //get in on..
                mocks.ReplayAll();
                var command = new ShowSideViewCommand
                    {
                        Gui = pluginGui.Gui,
                        //SideViewDataBuilder = sideViewDataBuilder
                    };


                mapView.MapControl.Tools.Add(hydroNetworkEditorMapTool);
                project.RootFolder.Add(testModelWithHydroNetwork);

                //add a group layer

                var coverageGroupLayer = new NetworkCoverageGroupLayer { NetworkCoverage = coverageInMapView };
                mapView.Map.Layers.Add(coverageGroupLayer);

                //this one should be added to allcoverages of the sideviewdata.
                project.RootFolder.Add(addedCoverage);
                project.RootFolder.Add(coverageInMapView);
                //go!
                command.Execute();

                //verify we got the calls
                mocks.VerifyAll();
            }
        }
    }

    //need this hand-rolled te check calls to sideviewdatabuilder ...can't get rhino to the arguments correctly.
    public delegate TResult Func5<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public class MockSideViewDataBuilder
    {
        //public NetworkSideViewData GetSideViewData(INetworkCoverage route, ICoverage waterLevel, IEnumerable<INetworkCoverage> allCoverages, IEnumerable<INetworkCoverage> renderedCoverages)
        //{
        //    return GetSideViewDataFunction(route, waterLevel, allCoverages, renderedCoverages);
        //}

        public
            Func5
                <INetworkCoverage, ICoverage, IEnumerable<INetworkCoverage>, IEnumerable<IFeatureCoverage>, IEnumerable<INetworkCoverage>,
                    NetworkSideViewDataController> GetSideViewDataFunction;
    }
}