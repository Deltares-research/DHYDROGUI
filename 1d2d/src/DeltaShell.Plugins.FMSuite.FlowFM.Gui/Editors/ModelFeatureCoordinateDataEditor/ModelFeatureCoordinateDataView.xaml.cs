using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.ModelFeatureCoordinateDataEditor
{
    /// <summary>
    /// Interaction logic for ModelFeatureCoordinateDataView.xaml
    /// </summary>
    public partial class ModelFeatureCoordinateDataView : UserControl, IReusableView
    {
        private bool locked;

        public ModelFeatureCoordinateDataView()
        {
            InitializeComponent();

            // inject view logic in viewmodel
            ViewModel.AddColumn = (propertyName, columnHeader, isReadOnly, format) =>
            {
                var bindingBase = new Binding{Path = new PropertyPath(propertyName)};
                if (!string.IsNullOrEmpty(format))
                {
                    bindingBase.StringFormat = format;
                }

                DataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = columnHeader,
                    Binding = bindingBase,
                    IsReadOnly = isReadOnly
                });
            };
            ViewModel.ClearColumns = () => DataGrid.Columns.Clear();
        }

        public object Data
        {
            get { return ViewModel.ModelFeatureCoordinateData; }
            set { ViewModel.ModelFeatureCoordinateData = (IModelFeatureCoordinateData) value; }
        }

        public string Text { get; set; }

        public Image Image { get; set; }

        public bool Visible
        {
            get { return IsVisible; }
        }

        public ViewInfo ViewInfo { get; set; }

        public bool Locked
        {
            get { return locked; }
            set
            {
                locked = value;
                if (LockedChanged != null)
                {
                    LockedChanged(this, new EventArgs());
                }
            }
        }

        public void Dispose()
        {
            ViewModel.Dispose();
            BoundaryGeometryPreview?.Dispose();
            BoundaryGeometryPreview = null;
        }

        public void EnsureVisible(object item)
        {
        }

        public event EventHandler LockedChanged;
    }
}
