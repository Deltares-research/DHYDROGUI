using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DelftTools.Hydro.Structures;
using Point = System.Windows.Point;
using DelftTools.Controls.Wpf.Extensions;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for ManholeVisualisationControl.xaml
    /// </summary>
    public partial class ManholeVisualisation : UserControl
    {
        public static readonly DependencyProperty ManholeProperty = DependencyProperty.Register(
            nameof(Manhole),
            typeof(Manhole),
            typeof(ManholeVisualisation),
            new PropertyMetadata(default(ObservableCollection<Manhole>), PropertyChangedCallback));

        public ManholeVisualisation()
        {
            InitializeComponent();
            ViewModel.ContainerWidth = () => ViewGrid.ActualWidth;
            ViewModel.ContainerHeight = () => ViewGrid.ActualHeight;
            ViewModel.SetWindowSize = SetViewGridSize;
        }

        public Manhole Manhole
        {
            get { return (Manhole)GetValue(ManholeProperty); }
            set { SetValue(ManholeProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as ManholeVisualisation;
            if (view == null) return;

            view.ViewModel.Manhole = dependencyPropertyChangedEventArgs.NewValue as Manhole;
        }

        private void SetViewGridSize()
        {
            var ratio = ViewModel.HeigthWidthRatio;
            if (double.IsNaN(ratio)) return;

            if (UserControl.ActualHeight / ratio < UserControl.ActualWidth)
            {
                // Adjust height to available height, scale width by ratio
                var height = UserControl.ActualHeight;
                ViewGrid.Height = height;
                ViewGrid.Width = height / ratio;
                return;
            }

            // Adjust width to available width, scale height by ratio
            var width = UserControl.ActualWidth;
            ViewGrid.Width = width;
            ViewGrid.Height = width * ratio;

        }

        private void ViewGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetViewGridSize();
            ViewModel.SetShapesPixelValues();
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = sender as Canvas;
            if (canvas == null) return;

            originalElement = (e.Source as DependencyObject)?.TryFindParent<ContentPresenter>();
            if (originalElement == null) return;

            startPoint = e.GetPosition(canvas);

            isDown = true;
            canvas.CaptureMouse();
            e.Handled = true;
        }

        private void UIElement_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!isDown) return;

            var canvas = sender as Canvas;
            if (canvas == null) return;

            if (isDragging == false && (Math.Abs(e.GetPosition(canvas).X - startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                        Math.Abs(e.GetPosition(canvas).Y - startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                DragStarted();
            }
            if (isDragging)
            {
                DragMoved(canvas);
            }
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDown)
            {
                DragFinished(sender, false);
                e.Handled = true;
            }
        }

        private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && isDragging)
            {
                DragFinished(sender, true);
            }
        }

        private Point startPoint;
        private Point currentPosition;
        private double originalLeft;
        private double originalTop;
        private bool isDown;
        private bool isDragging;
        private UIElement originalElement;
        private SimpleAdorner overlayElement;

        private void DragStarted()
        {
            isDragging = true;
            originalLeft = Canvas.GetLeft(originalElement);
            originalTop = Canvas.GetTop(originalElement);

            overlayElement = new SimpleAdorner(originalElement);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(originalElement);
            layer.Add(overlayElement);
        }

        private void DragMoved(Canvas canvas)
        {
            currentPosition = Mouse.GetPosition(canvas);
            overlayElement.LeftOffset = currentPosition.X - startPoint.X;
            overlayElement.TopOffset = currentPosition.Y - startPoint.Y;

            // Validate?
        }

        private void DragFinished(object sender, bool cancelled)
        {
            Mouse.Capture(null);
            try
            {
                if (!isDragging || cancelled) return;

                var contentPresenter = originalElement as ContentPresenter;
                var canvas = sender as Canvas;
                if (contentPresenter == null || canvas == null) return;

                var cps = canvas.Children.OfType<ContentPresenter>().Where(cp => cp.Content is CompartmentShape || cp.Content is ConnectionShape);

                var originalPosition = originalLeft + 0.5 * contentPresenter.ActualWidth;
                var newPosition = overlayElement.LeftOffset + originalLeft + 0.5 * contentPresenter.ActualWidth;

                var tuple = cps.Select(cp => new KeyValuePair<ContentPresenter, double>(cp, GetElementMiddle(cp))).ToList();
                var closestLeft = tuple.Where(t => t.Value < newPosition).OrderBy(t => Math.Abs(t.Value - newPosition)).FirstOrDefault().Key;
                var closestRight = tuple.Where(t => t.Value > newPosition).OrderBy(t => Math.Abs(t.Value - newPosition)).FirstOrDefault().Key;

                var originalDrawingShape = contentPresenter.Content as IDrawingShape;
                if (originalDrawingShape == null) return;
                
                // insert after closest left
                if (closestLeft?.Content is IDrawingShape)
                {
                    var closestLeftShape = (IDrawingShape)closestLeft.Content;
                    var indexOfClosestLeft = ViewModel.Shapes.IndexOf(closestLeftShape);
                    ViewModel.Shapes.Move(ViewModel.Shapes.IndexOf(originalDrawingShape), indexOfClosestLeft + 1);
                }

                // Insert before closest right
                else if (closestRight?.Content is IDrawingShape)
                {
                    var closestRightShape = (IDrawingShape)closestRight.Content;
                    var indexOfClosestRight = ViewModel.Shapes.IndexOf(closestRightShape);

                    ViewModel.Shapes.Move(ViewModel.Shapes.IndexOf(originalDrawingShape), indexOfClosestRight);

                }

                AdornerLayer.GetAdornerLayer(overlayElement.AdornedElement).Remove(overlayElement);
            }
            finally
            {
                overlayElement = null;

                isDragging = false;
                isDown = false;
            }
        }

        private double GetElementMiddle(FrameworkElement element)
        {
            var x = Canvas.GetLeft(element) + 0.5 * element.ActualWidth;
            return x;
        }

    }

    public class SimpleAdorner : Adorner
    {
        public SimpleAdorner(UIElement adornedElement) : base(adornedElement)
        {
            VisualBrush brush = new VisualBrush(adornedElement);

            child = new Rectangle();
            child.Width = adornedElement.RenderSize.Width;
            child.Height = adornedElement.RenderSize.Height;
            child.Fill = brush;

            base.IsClipEnabled = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // Get a rectangle that represents the desired size of the rendered element
            // after the rendering pass.  This will be used to draw at the corners of the 
            // adorned element.
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

            // Some arbitrary drawing implements.
            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Green);
            renderBrush.Opacity = 0.2;
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);
            double renderRadius = 5.0;

            // Just draw a circle at each corner.
            //drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            child.Measure(constraint);
            return child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return child;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        public double LeftOffset
        {
            get
            {
                return _leftOffset;
            }
            set
            {
                _leftOffset = value;
                UpdatePosition();
            }
        }

        public double TopOffset
        {
            get
            {
                return _topOffset;
            }
            set
            {
                _topOffset = value;
                UpdatePosition();

            }
        }

        private void UpdatePosition()
        {
            AdornerLayer adornerLayer = this.Parent as AdornerLayer;
            if (adornerLayer != null)
            {
                adornerLayer.Update(AdornedElement);
            }
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
            return result;
        }

        private Rectangle child = null;
        private double _leftOffset = 0;
        private double _topOffset = 0;
    }
}