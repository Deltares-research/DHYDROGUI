using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.MapTools
{
    /// <summary>
    /// This map tool attempts to find a grid cell for a <see cref="WaterQualityModel"/> grid
    /// given a cell index. It takes into account layering of the model, informing at which
    /// layer-index the grid cell is at.
    /// </summary>
    public class FindGridCellTool : MapTool
    {
        private readonly CoordinateTransformationCache transformationCache;
        private Polygon chosenCellPolygon;
        private int layerNumber;
        private int segmentIndex;

        /// <summary>
        /// Create a new instance of this maptool to find and highlight grid cells for a
        /// given grid ID.
        /// </summary>
        /// <param name="toolName">Name of the tool.</param>
        public FindGridCellTool(string toolName)
        {
            Name = toolName;
            LayerFilter = IsToolAllowedOnLayer;
            transformationCache = new CoordinateTransformationCache();
        }

        private bool IsToolAllowedOnLayer(ILayer layer)
        {
            var unstructuredGridLayer = layer as UnstructuredGridLayer;
            if (unstructuredGridLayer != null && GetWaqModelForGrid != null)
            {
                GridEdgeRenderer renderer = unstructuredGridLayer.Renderer as GridEdgeRenderer;
                bool isBlockedLinksRenderer = false;
                if (renderer != null)
                {
                    isBlockedLinksRenderer = renderer.GridEdgeRenderMode == GridEdgeRenderMode.EdgesWithBlockedFlowLinks;
                }

                return !isBlockedLinksRenderer && GetWaqModelForGrid(unstructuredGridLayer.Grid) != null;
            }
            return false;
        }

        /// <summary>
        /// Method injection point to retrieve a <see cref="WaterQualityModel"/> for a given
        /// <see cref="UnstructuredGrid"/>. It is required to have this method set before
        /// using the maptool.
        /// </summary>
        public Func<UnstructuredGrid, WaterQualityModel> GetWaqModelForGrid { get; set; }

        public override bool Enabled
        {
            get { return base.Enabled && Layers.Any(); }
        }

        public override bool IsActive
        {
            get { return base.IsActive; }
            set
            {
                if (base.IsActive && !value)
                {
                    ((Control)MapControl).Invalidate();
                }
                base.IsActive = value;

                if (IsActive)
                {
                    Debug.Assert(Layers.Count() <= 1, 
                        "Assumption: There is upto 1 UnstructuredGridLayer available, even if there are multiple WAQ models in the project.");

                    var firstGridLayer = Layers.FirstOrDefault() as UnstructuredGridLayer;
                    if (firstGridLayer == null) return;

                    var grid = firstGridLayer.Grid;
                    var model = GetWaqModelForGrid(grid);

                    var maximumGridCell = model.NumberOfWaqSegmentLayers * grid.Cells.Count;
                    var initialSelectedGridCell = (segmentIndex >= 1 && segmentIndex <= maximumGridCell) ? segmentIndex : 1;
                    
                    var askGridCellDialog = new FindGridCellDialog(maximumGridCell, initialSelectedGridCell)
                        {
                            StartPosition = FormStartPosition.CenterScreen
                        };

                    var dialogResult = askGridCellDialog.ShowDialog();
                    if (dialogResult == DialogResult.OK)
                    {
                        segmentIndex = askGridCellDialog.GridCellId;
                        FindGridCell(firstGridLayer, model.PointToGridCellMapper);
                    }
                    else
                    {
                        Cancel();
                    }
                }
            }
        }

        private void FindGridCell(UnstructuredGridLayer gridLayer, PointToGridCellMapper gridCellMapper)
        {
            var grid = gridLayer.Grid;

            Cell gridCell = gridCellMapper.GetCellFromWaqSegmentId(segmentIndex);

            chosenCellPolygon = gridCell.ToPolygon(grid);
            layerNumber = segmentIndex / grid.Cells.Count + 1; // + 1 because waq is one based with layer indices as well

            Polygon projectedCell = GetTransformedCellPolygon(gridLayer);

            var envelopeInternal = projectedCell.EnvelopeInternal.Clone();
            // add 10% margin:
            envelopeInternal.ExpandBy(
                projectedCell.EnvelopeInternal.Width * 0.1,
                projectedCell.EnvelopeInternal.Height * 0.1);

            Map.ZoomToFit(envelopeInternal);
        }

        public override void Cancel()
        {
            chosenCellPolygon = null;
            layerNumber = -1;

            MapControl.ActivateTool(MapControl.SelectTool);
            MapControl.SelectTool.RefreshSelection();

            base.Cancel();
        }

        public override void OnPaint(PaintEventArgs e) // Override to ensure drawing always happens, not only during drag/drop
        {
            base.OnPaint(e);
            Render(e.Graphics, MapControl.Map);
        }

        public override void Render(Graphics graphics, Map mapBox)
        {
            if (chosenCellPolygon == null)
                return;

            var firstGridLayer = (UnstructuredGridLayer)Layers.First();
            var projectedCell = GetTransformedCellPolygon(firstGridLayer);

            RenderHighlightedGridCell(graphics, projectedCell);
            RenderLayerIndexText(graphics, projectedCell);
        }

        private void RenderHighlightedGridCell(Graphics graphics, Polygon projectedCell)
        {
            using (var renderer = new WorldToScreenRendererWrapper(new GdiPrimitivesRenderer()))
            {
                renderer.BeginDraw(graphics, Map.Size, new RectangleF(
                    Convert.ToSingle(Map.WorldLeft),
                    Convert.ToSingle(Map.WorldTop - Map.WorldHeight),
                    Convert.ToSingle(Map.Zoom),
                    Convert.ToSingle(Map.WorldHeight)));
                renderer.SetFillColor(Color.Purple);

                renderer.FillPolygon(
                    projectedCell.Coordinates.Select(c => new PointF(Convert.ToSingle(c.X), Convert.ToSingle(c.Y))).ToArray());

                renderer.EndDraw();
            }
        }

        private void RenderLayerIndexText(Graphics graphics, Polygon projectedCell)
        {
            var textPoint = GetLocationToRenderText(projectedCell);

            var originalValue = graphics.TextRenderingHint;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.DrawString(string.Format("Cell: {0}; Layer index: {1}", segmentIndex, layerNumber), new Font("Arial", 10), Brushes.Black, textPoint);
            graphics.TextRenderingHint = originalValue;
        }

        private PointF GetLocationToRenderText(Polygon projectedCell)
        {
            PointF textPoint = Map.WorldToImage(projectedCell.Centroid.Coordinate);
            var bottomTextPoint = Map.WorldToImage(projectedCell.Coordinates.OrderBy(c => c.Y).First());
            var topPoint = Map.WorldToImage(projectedCell.Coordinates.OrderByDescending(c => c.Y).First());
            textPoint.Y = bottomTextPoint.Y + Math.Abs(bottomTextPoint.Y - topPoint.Y) * 0.05f;
                // Margin: 5% lower the dynamic height of the polygon
            return textPoint;
        }

        private Polygon GetTransformedCellPolygon(ILayer layer)
        {
            transformationCache.SetTransformation(layer.CoordinateTransformation);

            var projectedCell = (Polygon)chosenCellPolygon.Clone();
            var transformedCoordinates = projectedCell.Coordinates.Select(transformationCache.PerformProjection).ToArray();

            for (int i = 0; i < projectedCell.Coordinates.Length; i++)
            {
                projectedCell.Coordinates[i].X = transformedCoordinates[i].X;
                projectedCell.Coordinates[i].Y = transformedCoordinates[i].Y;
            }
            return projectedCell;
        }
    }
}