using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Extensions;
using DelftTools.Hydro.SewerFeatures;

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

        public static readonly DependencyProperty ShowLabelsProperty = DependencyProperty.Register(
            nameof(ShowLabels), typeof(bool), typeof(ManholeVisualisation), new PropertyMetadata(true, PropertyChangedCallback));

        public bool ShowLabels
        {
            get
            {
                return (bool) GetValue(ShowLabelsProperty);
            }
            set
            {
                SetValue(ShowLabelsProperty, value);
            }
        }

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

            if (!(sender is Canvas canvas))
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

            if (!(sender is Canvas canvas)) return;

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
            Mouse.Capture(null);
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

            if (shape is InternalConnectionShape)
                return connectionStrategy;

            throw new NotImplementedException($"Not able to obtain a strategy for the shape of type '{shape.GetType()}'");
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is ManholeVisualisation view))
            {
                return;
            }

            if (dependencyPropertyChangedEventArgs.Property == ManholeProperty)
            {
                view.ViewModel.Manhole = dependencyPropertyChangedEventArgs.NewValue as Manhole;
            }

            if (dependencyPropertyChangedEventArgs.Property == ShowLabelsProperty)
            {
                view.ViewModel.ShowLabels = (bool) dependencyPropertyChangedEventArgs.NewValue;
            }
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
}