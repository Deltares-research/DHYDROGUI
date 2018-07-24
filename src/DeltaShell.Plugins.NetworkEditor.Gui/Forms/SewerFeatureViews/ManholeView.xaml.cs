using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DelftTools.Controls;
using DelftTools.Hydro.Structures;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for ManholeView.xaml
    /// </summary>
    public partial class ManholeView : UserControl, IView
    {
        private Point startPoint;

        public ManholeView()
        {
            InitializeComponent();
            ViewModel.DeselectItem = () => ManholeVisualisationControl.DeselectItem();
        }

        #region IView implementation

        public object Data
        {
            get { return ViewModel.Manhole; }
            set { ViewModel.Manhole = (Manhole) value; }
        }

        public void Dispose()
        {
        }

        public void EnsureVisible(object item)
        {
        }

        public string Text { get; set; }

        public Image Image { get; set; }

        public bool Visible { get; }

        public ViewInfo ViewInfo { get; set; }

        #endregion

        private void ToolBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void ToolBoxMouseMove(object sender, MouseEventArgs e)
        {
            if (!MouseMoved(e)) return;

            var listView = sender as ListView;

            // Get the dragged ListViewItem
            var listViewItem = FindAnchestor<ListViewItem>((DependencyObject) e.OriginalSource);
            if (listViewItem == null) return;

            // Find the data behind the ListViewItem
            var data = (ShapeType) listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

            // Initialize the drag & drop operation
            var dragData = new DataObject("ShapeType", data);

            DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
        }

        private bool MouseMoved(MouseEventArgs e)
        {
            var mousePos = e.GetPosition(null);
            var diff = startPoint - mousePos;

            return e.LeftButton == MouseButtonState.Pressed &&
                   Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                   Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance;
        }

        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T) current;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }

        private void ManholeVisualisationControl_OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("ShapeType")) return;

            var item = e.Data.GetData("ShapeType");
            if (item == null || !(item is ShapeType)) return;

            var pos = e.GetPosition((IInputElement) sender);
            var index = ManholeVisualisationControl.GetIndexFor(pos);

            ViewModel.AddShape((ShapeType) item, index);
        }
    }

    public enum ShapeType
    {
        Compartment,
        Pump,
        Weir,
        Orifice
    }
}