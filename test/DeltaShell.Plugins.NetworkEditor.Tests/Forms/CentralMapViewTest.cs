using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using GisSharpBlog.NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Api;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using Point = GisSharpBlog.NetTopologySuite.Geometries.Point;
using ShapeType = SharpMap.Styles.ShapeType;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms
{
    [TestFixture]
    public class CentralMapViewTest
    {
        [Test, Category(TestCategory.WindowsForms )]
        public void ShowModelView()
        {
            var mocks = new MockRepository();
            var model = mocks.StrictMock<IModel>();
            
            mocks.ReplayAll();
            
            var modelView = new CentralMapView
                                {
                                    CreateLayerForData = (o, di) => null,
                                    Data = model
                                };

            WindowsFormsTestHelper.ShowModal(modelView);
        }

        [Test, Category(TestCategory.WindowsForms)]
        public void ModelViewSynchronizesMapSelection()
        {
            var mocks = new MockRepository();
            
            var model = mocks.StrictMock<IModel>();

            var featureToSync = mocks.Stub<IFeature>();
            var featureProvider = mocks.Stub<IFeatureProvider>();

            var tabControl = mocks.Stub<MapViewTabControl>();
            var mapViewEditor = mocks.StrictMock<ILayerEditorView>();
            var layerEditorViews = new EventedList<IView> {mapViewEditor};

            featureToSync.Geometry = new Point(2,2);
            
            tabControl.Expect(tc => tc.Dispose());
            tabControl.Expect(tc => tc.ChildViews).Return(layerEditorViews).Repeat.Any();

            mapViewEditor.Expect(mve => mve.Data).Return("");

            featureProvider.Expect(fp => fp.Contains(featureToSync)).Return(true).Repeat.Any();
            featureProvider.Expect(fp => fp.GetFeature(0)).Return(featureToSync).Repeat.Any();
            featureProvider.Expect(fp => fp.GetFeatureCount()).Return(1).Repeat.Any();
            //featureProvider.Expect(fp => fp.GetFeatures(new Envelope())).IgnoreArguments().Return(new []{featureToSync}).Repeat.Any();
            featureProvider.Expect(fp => fp.GetExtents()).Return(new Envelope(0, 10, 0, 10)).Repeat.Any();
            
            featureProvider.FeaturesChanged += null;
            LastCall.IgnoreArguments().Repeat.Any();

            // expect call to mapViewEditor.SelectedFeatures after map selection
            mapViewEditor.Expect(me => me.SelectedFeatures).SetPropertyWithArgument(new []{featureToSync});

            mocks.ReplayAll();

            var modelView = new CentralMapView
                                {
                                    CreateLayerForData = (o, di) => new VectorLayer{ DataSource = featureProvider },
                                    Data = model
                                };

            // use mocked tab control to check synchronization
            TypeUtils.SetField(modelView.MapView,"tabControl", tabControl);

            // selecting feature will set the mapViewEditor.SelectedFeatures 
            modelView.MapView.MapControl.SelectTool.Select(new[] {featureToSync});

            WindowsFormsTestHelper.ShowModal(modelView);
        }

        [Test]
        public void CreateCentralMapViewContext()
        {
            var mocks = new MockRepository();
            var model = mocks.StrictMock<IModel>();
            var modelItem = "Item";

            mocks.ReplayAll();

            VectorLayer modelItemLayer = null;

            var centralMapView = new CentralMapView
                {
                    CreateLayerForData = (o, d) =>
                        {
                            var itemLayer = new VectorLayer();
                            var modelLayer = new GroupLayer
                                                {
                                                    Layers = new EventedList<ILayer>
                                                                {
                                                                    itemLayer
                                                                }
                                                };
                            d[modelLayer] = model;
                            d[itemLayer] = modelItem;

                            modelItemLayer = itemLayer;

                            return modelLayer;
                        },

                    Data = model
                };

            Assert.NotNull(modelItemLayer);

            modelItemLayer.Visible = true;
            modelItemLayer.Style.Shape = ShapeType.Triangle;
            modelItemLayer.Style.Line.Color = Color.Red;

            var vectorLayer = new VectorLayer("non generated layer");
            centralMapView.MapView.Map.Layers.Add(vectorLayer);
            
            var context = (CentralMapViewContext)centralMapView.ViewContext;

            centralMapView.Dispose();

            // Generated layer to be removed 
            Assert.AreEqual(vectorLayer.Name, context.Map.Layers[0].Name);
            Assert.AreEqual(0, context.DataLayerIndex);
            
            var generatedMapLayerInfoList = context.GeneratedMapLayerInfoList;
            Assert.AreEqual(2, generatedMapLayerInfoList.Count);
            Assert.AreEqual(ShapeType.Triangle, generatedMapLayerInfoList[1].VectorStyle.Shape);
            Assert.AreEqual(Color.Red.ToArgb(), generatedMapLayerInfoList[1].VectorStyle.Line.Color.ToArgb());
            Assert.AreEqual(1, generatedMapLayerInfoList[1].Level);
        }

        [Test]
        public void SetCentralMapViewContext()
        {
            var mocks = new MockRepository();
            var model = mocks.StrictMock<IModel>();
            var featureProvider1 = mocks.Stub<IFeatureProvider>();
            var featureProvider2 = mocks.Stub<IFeatureProvider>();
            var modelItem = "Item";

            Expect.Call(featureProvider1.Features).Return(new IFeature[] {}).Repeat.Any();
            Expect.Call(featureProvider2.Features).Return(new IFeature[] {}).Repeat.Any();

            mocks.ReplayAll();
                
            VectorLayer modelItemLayer = null;

            var centralMapView = new CentralMapView
            {
                CreateLayerForData = (o, d) =>
                {
                    var itemLayer = new VectorLayer { DataSource = featureProvider1 };
                    var modelLayer = new GroupLayer
                    {
                        Layers = new EventedList<ILayer>
                                {
                                    itemLayer
                                }
                    };
                    d[modelLayer] = model;
                    d[itemLayer] = modelItem;

                    modelItemLayer = itemLayer;

                    return modelLayer;
                },

                Data = model
            };

            Assert.NotNull(modelItemLayer);
            
            modelItemLayer.Visible = true;
            modelItemLayer.Style.Shape = ShapeType.Triangle;
            modelItemLayer.Style.Line.Color = Color.Red;

            var nonGeneratedLayer = new VectorLayer("non generated layer") { DataSource = featureProvider2 };
            centralMapView.MapView.Map.Layers.Add(nonGeneratedLayer);

            var context = (CentralMapViewContext)centralMapView.ViewContext;
            centralMapView.UpdateContext();

            modelItemLayer.Visible = false;
            modelItemLayer.Style.Shape = ShapeType.Ellipse;
            modelItemLayer.Style.Line.Color = Color.Blue;

            centralMapView.MapView.Map.Layers.Remove(nonGeneratedLayer);

            // reset changes using data and viewContext
            centralMapView.Data = model;
            centralMapView.ViewContext = context;
            
            Assert.AreEqual(2, centralMapView.MapView.Map.Layers.Count);

            Assert.IsTrue(modelItemLayer.Visible);
            Assert.AreEqual(ShapeType.Triangle, modelItemLayer.Style.Shape);
            Assert.AreEqual(Color.Red.ToArgb(), modelItemLayer.Style.Line.Color.ToArgb());

            Assert.AreEqual(nonGeneratedLayer.Name, centralMapView.MapView.Map.Layers[1].Name);
        }
    }
}
