using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Binding;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections
{
    /// <summary>
    /// Draws the data of the standard cross-section. Does not auto-update. Call refresh
    /// </summary>
    public partial class CrossSectionStandardDataView : UserControl,IView
    {
        private CrossSectionDefinitionStandard data;

        public CrossSectionStandardDataView()
        {
            InitializeComponent();

            var dataSource = EnumBindingHelper.ToList<CrossSectionStandardShapeType>();
            
            comboBoxShapeType.DataSource = dataSource;
            comboBoxShapeType.DisplayMember = "Value";
            comboBoxShapeType.ValueMember = "Key";
            
        }
        
        private void SetStandardCrossSectionDataView()
        {
            panelDataView.Controls.Clear();
            if (Data != null)
            {
                panelDataView.Controls.Add(CrossSectionStandardShapeViewFactory.GetStandardShapeView(data.Shape));
            }
        }

        private void SetComboBoxType()
        {
            if (Data != null)
            {
                comboBoxShapeType.SelectedValue = data.ShapeType;
            }
        }

        public object Data
        {
            get { return data; }
            set
            {
                data = (CrossSectionDefinitionStandard) value;
                
                RefreshView();
            }
        }

        public Image Image
        {
            get; set;
        }

        public void EnsureVisible(object item)
        {
            throw new NotImplementedException();
        }

        public ViewInfo ViewInfo { get; set; }

        private void ComboBoxShapeTypeSelectedIndexChanged(object sender, EventArgs e)
        {
            if (Data != null)
            {
                var newShape = (CrossSectionStandardShapeType) comboBoxShapeType.SelectedValue;

                if (data.ShapeType != newShape)
                {
                    data.ShapeType = newShape;
                }
            }
        }

        /// <summary>
        /// Redraws the view based on current data. Needed because this view does not subscribe to data. Parent view is responsible for notification.
        /// </summary>
        public void RefreshView()
        {
            if (data != null)
            {
                bindingSourceStandardDefinition.DataSource = data;
            }

            SetComboBoxType();
            SetStandardCrossSectionDataView();
        }
    }
}
