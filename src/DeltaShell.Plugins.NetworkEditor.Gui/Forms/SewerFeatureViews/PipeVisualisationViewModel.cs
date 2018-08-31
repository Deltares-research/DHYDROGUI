using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class PipeVisualisationViewModel
    {
        public Pipe Pipe
        {
            get { return pipe; }
            set
            {
                if (pipe != null)
                {
                    pipe.PropertyChanged -= PipeOnPropertyChanged;
                }

                pipe = value;

                if (pipe != null)
                {
                    pipe.PropertyChanged += PipeOnPropertyChanged;
                }

                if (pipe == null) return;
                Update();
            }
        }

        private void PipeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TypeUtils.GetMemberName<SewerConnection>(p => p.LevelSource) || e.PropertyName == TypeUtils.GetMemberName<SewerConnection>(s => s.LevelTarget))
            {
                Update();
            }
        }

        private Point sourceA;
        private Point sourceB;
        private Point sourceC;
        private Point sourceD;
        private Point sourceE;
        private Point sourceF;

        private Point targetA;
        private Point targetB;
        private Point targetC;
        private Point targetD;
        private Point targetE;
        private Point targetF;

        private double minX;
        private double maxX;
        private double minY;
        private double maxY;
        private double widthMargin;
        private double heightMargin;
        private Pipe pipe;

        public void Update()
        {
            if (pipe == null) return;

            DetermineRanges();
            CreatePointCollections();
            UpdateTextPositions();
        }

        private void DetermineRanges()
        {
            var dx = pipe.Length;
            var pipeDiameter = pipe.CrossSectionDefinition?.HighestPoint - pipe.CrossSectionDefinition?.LowestPoint ?? 0.1 * dx;

            var x0 = 0;
            var xL = x0 + dx;

            var y0 = Pipe.LevelSource;
            var yL = Pipe.LevelTarget;

            var heightLineLength = 0.25 * dx;

            var sourceLeft = x0 - heightLineLength;
            var sourceRight = x0 + heightLineLength;

            var targetLeft = xL - heightLineLength;
            var targetRight = xL + heightLineLength;

            var sourceBottomLevel = Pipe.SourceCompartment.BottomLevel;
            var sourceSurfaceLevel = Pipe.SourceCompartment.SurfaceLevel;
            var targetBottomLevel = Pipe.TargetCompartment.BottomLevel;
            var targetSurfaceLevel = Pipe.TargetCompartment.SurfaceLevel;

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

            sourceA = NewScaledPoint(sourceLeft, sourceBottomLevel);
            sourceB = NewScaledPoint(x0, sourceBottomLevel);
            sourceC = NewScaledPoint(x0, y0);
            sourceD = NewScaledPoint(x0, y0 + pipeDiameter);
            sourceE = NewScaledPoint(x0, sourceSurfaceLevel);
            sourceF = NewScaledPoint(sourceRight, sourceSurfaceLevel);

            targetA = NewScaledPoint(targetRight, targetBottomLevel);
            targetB = NewScaledPoint(xL, targetBottomLevel);
            targetC = NewScaledPoint(xL, yL);
            targetD = NewScaledPoint(xL, yL + pipeDiameter);
            targetE = NewScaledPoint(xL, targetSurfaceLevel);
            targetF = NewScaledPoint(targetLeft, targetSurfaceLevel);
        }

        private void UpdateTextPositions()
        {
            var canvas = DrawingCanvas?.Invoke();
            if (canvas == null) return;

            var textBlocks = canvas.Children.OfType<TextBlock>().ToList();
            var textBlockSourceBottom = textBlocks.FirstOrDefault(t => t.Name == "SourceBottom");
            if (textBlockSourceBottom == null) return;
            Canvas.SetLeft(textBlockSourceBottom, sourceA.X);
            Canvas.SetTop(textBlockSourceBottom, sourceA.Y - textBlockSourceBottom.ActualHeight);

            var sourceSurface = textBlocks.FirstOrDefault(t => t.Name == "SourceSurface");
            Canvas.SetLeft(sourceSurface, sourceF.X - sourceSurface.ActualWidth);
            Canvas.SetTop(sourceSurface, sourceF.Y);

            var targetBottom = textBlocks.FirstOrDefault(t => t.Name == "TargetBottom");
            Canvas.SetLeft(targetBottom, targetA.X - targetBottom.ActualWidth);
            Canvas.SetTop(targetBottom, targetA.Y - targetBottom.ActualHeight);

            var targetSurface = textBlocks.FirstOrDefault(t => t.Name == "TargetSurface");
            Canvas.SetLeft(targetSurface, targetF.X);
            Canvas.SetTop(targetSurface, targetF.Y);
        }


        private Point NewScaledPoint(double x, double y)
        {
            return new Point(ScaleX(x), ScaleY(y));
        }

        private void CreatePointCollections()
        {
            PipeBottomPoints = new PointCollection { sourceC, targetC };
            PipeTopPoints = new PointCollection { sourceD, targetD };

            SourceBottomLevelPoints = new PointCollection { sourceA, sourceB, sourceC };
            SourceSurfaceLevelPoints = new PointCollection { sourceD, sourceE, sourceF };

            TargetBottomLevelPoints = new PointCollection { targetA, targetB, targetC };
            TargetSurfaceLevelPoints = new PointCollection { targetD, targetE, targetF };
        }

        public PointCollection TargetSurfaceLevelPoints { get; set; }
        public PointCollection TargetBottomLevelPoints { get; set; }
        public PointCollection SourceSurfaceLevelPoints { get; set; }
        public PointCollection SourceBottomLevelPoints { get; set; }
        public PointCollection PipeTopPoints { get; set; }
        public PointCollection PipeBottomPoints { get; set; }

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