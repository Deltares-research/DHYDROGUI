using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for ManholeVisualisationControl.xaml
    /// </summary>
    public partial class ManholeVisualisationControl : UserControl
    {
        public static readonly DependencyProperty ManholeProperty = DependencyProperty.Register(
            nameof(Manhole), 
            typeof(Manhole), 
            typeof(ManholeVisualisationControl), 
            new PropertyMetadata(default(Manhole), PropertyChangedCallback));

        #region Test with moving

        private bool isDragging = false;

        private double x_shape;
        private double y_shape;
        private double x_canvas;
        private double y_canvas;
        private UIElement source = null;
        


        private void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            source = sender as UIElement;
            if (source == null) return;
            Mouse.Capture(source);
            isDragging = true;
            x_shape = Canvas.GetLeft(source);
            x_canvas = e.GetPosition(Canvas).X;
            y_shape = Canvas.GetTop(source);
            y_canvas = e.GetPosition(Canvas).Y;
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var x = e.GetPosition(Canvas).X;
                var y = e.GetPosition(Canvas).Y;

                x_shape += x - x_canvas;
                Canvas.SetLeft(source, x_shape);
                x_canvas = x;

                y_shape += y - y_canvas;
                Canvas.SetTop(source, y_shape);
                y_canvas = y;
            }
        }

        private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            isDragging = false;
        }

        #endregion

        

        public ManholeVisualisationControl()
        {
            InitializeComponent();
            ViewModel.AddShapeAction = (s) =>
            {
                s.MouseLeftButtonDown += MouseLeftButtonDown;
                s.MouseLeftButtonUp += MouseLeftButtonUp;
                s.MouseMove += MouseMove;

                var shapeWidth = ((Shape) s).Width;
                var shapeHeight = ((Shape) s).Height;

                var left = (Canvas.ActualWidth - shapeWidth) / 2;
                var top = (Canvas.ActualHeight - shapeHeight) / 2;

                Canvas.SetTop(s, top);
                Canvas.SetLeft(s, left);
                Canvas.Children.Add(s);

                // TODO Reposition the children
                RepositionChildren();
            };
            ViewModel.RemoveShapeAction = () =>
            {
                var numberOfShapes = Canvas.Children.Count;
                if (numberOfShapes < 1) return;
                Canvas.Children.RemoveAt(numberOfShapes - 1);

                // TODO Reposition the children
                RepositionChildren();
            };
        }

        private void RepositionChildren()
        {
            var shapes = Canvas.Children.OfType<Shape>().ToList();

            var numberOfShapes = shapes.Count;

            const int elementSpacing = 10;
            var totalWidth = shapes.Sum(s => s.Width) + (numberOfShapes - 1) * elementSpacing;
            var panelWidth = Canvas.ActualWidth;

            var offsetLeft = (panelWidth - totalWidth) / 2;

            foreach (var shape in shapes)
            {
                Canvas.SetLeft(shape, offsetLeft);
                offsetLeft += shape.Width + elementSpacing;
            }
        }

    /*    private void DetermineShapePosition(UIElement shape)
        {
            var left = (Canvas.ActualWidth - Canvas.GetLeft(shape)) / 2;
            var canvasCenterX = 
        }*/

        public Manhole Manhole
        {
            get { return (Manhole) GetValue(ManholeProperty); }
            set { SetValue(ManholeProperty, value); }
        }

        

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as ManholeVisualisationControl;
            if (view == null) return;

            view.ViewModel.Manhole = dependencyPropertyChangedEventArgs.NewValue as Manhole;
        }

        
    }
}
