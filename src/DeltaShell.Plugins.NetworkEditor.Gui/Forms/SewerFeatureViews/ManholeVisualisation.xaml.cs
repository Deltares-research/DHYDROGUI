using System;
using System.Collections.ObjectModel;
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
        
        private MoveAdorner moveAdorner;
        private SelectedAdorner selectedItemAdorner;
        
        private bool moveIsValid;
        private bool foundNewPosition;

        private readonly PipeShapeDragAndDropStrategy pipeStrategy;
        private readonly CompartmentShapeDragAndDropStrategy compartmentStrategy;
        private readonly ConnectionShapeDragAndDropStrategy connectionStrategy;

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public ManholeVisualisation()
        {
            InitializeComponent();
            ViewModel.ContainerWidth = () => ViewGrid.Width;
            ViewModel.ContainerHeight = () => ViewGrid.Height;
            ViewModel.SetWindowSize = SetViewGridSize;

            pipeStrategy = new PipeShapeDragAndDropStrategy();
            compartmentStrategy = new CompartmentShapeDragAndDropStrategy(ViewModel.Shapes);
            connectionStrategy = new ConnectionShapeDragAndDropStrategy(ViewModel.Shapes);
        }
        
        public Manhole Manhole
        {
            get { return (Manhole)GetValue(ManholeProperty); }
            set { SetValue(ManholeProperty, value); }
        }

        public void DeselectItem()
        {
            ContentPresenter = null;
            SelectedItem = null;
            if (isDragging) DragFinished(true);
        }

        private ContentPresenter ContentPresenter
        {
            get { return contentPresenter; }
            set
            {
                RemoveSelectedItemAdorner();
                contentPresenter = value;
                AddSelectedItemAdorner();

                SelectedItem = (contentPresenter?.Content as IDrawingShape)?.Source;
            }
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDown = false;

            var canvas = sender as Canvas;
            if (canvas == null)
            {
                ContentPresenter = null;
                return;
            }

            ContentPresenter = (e.Source as DependencyObject)?.TryFindParent<ContentPresenter>();
            if (contentPresenter == null) return;

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

            var mousePosition = e.GetPosition(canvas);

            if (isDragging == false && (Math.Abs(mousePosition.X - startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                        Math.Abs(mousePosition.Y - startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                DragStarted();
            }

            var diff = startPoint - mousePosition;

            if (e.LeftButton != MouseButtonState.Pressed ||
                (!(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance) &&
                 !(Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)))
            {
                return;
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

            e.Handled = true;
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

            if (contentPresenter == null || canvas == null) return;

            var helper = GetStrategyForShape(contentPresenter?.Content as IDrawingShape);
            if (helper == null) return;
            foundNewPosition = helper.FindNewPosition(canvas, contentPresenter, moveAdorner.LeftOffset, originalLeft);
            moveIsValid = helper.Validate();
        }

        private void DragFinished(bool cancelled)
        {
            Mouse. Capture(null);
            try
            {
                if (!isDragging || cancelled) return;

                if (!moveIsValid || !foundNewPosition) return;

                var helper = GetStrategyForShape(contentPresenter?.Content as IDrawingShape);
                helper?.Reposition();
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

        #region Adorner adding/removal

        private void AddMoveAdorner()
        {
            if (contentPresenter == null) return;

            moveAdorner = new MoveAdorner(contentPresenter);
            AdornerLayer.GetAdornerLayer(contentPresenter).Add(moveAdorner);
        }

        private void RemoveMoveAdorner()
        {
            if (moveAdorner == null) return;

            AdornerLayer.GetAdornerLayer(moveAdorner.AdornedElement)?.Remove(moveAdorner);
            moveAdorner = null;
        }

        private void AddSelectedItemAdorner()
        {
            if (contentPresenter == null) return;

            selectedItemAdorner = new SelectedAdorner(contentPresenter);
            AdornerLayer.GetAdornerLayer(contentPresenter).Add(selectedItemAdorner);
        }

        private void RemoveSelectedItemAdorner()
        {
            if (selectedItemAdorner == null) return;

            AdornerLayer.GetAdornerLayer(selectedItemAdorner.AdornedElement)?.Remove(selectedItemAdorner);
            selectedItemAdorner = null;
        }

        #endregion
        
        private void SetViewGridSize()
        {
            var ratio = ViewModel.HeightWidthRatio;
            if (double.IsNaN(ratio)) return;

            var actualHeight = UserControl.ActualHeight;
            var actualWidth = UserControl.ActualWidth;

            if (actualHeight / ratio < actualWidth)
            {
                // Adjust height to available height, scale width by ratio
                var height = actualHeight;
                ViewGrid.Height = height;
                ViewGrid.Width = height / ratio;
                return;
            }

            // Adjust width to available width, scale height by ratio
            var width = actualWidth;
            ViewGrid.Width = width;
            ViewGrid.Height = width * ratio;
        }

        private IDragAndDropStrategy GetStrategyForShape(IDrawingShape shape)
        {
            if (shape == null) return null;

            if (shape is CompartmentShape)
                return compartmentStrategy;

            if (shape is PipeShape)
                return pipeStrategy;

            if (shape is ConnectionShape)
                return connectionStrategy;

            throw new NotImplementedException($"Not able to obtain a strategy for the shape of type '{shape.GetType()}'");
        }

        private void ViewGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*SetViewGridSize();
            ViewModel.SetShapesPixelValues();*/
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as ManholeVisualisation;
            if (view == null) return;

            view.ViewModel.Manhole = dependencyPropertyChangedEventArgs.NewValue as Manhole;
        }

        private void ManholeVisualisation_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetViewGridSize();
            ViewModel.SetShapesPixelValues();
        }

        public int GetIndexFor(Point pos)
        {
            return ViewModel.GetIndexFor(pos);
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