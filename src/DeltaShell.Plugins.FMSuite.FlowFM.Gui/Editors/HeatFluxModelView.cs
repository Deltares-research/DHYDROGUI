using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public partial class HeatFluxModelView : UserControl, ICompositeView
    {
        private HeatFluxModel heatFluxModel;

        public HeatFluxModelView()
        {
            InitializeComponent();
            radiationCheckBox.CheckedChanged += RadiationCheckBoxOnCheckedChanged;
            ChildViews = new EventedList<IView>(new[]
            {
                tabbedMultipleFunctionView
            });
        }

        public TabbedMultipleFunctionView FunctionView => tabbedMultipleFunctionView;

        public override string Text => "Heat flux model data";

        public Image Image { get; set; }

        public IEventedList<IView> ChildViews { get; private set; }

        public bool HandlesChildViews => true;

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        public void ActivateChildView(IView childView)
        {
            // Nothing to be done, enforced through ICompositeView
        }

        private HeatFluxModel HeatFluxModel
        {
            get => heatFluxModel;
            set
            {
                heatFluxModel = value;
                radiationCheckBox.CheckedChanged -= RadiationCheckBoxOnCheckedChanged;
                if (heatFluxModel != null)
                {
                    radiationCheckBox.Enabled = heatFluxModel.CanHaveSolarRadiation;
                    radiationCheckBox.Checked = heatFluxModel.ContainsSolarRadiation;

                    if (heatFluxModel.MeteoData != null)
                    {
                        tabbedMultipleFunctionView.Data = new[]
                        {
                            heatFluxModel.MeteoData
                        };
                    }
                    else
                    {
                        tabbedMultipleFunctionView.Data = null;
                    }
                }
                else
                {
                    radiationCheckBox.Checked = false;
                    radiationCheckBox.Enabled = false;
                    tabbedMultipleFunctionView.Data = null;
                }

                radiationCheckBox.CheckedChanged += RadiationCheckBoxOnCheckedChanged;
            }
        }

        private void RadiationCheckBoxOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            var proceed = true;

            if (heatFluxModel != null)
            {
                if (radiationCheckBox.Enabled &&
                    !radiationCheckBox.Checked &&
                    heatFluxModel.MeteoData != null &&
                    heatFluxModel.MeteoData.Arguments[0].Values.Count > 0)
                {
                    DialogResult dialogResult = MessageBox.Show("Are you sure you want to erase solar radiation data?",
                                                                "Dismiss solar radiation",
                                                                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                    if (dialogResult == DialogResult.Cancel)
                    {
                        proceed = false;
                    }
                }

                if (proceed)
                {
                    heatFluxModel.ContainsSolarRadiation = radiationCheckBox.Checked;
                    tabbedMultipleFunctionView.FullRefresh();
                }
                else
                {
                    radiationCheckBox.CheckedChanged -= RadiationCheckBoxOnCheckedChanged;
                    // reset value back to what it was
                    radiationCheckBox.Checked = heatFluxModel.ContainsSolarRadiation;
                    radiationCheckBox.CheckedChanged += RadiationCheckBoxOnCheckedChanged;
                }
            }
        }

        #region IView

        public object Data
        {
            get => HeatFluxModel;
            set => HeatFluxModel = value as HeatFluxModel;
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion
    }
}