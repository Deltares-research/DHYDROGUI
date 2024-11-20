using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools
{

    public class Add1D2DLinkMapTool : Base1D2DLinksMapTool
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Add1D2DLinkMapTool));

        private VectorLayer newArrowLineLayer;
        private ILineString newArrowLineGeometry;
        private Coordinate startCoordinate;
        private Coordinate endCoordinate;
        private int snapToleranceInPixels = 20;

        public Add1D2DLinkMapTool()
        {
            Name = FlowFMMapViewDecorator.AddLinksToolName;
        }

        protected Coordinate StartCoordinate
        {
            get
            {
                return this.startCoordinate;
            }
            set
            {
                this.startCoordinate = value;
                this.endCoordinate = this.startCoordinate;
                this.CreateNewLineGeometry();
            }
        }

        protected Coordinate EndCoordinate
        {
            get
            {
                return this.endCoordinate;
            }
            set
            {
                this.endCoordinate = value;
                this.CreateNewLineGeometry();
            }
        }

        public VectorLayer VectorLayer
        {
            get
            {
                return Layers.FirstOrDefault(l => l.Name.Equals(FlowFMLayerNames.Links1D2DLayerName)) as VectorLayer;
            }
        }

        public override void Render(Graphics graphics)
        {
            if ((Unique<long>)this.newArrowLineLayer != (Unique<long>)null && this.newArrowLineGeometry != null)
            {
                this.newArrowLineLayer.Render();
                graphics.DrawImage(this.newArrowLineLayer.Image, 0, 0);
            }
        }

        public override void StartDrawing()
        {
            base.StartDrawing();
            this.AddDrawingLayer();
        }

        public override void StopDrawing()
        {
            base.StopDrawing();
            this.RemoveDrawingLayer();
        }

        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            if ((Unique<long>)this.VectorLayer == (Unique<long>)null || e.Button != MouseButtons.Left)
                return;
            this.IsBusy = true;
            this.StartCoordinate = this.GetLocalCoordinate(new Coordinate(worldPosition.X, worldPosition.Y));
        }

        public override void OnMouseMove(Coordinate worldPosition, MouseEventArgs e)
        {
            if ((Unique<long>)this.VectorLayer == (Unique<long>)null || this.AdditionalButtonsBeingPressed(e))
                return;
            if (this.startCoordinate != null)
                this.EndCoordinate = this.GetLocalCoordinate(new Coordinate(worldPosition.X, worldPosition.Y));
            this.StartDrawing();
            this.DoDrawing(true);
            this.StopDrawing();
        }

        public override void OnMouseUp(Coordinate worldPosition, MouseEventArgs e)
        {
            if ((Unique<long>)this.VectorLayer == (Unique<long>)null || this.AdditionalButtonsBeingPressed(e))
                return;

            if (this.startCoordinate != null)
            {
                this.EndCoordinate = this.GetLocalCoordinate(new Coordinate(worldPosition.X, worldPosition.Y));
                var snapTolerance = GetSnapTolerance(e.Location, snapToleranceInPixels);
                Add1D2DLink(this.StartCoordinate, this.EndCoordinate, snapTolerance);
            }
            Cancel();
        }

        private Coordinate GetLocalCoordinate(Coordinate coordinate)
        {
            if (!((Unique<long>)this.VectorLayer != (Unique<long>)null) || this.VectorLayer.CoordinateTransformation == null)
                return coordinate;
            return GeometryTransform.TransformPoint((IPoint)new NetTopologySuite.Geometries.Point(coordinate), this.VectorLayer.CoordinateTransformation.MathTransform.Inverse()).Coordinate;
        }



        public override void Cancel()
        {
            this.RemoveDrawingLayer();
            this.newArrowLineGeometry = (ILineString)null;
            this.startCoordinate = (Coordinate)null;
            this.endCoordinate = (Coordinate)null;
            this.IsBusy = false;
            this.MapControl.SnapTool.Cancel();
        }

        private void AddDrawingLayer()
        {
            VectorLayer vectorLayer = new VectorLayer(this.VectorLayer);
            var theme = VectorLayer.Theme.Clone() as ITheme;
            int num = 1;
            vectorLayer.RenderRequired = num != 0;
            string str = "newArrowLine";
            vectorLayer.Name = str;
            IMap map = this.VectorLayer.Map;
            vectorLayer.Map = map;
            ICoordinateTransformation coordinateTransformation = this.VectorLayer.CoordinateTransformation;
            vectorLayer.CoordinateTransformation = coordinateTransformation;
            vectorLayer.DataSource = (IFeatureProvider)new DataTableFeatureProvider((IGeometry)this.newArrowLineGeometry)
            {
                CoordinateSystem = this.VectorLayer.CoordinateSystem
            };
            this.newArrowLineLayer = vectorLayer;
            if (theme != null)
            {
                var styleName = LinkType.GetDescription();
                var themeItem = theme.ThemeItems.FirstOrDefault(i => i.Label.Equals(styleName));
                var style = themeItem?.Style as VectorStyle;
                if (style != null)
                {
                    this.newArrowLineLayer.Style = style;
                }
            }
            this.newArrowLineLayer.Style.Line.DashStyle = DashStyle.Dot;

        }

        private void RemoveDrawingLayer()
        {
            this.newArrowLineLayer = (VectorLayer)null;
        }

        private void CreateNewLineGeometry()
        {
            this.newArrowLineGeometry = new LineString(new Coordinate[2]
            {
                this.startCoordinate,
                this.endCoordinate
            });
        }

        private void Add1D2DLink(Coordinate startPoint, Coordinate endPoint, double snapTolerance)
        {
            var fmLayer = Map.GetAllLayers(true).OfType<ModelGroupLayer>().FirstOrDefault(l => l.Model is WaterFlowFMModel);
            if (fmLayer == null)
            {
                log.Error("Can not find the FM layer to create the grid on.");
                return;
            }

            var fmModel = (WaterFlowFMModel)fmLayer.Model;
            //check conditions
            if (fmModel.Grid == null || !fmModel.Grid.Cells.Any())
            {
                log.Error("No 2D grid available for 1D2D links");
                return;
            }
            if (fmModel.NetworkDiscretization == null || !fmModel.NetworkDiscretization.Locations.AllValues.Any())
            {
                log.Error("No discretizazion/ 1D mesh available for 1D2D links");
                return;
            }
            MapTool1D2DLinksHelper.AddNew1D2DLink(fmModel, LinkType, startPoint, endPoint, snapTolerance);
        }

        private double GetSnapTolerance(System.Drawing.Point location, int pixelTolerance)
        {
            var result = 0.0;
            var start = MapHelper.ImageToWorld((IMap) this.MapControl.Map, location.X, location.Y);
            var end = MapHelper.ImageToWorld((IMap)this.MapControl.Map, location.X + pixelTolerance, location.Y + pixelTolerance);
            result = Math.Max(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
            return result;
        }
    }
}
