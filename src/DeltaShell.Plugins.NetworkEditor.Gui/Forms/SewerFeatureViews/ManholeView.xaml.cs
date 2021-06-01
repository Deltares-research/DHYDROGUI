using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Extensions;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for ManholeView.xaml
    /// </summary>
    public partial class ManholeView : UserControl, IReusableView
    {
        private Point startPoint;
        private Cursor customCursor;

        public ManholeView()
        {
            InitializeComponent();
            ViewModel.DeselectItem = () => ManholeVisualisationControl.DeselectItem();
        }

        public object Data
        {
            get { return ViewModel.Manhole; }
            set { ViewModel.Manhole = (Manhole) value; }
        }

        public bool Locked { get; set; }

        public string Text { get; set; }

        public Image Image { get; set; }
        
        public ViewInfo ViewInfo { get; set; }

        public void Dispose()
        {
        }

        [InvokeRequired]
        public void EnsureVisible(object item)
        {
            ViewModel.SelectedItem = item;
        }

        public event EventHandler LockedChanged;

        private void ToolBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void ToolBoxMouseMove(object sender, MouseEventArgs e)
        {
            if (!MouseMoved(e)) return;

            var listView = sender as ListView;

            // Get the dragged ListViewItem
            var listViewItem = ((DependencyObject) e.OriginalSource).TryFindParent<ListViewItem>();
            if (listViewItem == null) return;

            // Find the data behind the ListViewItem
            var data = (ShapeType) listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

            // Initialize the drag & drop operation
            var dragData = new DataObject("ShapeType", data);

            var rect = (UIElement)GetChildOfType<Rectangle>(listViewItem);

            if (customCursor == null)
            {
                CreateDragCursor(rect);
            }

            DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);

            customCursor.Dispose();
            customCursor = null;
        }

        private void CreateDragCursor(UIElement rect)
        {
            var iconItem = new Grid();

            var dragRectangle = new Rectangle
            {
                Style = (Style) Resources["DragRectangleStyle"],
                Width = rect.RenderSize.Width,
                Height = rect.RenderSize.Height,
                Fill = new VisualBrush(rect)
            };

            var dragCenterPoint = new Ellipse
            {
                Style = (Style) Resources["DragCenterPointStyle"]
            };

            iconItem.Children.Add(dragRectangle);
            iconItem.Children.Add(dragCenterPoint);

            var source = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice;
            if (source != null)
            {
                customCursor = CursorHelper.CreateCursor(iconItem, source.Value.M11, source.Value.M22);
            }
        }

        private bool MouseMoved(MouseEventArgs e)
        {
            var mousePos = e.GetPosition(null);
            var diff = startPoint - mousePos;

            return e.LeftButton == MouseButtonState.Pressed &&
                   Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                   Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance;
        }

        private static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void ManholeVisualisationControl_OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("ShapeType")) return;

            var item = e.Data.GetData("ShapeType");
            if (!(item is ShapeType)) return;

            var pos = e.GetPosition((IInputElement) sender);

            var actualWidth = ManholeVisualisationControl.ActualWidth;
            var desiredWidth = ManholeVisualisationControl.DesiredSize.Width;

            var horizontalOffset = (actualWidth - desiredWidth) * 0.5;
            pos.X = pos.X - horizontalOffset;

            var index = ManholeVisualisationControl.GetIndexFor(pos);

            var addedFeature = ViewModel.AddShape((ShapeType) item);

            var shapes = ManholeVisualisationControl.ViewModel.Shapes;

            var addedShape = shapes.FirstOrDefault(s => ReferenceEquals(s.Source, addedFeature));
            if (addedShape == null) return;
            var oldIndex = shapes.IndexOf(addedShape);

            if (!shapes.IndexInRange(oldIndex) || !shapes.IndexInRange(index)) return;

            shapes.Move(oldIndex, index);
        }

        private void ToolBoxGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (customCursor != null)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(customCursor);
            }

            e.Handled = true;
        }
    }
}