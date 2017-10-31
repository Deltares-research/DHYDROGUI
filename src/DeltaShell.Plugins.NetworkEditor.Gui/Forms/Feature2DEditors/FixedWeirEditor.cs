using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.Feature2DEditors
{
    public partial class FixedWeirEditor : UserControl, IReusableView
    {
        private bool locked;
        private FixedWeir fixedWeir;

        private class FixedWeirRow
        {
            private readonly int row;

            private readonly FixedWeir fixedWeir;

            public FixedWeirRow(FixedWeir fixedWeir, int row)
            {
                this.fixedWeir = fixedWeir;
                this.row = row;
            }

            public double X
            {
                get { return fixedWeir.Geometry.Coordinates[row].X; }
            }

            public double Y
            {
                get { return fixedWeir.Geometry.Coordinates[row].Y; }
            }

            [DisplayName("Crest level")]
            public double CrestLevel
            {
                get { return fixedWeir.CrestLevels[row]; }
                set
                {
                    fixedWeir.CrestLevels[row] = value;
                    fixedWeir.Geometry.Coordinates[row].Z = value;
                }
            }

            [DisplayName("Left sill depth")]
            public double LeftGroundDistance
            {
                get { return fixedWeir.GroundLevelsLeft[row]; }
                set { fixedWeir.GroundLevelsLeft[row] = value; }
            }

            [DisplayName("Right sill depth")]
            public double RightGroundDistance
            {
                get { return fixedWeir.GroundLevelsRight[row]; }
                set { fixedWeir.GroundLevelsRight[row] = value; }
            }
        }

        public FixedWeirEditor()
        {
            InitializeComponent();
            tableView.SelectionChanged += TableViewOnSelectionChanged;
        }

        private void TableViewOnSelectionChanged(object sender, TableSelectionChangedEventArgs tableSelectionChangedEventArgs)
        {
            var selectedIndices = tableView.SelectedRowsIndices;
            boundaryGeometryPreview.SelectedPoints = selectedIndices;
        }

        private FixedWeir FixedWeir
        {
            get { return fixedWeir; }
            set
            {
                if (fixedWeir != null)
                {
                    tableView.Data = null;
                    ((INotifyPropertyChanged)fixedWeir).PropertyChanged -= OnFixedWeirGeometryChanged;
                }
                fixedWeir = value;

                if (fixedWeir != null)
                {
                    ((INotifyPropertyChanged) fixedWeir).PropertyChanged += OnFixedWeirGeometryChanged;
                    UpdateView();
                }
            }
        }

        private void OnFixedWeirGeometryChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Equals(sender, fixedWeir) && e.PropertyName == TypeUtils.GetMemberName<FixedWeir>(fw => fw.Geometry))
            {
                UpdateView();
            }
        }

        private void UpdateView()
        {
            var count = fixedWeir.Geometry.Coordinates.Count();
            var dataList = Enumerable.Range(0, count).Select(i => new FixedWeirRow(fixedWeir, i)).ToList();
            tableView.Data = dataList;
            boundaryGeometryPreview.Feature = fixedWeir;
            boundaryGeometryPreview.FeatureGeometry = fixedWeir.Geometry;
            boundaryGeometryPreview.DataPoints =
                new EventedList<int>(Enumerable.Range(0, fixedWeir.Geometry.Coordinates.Count()));
        }

        public object Data
        {
            get { return FixedWeir; }
            set { FixedWeir = value as FixedWeir; }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
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

        public event EventHandler LockedChanged;
    }
}
