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
        private CrossSectionDefinitionStandardViewModel data;

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
            if (data?.Definition != null)
            {
                panelDataView.Controls.Add(CrossSectionStandardShapeViewFactory.GetStandardShapeView(data.Definition.Shape));
            }
        }

        private void SetComboBoxType()
        {
            if (Data != null)
            {
                comboBoxShapeType.SelectedValue = data.Definition.ShapeType;
            }
        }

        public object Data
        {
            get { return data; }
            set
            {
                data = (CrossSectionDefinitionStandardViewModel) value;
                
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

                if (data.Definition.ShapeType != newShape)
                {
                    data.Definition.ShapeType = newShape;
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

            SetVisibilityShiftLevelItems();
            SetComboBoxType();
            SetStandardCrossSectionDataView();
        }

        private void SetVisibilityShiftLevelItems()
        {
            if (Data != null)
            {
                textBoxLevelShift.Visible = data.IsOnChannel;
                labelLevelShift.Visible = data.IsOnChannel;
            }
        }
    }
}
