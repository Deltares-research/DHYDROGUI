using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews
{
    public partial class FreeFormWeirFormulaView : UserControl, IView
    {
        private FreeFormWeirFormula data;

        public FreeFormWeirFormulaView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets data shown by this view. Usually it is any object in the system which can be shown by some IView derived class.
        /// </summary>
        public object Data
        {
            get { return data; }
            set
            {
                data = (FreeFormWeirFormula) value;
                yzTableView.CollectionChanged += YZTableViewCollectionChanged;
                yzTableView.PropertyChanged += YZTableViewPropertyChanged;
                YZTableReBind();
                bindingSourceFreeFormWeirFormula.DataSource = data;
            }
        }

        void YZTableViewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var coordinate = sender as Coordinate;
            if (null == coordinate)
            {
                return;
            }
            if (e.PropertyName == "X" || e.PropertyName == "Y")
            {
                SetTableToWeirFormula();
            }
        }

        void YZTableViewCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
            {
                SetTableToWeirFormula();
            }
        }

        private void SetTableToWeirFormula()
        {
            var yzData = yzTableView.Data as IEventedList<SimplifiedCoordinate>;
            data.SetShape(yzData.Select(yz => yz.X).ToArray(), yzData.Select(yz => yz.Y).ToArray());
        }

        void YZTableReBind()
        {
            yzTableView.Data = new EventedList<Coordinate>(data.Shape.Coordinates.Cast<Coordinate>());
            yzTableView.EditableObject = data;
        }

        /// <summary>
        /// Sets or gets image set on the title of the view.
        /// </summary>
        public Image Image { get; set; }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }
    }
}