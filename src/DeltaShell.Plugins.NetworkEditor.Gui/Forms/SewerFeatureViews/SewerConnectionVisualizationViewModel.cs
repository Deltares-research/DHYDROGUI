using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class SewerConnectionVisualizationViewModel
    {
        public ISewerConnection SewerConnection
        {
            get { return sewerConnection; }
            set
            {
                if (sewerConnection != null)
                {
                    sewerConnection.PropertyChanged -= PipeOnPropertyChanged;
                }

                sewerConnection = value;

                if (sewerConnection != null)
                {
                    sewerConnection.PropertyChanged += PipeOnPropertyChanged;
                }

                if (sewerConnection == null) return;
                Update();
            }
        }

        private void PipeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPipe.LevelSource) || e.PropertyName == nameof(IPipe.LevelTarget))
            {
                Update();
            }
        }

        private Point sourceBottomLeft;
        private Point sourceBottomRight;
        private Point sourceBottomPipe;
        private Point sourceTopPipe;
        private Point sourceTopRight;
        private Point sourceTopLeft;

        private Point targetBottomRight;
        private Point targetBottomLeft;
        private Point targetBottomPipe;
        private Point targetTopPipe;
        private Point targetTopLeft;
        private Point targetTopRight;

        private double minX;
        private double maxX;
        private double minY;
        private double maxY;
        private double widthMargin;
        private double heightMargin;
        private ISewerConnection sewerConnection;

        public void Update()
        {
            if (sewerConnection == null) return;

            DetermineRanges();
            CreatePointCollections();
            UpdateTextPositions();
        }

        private void DetermineRanges()
        {
            var dx = sewerConnection.Length;
            var pipeDiameter = sewerConnection.CrossSection?.Definition?.HighestPoint - sewerConnection.CrossSection?.Definition?.LowestPoint ?? 0.1 * dx;

            var x0 = 0;
            var xL = x0 + dx;

            var y0 = SewerConnection.LevelSource;
            var yL = SewerConnection.LevelTarget;

            var heightLineLength = 0.25 * dx;

            var sourceLeft = x0 - heightLineLength;
            
            var targetRight = xL + heightLineLength;

            var sourceBottomLevel = SewerConnection.SourceCompartment.BottomLevel;
            var sourceSurfaceLevel = SewerConnection.SourceCompartment.SurfaceLevel;
            var targetBottomLevel = SewerConnection.TargetCompartment?.BottomLevel ?? 0d;
            var targetSurfaceLevel = SewerConnection.TargetCompartment?.SurfaceLevel ?? 0d;

            widthMargin = 0.03 * (targetRight - sourceLeft);
            minX = sourceLeft - widthMargin;
            maxX = targetRight + widthMargin;

            var minBottomLevel = Math.Min(sourceBottomLevel, targetBottomLevel);
            var minPipeLevel = Math.Min(y0, yL);
            minY = Math.Min(minBottomLevel, minPipeLevel);
            var maxSurfaceLevel = Math.Max(sourceSurfaceLevel, targetSurfaceLevel);
            var maxPipeLevel = Math.Max(y0, yL);
            maxY = Math.Max(maxSurfaceLevel, maxPipeLevel);
            heightMargin = 0.1 * (maxY - minY);
            minY -= heightMargin;
            maxY += heightMargin;

            sourceBottomLeft = NewScaledPoint(sourceLeft, sourceBottomLevel);
            sourceBottomRight = NewScaledPoint(x0, sourceBottomLevel);
            sourceBottomPipe = NewScaledPoint(x0, y0);
            sourceTopPipe = NewScaledPoint(x0, y0 + pipeDiameter);
            sourceTopRight = NewScaledPoint(x0, sourceSurfaceLevel);
            sourceTopLeft = NewScaledPoint(sourceLeft, sourceSurfaceLevel);

            targetBottomRight = NewScaledPoint(targetRight, targetBottomLevel);
            targetBottomLeft = NewScaledPoint(xL, targetBottomLevel);
            targetBottomPipe = NewScaledPoint(xL, yL);
            targetTopPipe = NewScaledPoint(xL, yL + pipeDiameter);
            targetTopLeft = NewScaledPoint(xL, targetSurfaceLevel);
            targetTopRight = NewScaledPoint(targetRight, targetSurfaceLevel);
        }

        private void UpdateTextPositions()
        {
            var canvas = DrawingCanvas?.Invoke();
            if (canvas == null) return;

            var marginX = 2;
            var textBlocks = canvas.Children.OfType<TextBlock>().ToList();

            var textBlockSourceBottom = textBlocks.FirstOrDefault(t => t.Name == "SourceBottom");
            if (textBlockSourceBottom != null)
            {
                Canvas.SetLeft(textBlockSourceBottom, sourceBottomLeft.X + marginX);
                Canvas.SetTop(textBlockSourceBottom, sourceBottomLeft.Y - textBlockSourceBottom.ActualHeight);
            }
            var sourceSurface = textBlocks.FirstOrDefault(t => t.Name == "SourceSurface");
            if (sourceSurface != null)
            {
                Canvas.SetLeft(sourceSurface, sourceBottomLeft.X + marginX);
                Canvas.SetTop(sourceSurface, sourceTopLeft.Y);
            }
            
            var targetBottom = textBlocks.FirstOrDefault(t => t.Name == "TargetBottom");
            if (targetBottom != null)
            {
                Canvas.SetLeft(targetBottom, targetBottomRight.X - targetBottom.ActualWidth - marginX);
                Canvas.SetTop(targetBottom, targetBottomRight.Y - targetBottom.ActualHeight);
            }

            var targetSurface = textBlocks.FirstOrDefault(t => t.Name == "TargetSurface");
            if (targetSurface != null)
            {
                Canvas.SetLeft(targetSurface, targetTopRight.X - targetSurface.ActualWidth - marginX);
                Canvas.SetTop(targetSurface, targetTopRight.Y);
            }
        }

        private Point NewScaledPoint(double x, double y)
        {
            return new Point(ScaleX(x), ScaleY(y));
        }

        private void CreatePointCollections()
        {
            PipeBottomPoints = new PointCollection { sourceBottomPipe, targetBottomPipe };
            PipeTopPoints = new PointCollection { sourceTopPipe, targetTopPipe };
            PipePolygonPoints = new PointCollection { sourceBottomPipe, targetBottomPipe, targetTopPipe, sourceTopPipe };

            TopLevelPoints = new PointCollection{ sourceTopLeft, sourceTopRight, targetTopLeft, targetTopRight };

            SourceCompartmentPoints = new PointCollection { sourceBottomLeft, sourceBottomRight, sourceTopRight, sourceTopLeft };
            TargetCompartmentPoints = new PointCollection { targetBottomRight, targetBottomLeft, targetTopLeft, targetTopRight };
        }

        public PointCollection PipeTopPoints { get; set; }

        public PointCollection PipeBottomPoints { get; set; }

        public PointCollection PipePolygonPoints { get; set; }

        public PointCollection TopLevelPoints { get; set; }

        public PointCollection SourceCompartmentPoints { get; set; }

        public PointCollection TargetCompartmentPoints { get; set; }

        private double ScaleX(double x)
        {
            return GetActualWidth == null ? 0 : CoordinateScalingHelper.ScaleX(x, minX, maxX, GetActualWidth.Invoke());
        }

        private double ScaleY(double y)
        {
            return GetActualHeight == null ? 0 : CoordinateScalingHelper.ScaleY(y, minY, maxY, GetActualHeight.Invoke());
        }

        public Func<double> GetActualWidth { get; set; }
        public Func<double> GetActualHeight { get; set; }

        public Func<Canvas> DrawingCanvas { get; set; }
    }
}