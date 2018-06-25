using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DelftTools.Controls.Wpf.Extensions;
using DelftTools.Hydro.Structures;
using Point = System.Windows.Point;

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

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(ManholeVisualisation),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private Point startPoint;
        private Point currentPosition;
        private double originalLeft;
        private bool isDown;
        private bool isDragging;
        private ContentPresenter contentPresenter;
        private UIElement selectedElement;
        private MoveAdorner moveAdorner;
        private SelectedAdorner selectedItemAdorner;

        private int oldIndex;
        private int newIndex;
        private bool moveIsValid;
        private bool foundNewPosition;

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

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

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = sender as Canvas;
            if (canvas == null)
            {
                RemoveSelectedItemAdorner();
                selectedElement = null;
                SelectedItem = null;
                return;
            }

            contentPresenter = (e.Source as DependencyObject)?.TryFindParent<ContentPresenter>();
            if (contentPresenter == null) return;

            // remove old selected element layer
            RemoveSelectedItemAdorner();

            selectedElement = contentPresenter;
            SelectedItem = (((ContentPresenter)selectedElement).Content as IDrawingShape)?.Source;

            // Create element layer
            AddSelectedItemAdorner();

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
            if (!isDown) return;
            
            DragFinished(false);

            RemoveSelectedItemAdorner();

            selectedElement = contentPresenter;
            AddSelectedItemAdorner();

            e.Handled = true;
        }

        private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && isDragging)
            {
                DragFinished(true);
            }
        }

        private void DragStarted()
        {
            isDragging = true;
            moveIsValid = false;
            foundNewPosition = false;
            originalLeft = Canvas.GetLeft(contentPresenter);

            RemoveSelectedItemAdorner();

            AddMoveAdorner();
        }

        private void DragMoved(Canvas canvas)
        {
            currentPosition = Mouse.GetPosition(canvas);
            moveAdorner.LeftOffset = currentPosition.X - startPoint.X;
            moveAdorner.TopOffset = currentPosition.Y - startPoint.Y;

            moveIsValid = false;
            foundNewPosition = false;

            try
            {
                if (contentPresenter == null || canvas == null) return;
                foundNewPosition = TryDetermineNewPosition(canvas, contentPresenter);
            }
            finally
            {
                moveIsValid = ValidateMove();
            }
        }

        private void DragFinished(bool cancelled)
        {
            Mouse.Capture(null);
            try
            {
                if (!isDragging || cancelled) return;

                if (moveIsValid && foundNewPosition)
                {
                    ViewModel.Shapes.Move(oldIndex, newIndex);
                }
            }
            finally
            {
                RemoveMoveAdorner();
                isDragging = false;
                isDown = false;
                moveIsValid = false;
                foundNewPosition = false;
            }
        }

        private bool ValidateMove()
        {
            return true;
        }

        private bool TryDetermineNewPosition(Canvas canvas, ContentPresenter contentPresenter)
        {
            var originalDrawingShape = contentPresenter.Content as IDrawingShape;
            if (originalDrawingShape == null) return false;

            var cps = canvas.Children.OfType<ContentPresenter>().Where(cp => cp.Content is CompartmentShape || cp.Content is ConnectionShape);
            var newPosition = moveAdorner.LeftOffset + originalLeft + 0.5 * contentPresenter.ActualWidth;

            var tuple = cps.Select(cp => new KeyValuePair<ContentPresenter, double>(cp, GetElementMiddle(cp))).ToList();
            var closestLeft = tuple.Where(t => t.Value <= newPosition).OrderBy(t => Math.Abs(t.Value - newPosition)).FirstOrDefault().Key;
            // insert after closest left
            if (closestLeft?.Content is IDrawingShape)
            {
                var leftItem = (IDrawingShape)closestLeft.Content;
                if (leftItem == originalDrawingShape) return false;
                var indexOfClosestLeft = ViewModel.Shapes.IndexOf(leftItem);
                oldIndex = ViewModel.Shapes.IndexOf(originalDrawingShape);
                newIndex = oldIndex < indexOfClosestLeft ? indexOfClosestLeft : indexOfClosestLeft + 1;

                return true;
            }

            // Insert before closest right
            var closestRight = tuple.Where(t => t.Value > newPosition).OrderBy(t => Math.Abs(t.Value - newPosition)).FirstOrDefault().Key;
            if (!(closestRight?.Content is IDrawingShape)) return false;

            var closestRightShape = (IDrawingShape)closestRight.Content;
            var indexOfClosestRight = ViewModel.Shapes.IndexOf(closestRightShape);

            oldIndex = ViewModel.Shapes.IndexOf(originalDrawingShape);
            newIndex = indexOfClosestRight;
            return true;
        }

        private double GetElementMiddle(FrameworkElement element)
        {
            var x = Canvas.GetLeft(element) + 0.5 * element.ActualWidth;
            return x;
        }

        private void AddMoveAdorner()
        {
            moveAdorner = new MoveAdorner(contentPresenter);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(contentPresenter);
            layer.Add(moveAdorner);
        }

        private void RemoveMoveAdorner()
        {
            if (moveAdorner == null) return;
            
            AdornerLayer.GetAdornerLayer(moveAdorner.AdornedElement)?.Remove(moveAdorner);
            moveAdorner = null;
        }

        private void AddSelectedItemAdorner()
        {
            if (selectedElement == null) return;

            selectedItemAdorner = new SelectedAdorner(selectedElement as ContentPresenter);
            AdornerLayer.GetAdornerLayer(selectedElement).Add(selectedItemAdorner);
        }

        private void RemoveSelectedItemAdorner()
        {
            if (selectedItemAdorner == null) return;

            AdornerLayer.GetAdornerLayer(selectedItemAdorner.AdornedElement)?.Remove(selectedItemAdorner);
            selectedItemAdorner = null;
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

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as ManholeVisualisation;
            if (view == null) return;

            view.ViewModel.Manhole = dependencyPropertyChangedEventArgs.NewValue as Manhole;
        }
    }

    public class MoveAdorner : SimpleAdorner
    {
        private SolidColorBrush renderBrush = new SolidColorBrush(Colors.Pink);
        private Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);
        private Rect adornedElementRect;
        public MoveAdorner(UIElement adornedElement) : base(adornedElement)
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // Get a rectangle that represents the desired size of the rendered element
            // after the rendering pass.  This will be used to draw at the corners of the 
            // adorned element.
            adornedElementRect = new Rect(this.AdornedElement.RenderSize);

            // Some arbitrary drawing implements.
            renderBrush.Opacity = 0.2;
            
            
            // Just draw a circle at each corner.
            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);
        }
    }

    public class SelectedAdorner : SimpleAdorner
    {
        public SelectedAdorner(UIElement adornerdElement) : base(adornerdElement)
        {
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            // Get a rectangle that represents the desired size of the rendered element
            // after the rendering pass.  This will be used to draw at the corners of the 
            // adorned element.
            Rect adornedElementRect = new Rect(this.AdornedElement.RenderSize);

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
            Rect adornedElementRect = new Rect(this.AdornedElement.RenderSize);

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
            var adornerLayer = this.Parent as AdornerLayer;
            adornerLayer?.Update(AdornedElement);
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