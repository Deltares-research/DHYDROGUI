using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Threading;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public partial class WeirView : UserControl, IView
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WeirView));

        private const string NumberFormat = "{0:0.000}";

        private string useMaxFlowNegPropertyName;
        private string useMaxFlowPosPropertyName;
        private string gateOpeningPropertyName;
        private string bedLevelStructureCenterPropertyName;
        private string widthStructureCenterPropertyName;
        private string useLowerEdgeLevelTimeSeriesPropertyName;

        private readonly WeirViewData weirViewData;
        
        private bool handlingPropertyChanged;
        private IWeir data;

        public object Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    UnSubscribeToWeir();
                }
                data = (IWeir)value;

                //set formula etc in data class
                bindingSourceWeir.DataSource = (object)Data ?? typeof(Weir);

                if (value == null)
                {
                    return;
                }

                weirViewData.UpdateDataWithWeir(data);

                RenderControls();
                RenderFormulaControls();

                if (data != null)
                {
                    SubscribeToWeir();
                }
                ConfigureTimeDependentControls();
            }
        }

        public WeirView() : this(null){ }

        public WeirView(IEnumerable<IWeirFormula> supportedFormulas)
        {
            SetConstants();

            weirViewData = new WeirViewData(supportedFormulas);
            InitializeComponent();
            FillCombobox(comboBoxWeirFormula, weirViewData.GetWeirFormulaTypes(), ComboBoxWeirFormulaSelectedIndexChanged);
            FillCombobox(comboBoxCrestShape, weirViewData.GetCrestShapes(), ComboBoxCrestShapeSelectedIndexChanged);
        }

        private void SetConstants()
        {
            var gatedWeirFormula = new GatedWeirFormula();
            var generalStructureWeirFormula = new GeneralStructureWeirFormula();

            useMaxFlowNegPropertyName = nameof(gatedWeirFormula.UseMaxFlowNeg);
            useMaxFlowPosPropertyName = nameof(gatedWeirFormula.UseMaxFlowPos);
            gateOpeningPropertyName = nameof(gatedWeirFormula.GateOpening);
            useLowerEdgeLevelTimeSeriesPropertyName = nameof(gatedWeirFormula.UseLowerEdgeLevelTimeSeries);

            bedLevelStructureCenterPropertyName = nameof(generalStructureWeirFormula.BedLevelStructureCentre);
            widthStructureCenterPropertyName = nameof(generalStructureWeirFormula.WidthStructureCentre);
        }

        private void ConfigureTimeDependentControls()
        {
            if (data != null && data.CanBeTimedependent)
            {
                TimeDependentLabel.Visible = true;
                CrestLevelTimeDependentCheckBox.Visible = true;
                CrestLevelTimeDependentCheckBox.Checked = data.UseCrestLevelTimeSeries;
                ConfigureUseCrestLevelTimeSeries();
            }
            else
            {
                TimeDependentLabel.Visible = false;
                OpenCrestLevelTimeSeriesButton.Visible = false;
                CrestLevelTimeDependentCheckBox.Visible = false;

                textBoxCrestLevel.Visible = true;
                textBoxCrestWidth.Visible = true;
            }
        }

        /// <summary>
        /// Sets or gets image set on the title of the view.
        /// </summary>
        public Image Image { get; set; }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }

        private static void FillCombobox<T>(ComboBox comboBox, IDictionary<string, T> dictionary, EventHandler indexChangedEventHandler)
        {
            var bindingList = new ThreadsafeBindingList<string>(SynchronizationContext.Current, dictionary.Keys.ToList());

            //disable index changed
            comboBox.SelectedIndexChanged -= indexChangedEventHandler;
            comboBox.DataSource = bindingList;
            comboBox.SelectedIndexChanged += indexChangedEventHandler;
        }

        private void SubscribeToWeir()
        {
            ((INotifyPropertyChanged)data).PropertyChanged += WeirPropertyChanged;
        }

        private void UnSubscribeToWeir()
        {
            ((INotifyPropertyChanged)data).PropertyChanged -= WeirPropertyChanged;
        }

        //TODO: get this in a weirviewcontroller? has pro's (Testabilility) and cons (view gets more complex,more players involved)
        //if view gets too complex move towards controller.
        private void WeirPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (handlingPropertyChanged) return;

            handlingPropertyChanged = true;

            if (sender is GatedWeirFormula &&
                (e.PropertyName == useMaxFlowNegPropertyName || e.PropertyName == useMaxFlowPosPropertyName))
            {
                RenderMaxFlowControls();
                handlingPropertyChanged = false;
                return;
            }

            if (sender is IGatedWeirFormula && 
                (e.PropertyName == gateOpeningPropertyName || e.PropertyName == useLowerEdgeLevelTimeSeriesPropertyName))
            {
                RenderGateControls();
                handlingPropertyChanged = false;
                return;
            }

            if (sender is GeneralStructureWeirFormula &&
                (e.PropertyName == bedLevelStructureCenterPropertyName || e.PropertyName == widthStructureCenterPropertyName))
            {
                RenderGateControls();
                handlingPropertyChanged = false;
                return;
            }

            var weir = sender as IWeir;
            if (weir == null)
            {
                handlingPropertyChanged = false;
                return;
            }

            if (nameof(weir.WeirFormula) == e.PropertyName)
            {
                RenderFormulaControls();
            }
            if (weir.CanBeTimedependent && nameof(weir.UseCrestLevelTimeSeries) == e.PropertyName)
            {
                CrestLevelTimeDependentCheckBox.Checked = weir.UseCrestLevelTimeSeries;
                ConfigureUseCrestLevelTimeSeries();
            }

            RenderControls();
            handlingPropertyChanged = false;
        }

        private void RenderGateControls()
        {
            var igatedWeirFormula = data.WeirFormula as IGatedWeirFormula;
            groupBoxGate.Enabled = (igatedWeirFormula != null);

            if (igatedWeirFormula != null)
            {
                textBoxLowerEdgeLevel.Text = string.Format(NumberFormat, igatedWeirFormula.LowerEdgeLevel);
                textBoxGateOpening.Text = string.Format(NumberFormat, igatedWeirFormula.LowerEdgeLevel - data.CrestLevel);
                textBoxGateHeight.Text = string.Format(NumberFormat, igatedWeirFormula.GateHeight);
            }

            var gatedWeirFormula = data.WeirFormula as GatedWeirFormula;
            if (gatedWeirFormula != null)
            {
                var bindingList = new ThreadsafeBindingList<string>(SynchronizationContext.Current, new[] {data.WeirFormula.Name});
                comboBoxWeirFormula.DataSource = bindingList;
                comboBoxWeirFormula.Enabled = false;
                LowerEdgeLevelTimeDependentCheckBox.Visible = gatedWeirFormula.CanBeTimedependent;
                LowerEdgeLevelTimeDependentCheckBox.Checked = gatedWeirFormula.UseLowerEdgeLevelTimeSeries;

                ConfigureUseLowerEdgeLevelTimeSeries(gatedWeirFormula);
            }
            else
            {
                FillCombobox(comboBoxWeirFormula, weirViewData.GetWeirFormulaTypes(), ComboBoxWeirFormulaSelectedIndexChanged);
                comboBoxWeirFormula.Enabled = true;
                LowerEdgeLevelTimeDependentCheckBox.Visible = false;
                OpenLowerEdgeLevelTimeSeriesButton.Visible = false;

                OpenGateOpeningTimeSeriesButton.Visible = false;

                textBoxLowerEdgeLevel.Visible = true;
                textBoxGateOpening.Visible = true;
            }
        }

        private void RenderControls()   
        {
            RenderGateControls();
            RenderMaxFlowControls();
            // Sync combobox items
            comboBoxWeirFormula.SelectedItem = weirViewData.GetWeirFormulaTypeName(data.WeirFormula);
            comboBoxCrestShape.SelectedItem = weirViewData.GetCrestShapeName(data.CrestShape);
            comboBoxCrestShape.Enabled = data.WeirFormula is RiverWeirFormula;
            labelCrestShape.Enabled = data.WeirFormula is RiverWeirFormula;
            labelGeometry.Text = (data.WeirFormula is FreeFormWeirFormula) ? "Free Form" : "Rectangle";
        }

        private void RenderMaxFlowControls()
        {
            var gatedWeirFormula = data.WeirFormula as GatedWeirFormula;

            var activePos = data.AllowPositiveFlow && gatedWeirFormula != null;
            var activeneg = data.AllowNegativeFlow && gatedWeirFormula != null;

            checkBoxMaxPos.Enabled = activePos;
            checkBoxMaxNeg.Enabled = activeneg;

            if (gatedWeirFormula != null)
            {
                textBoxMaxNeg.Text = string.Format(NumberFormat, gatedWeirFormula.MaxFlowNeg);
                textBoxMaxPos.Text = string.Format(NumberFormat, gatedWeirFormula.MaxFlowPos);
                checkBoxMaxPos.Checked = gatedWeirFormula.UseMaxFlowPos;
                checkBoxMaxNeg.Checked = gatedWeirFormula.UseMaxFlowNeg;
            }

            textBoxMaxPos.Enabled = activePos && checkBoxMaxPos.Checked;
            labelUnitMaxPos.Enabled = activePos && checkBoxMaxPos.Checked;
            textBoxMaxNeg.Enabled = activeneg && checkBoxMaxNeg.Checked;
            labelUnitMaxNeg.Enabled = activeneg && checkBoxMaxNeg.Checked;
        }

        private void CheckBoxMaxPosCheckedChanged(object sender, EventArgs e)
        {
            //can't databind here since the UseMaxFlow is not declared on Weir
            var gatedWeirFormula = data.WeirFormula as GatedWeirFormula;
            if (gatedWeirFormula != null && gatedWeirFormula.UseMaxFlowPos != checkBoxMaxPos.Checked)
            {
                gatedWeirFormula.UseMaxFlowPos = checkBoxMaxPos.Checked;
            }
        }

        private void CheckBoxMaxNegCheckedChanged(object sender, EventArgs e)
        {
            var gatedWeirFormula = data.WeirFormula as GatedWeirFormula;
            if (gatedWeirFormula != null && gatedWeirFormula.UseMaxFlowNeg != checkBoxMaxNeg.Checked)
            {
                gatedWeirFormula.UseMaxFlowNeg = checkBoxMaxNeg.Checked;
            }
        }

        private void ComboBoxCrestShapeSelectedIndexChanged(object sender, EventArgs e)
        {
            var crestShape = weirViewData.GetCrestShape((string) comboBoxCrestShape.SelectedItem);
            if (crestShape == data.CrestShape)
            {
                return;
            }
            data.CrestShape = crestShape;
            if(data.WeirFormula is RiverWeirFormula)
            {
                SetCorrectionCoefficientAndSubmergeLimit((RiverWeirFormula)data.WeirFormula, data.CrestShape);
            }
            RenderFormulaControls();
        }

        private void ComboBoxWeirFormulaSelectedIndexChanged(object sender, EventArgs e)
       {
           if (comboBoxWeirFormula.SelectedItem == null)
           {
               return;
           }

           var selectedFormulaType = weirViewData.GetWeirFormulaType((string) comboBoxWeirFormula.SelectedItem);

           if (data.WeirFormula.GetType() == selectedFormulaType)
           {
               return;
           }

           data.WeirFormula = weirViewData.GetWeirCurrentFormula(selectedFormulaType);

           if (data.WeirFormula is RiverWeirFormula)
           {
               SetCorrectionCoefficientAndSubmergeLimit((RiverWeirFormula) data.WeirFormula, data.CrestShape);
           }

           SetVelocityHeightCheckboxVisibility(data.WeirFormula);
       }

        private void SetVelocityHeightCheckboxVisibility(IWeirFormula weirFormula)
        {
            if (weirFormula is SimpleWeirFormula || weirFormula is GeneralStructureWeirFormula || weirFormula is GatedWeirFormula)
            {
                useVelocityHeightCheckBox.Enabled = true;
            }
            else
            {
                useVelocityHeightCheckBox.Enabled = false;
            }
        }

        private void RenderFormulaControls()
        {
            foreach (var disposableControl in panelFormula.Controls.OfType<IDisposable>())
            {
                disposableControl.Dispose();
            }
            panelFormula.Controls.Clear();

            var formulaControl = CreateWeirFormulaControl(data.WeirFormula);
            if (formulaControl != null) // gated weir doesn't have formula
            {
                groupBoxFormula.Visible = true;
                panelFormula.Controls.Add(formulaControl);
                panelFormula.MinimumSize = new Size(formulaControl.Width, formulaControl.Height);
            }
            groupBoxFormula.Visible = (formulaControl != null);//don't show empty groupbox
            //only select flow direction if applicable for formula.
            labelFlowDirection.Enabled = data.WeirFormula.HasFlowDirection;
            checkBoxAllowNegativeFlow.Enabled= data.WeirFormula.HasFlowDirection;
            checkBoxAllowPositiveFlow.Enabled = data.WeirFormula.HasFlowDirection;
        }

        private static Control CreateWeirFormulaControl(IWeirFormula weirFormula)
        {
            var simpleWeirFormula = weirFormula as SimpleWeirFormula;
            if (simpleWeirFormula != null)
            {
                return new SimpleWeirFormulaView{Data = simpleWeirFormula};
            }

            var riverWeirFormula = weirFormula as RiverWeirFormula;
            if (riverWeirFormula != null)
            {
                return new RiverWeirFormulaView { Data = riverWeirFormula };
            }

            var pierWeirFormula = weirFormula as PierWeirFormula;
            if (pierWeirFormula != null)
            {
                return new PierWeirFormulaView { Data = pierWeirFormula };
            }
            
            var freeFormWeirFormula = weirFormula as FreeFormWeirFormula;
            if (freeFormWeirFormula != null)
            {
                return new FreeFormWeirFormulaView { Data = freeFormWeirFormula };
            }

            var gatedWeirFormula = weirFormula as GatedWeirFormula;
            if (gatedWeirFormula != null)
            {
                return new GatedWeirFormulaView { Data = gatedWeirFormula };
            }

            var generalStructureWeirFormula = weirFormula as GeneralStructureWeirFormula;
            if (generalStructureWeirFormula != null)
            {
                return new GeneralStructureWeirFormulaView { Data = generalStructureWeirFormula };
            }

            return null;
        }

        private void TextBoxGateOpeningValidated(object sender, EventArgs e)
        {
            var gatedWeirFormula = data.WeirFormula as IGatedWeirFormula;
            if (gatedWeirFormula == null)
            {
                return;
            }
            
            double opening;
            if (Double.TryParse(textBoxGateOpening.Text, out opening))
            {
                if (opening < 0)
                {
                    gatedWeirFormula.GateOpening = 0;
                    Log.WarnFormat("Gate opening can not be negative and is set to 0");
                }
                else
                {
                    gatedWeirFormula.GateOpening = opening;
                }
            }
        }

        private void TextBoxGateHeightValidated(object sender, EventArgs e)
        {
            var gatedWeirFormula = data.WeirFormula as IGatedWeirFormula;
            if (gatedWeirFormula == null)
            {
                return;
            }

            double gateHeight;
            if (double.TryParse(textBoxGateHeight.Text, out gateHeight))
            {
                if (gateHeight < 0)
                {
                    gatedWeirFormula.GateHeight = 0;
                    Log.WarnFormat("Gate height can not be negative and is set to 0");
                }
                else
                {
                    gatedWeirFormula.GateHeight = gateHeight;
                }
            }
        }

        private void TextBoxLowerEdgeLevelValidated(object sender, EventArgs e)
        {
            var gatedWeirFormula = data.WeirFormula as IGatedWeirFormula;
            if (gatedWeirFormula == null)
            {
                return;
            }
            double lowerEdge;
            if (double.TryParse(textBoxLowerEdgeLevel.Text, out lowerEdge))
            {
                if (lowerEdge < data.CrestLevel)
                {
                    lowerEdge = data.CrestLevel;
                    Log.WarnFormat("Gate lower edge can not be smaller than crest level, lower edge is set to crest level");
                }
                else
                {
                    gatedWeirFormula.LowerEdgeLevel = lowerEdge;
                }

                gatedWeirFormula.GateOpening = lowerEdge - data.CrestLevel;
            }
        }
        
        private static void SetCorrectionCoefficientAndSubmergeLimit(RiverWeirFormula weirFormula, CrestShape crestShape)
        {
            double correctionCoefficient = 0.0;
            double submergeReductionLimit = 0.0;
            IFunction submergeReduction = null;

            switch (crestShape)
            {
                case CrestShape.Broad:
                    correctionCoefficient = 1.0;
                    submergeReductionLimit = 0.82;
                    submergeReduction = RiverWeirFormula.CrestBroadSubmergeReduction;
                    break;
                case CrestShape.Round:
                    correctionCoefficient = 1.1;
                    submergeReductionLimit = 0.3;
                    submergeReduction = RiverWeirFormula.CrestRoundSubmergeReduction;
                    break;
                case CrestShape.Sharp:
                    correctionCoefficient = 1.2;
                    submergeReductionLimit = 0.01;
                    submergeReduction = RiverWeirFormula.CrestSharpSubmergeReduction;
                    break;
                case CrestShape.Triangular:
                    correctionCoefficient = 1.05;
                    submergeReductionLimit = 0.67;
                    submergeReduction = RiverWeirFormula.CrestTriangularSubmergeReduction;
                    break;

            }

            weirFormula.CorrectionCoefficientPos = correctionCoefficient;
            weirFormula.CorrectionCoefficientNeg = correctionCoefficient;
            weirFormula.SubmergeLimitPos = submergeReductionLimit;
            weirFormula.SubmergeLimitNeg = submergeReductionLimit;
            weirFormula.SubmergeReductionPos = submergeReduction; 
            weirFormula.SubmergeReductionNeg = submergeReduction;
            
        }

        private void TextBoxMaxPosValidated(object sender, EventArgs e)
        {
            double maxPos;
            var gatedWeirFormula = data.WeirFormula as GatedWeirFormula;
            if ((Double.TryParse(textBoxMaxPos.Text, out maxPos)) && (gatedWeirFormula != null))
            {
                gatedWeirFormula.MaxFlowPos = maxPos;
            }
        }

        private void TextBoxMaxNegValidated(object sender, EventArgs e)
        {
            double maxNeg;
            var gatedWeirFormula = data.WeirFormula as GatedWeirFormula;
            if ((Double.TryParse(textBoxMaxNeg.Text, out maxNeg)) && (gatedWeirFormula != null))
            {
                gatedWeirFormula.MaxFlowNeg= maxNeg;
            }
        }

        private void OpenCrestLevelTimeSeriesButton_Click(object sender, EventArgs e)
        {
            var editFunctionDialog = new EditFunctionDialog { Text = "Time dependent crest level for Weir" };
            var dialogData = (TimeSeries)data.CrestLevelTimeSeries.Clone();
            editFunctionDialog.ColumnNames = new[] { "Date time", String.Format("Crest level [{0}]", CrestLevelUnitLabel.Text) };
            editFunctionDialog.Data = dialogData;
            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                data.CrestLevelTimeSeries.Time.Clear();
                data.CrestLevelTimeSeries.Components[0].Clear();
                data.CrestLevelTimeSeries.Time.SetValues(dialogData.Time.Values);
                data.CrestLevelTimeSeries.Components[0].SetValues(dialogData.Components[0].Values);
            }
        }

        private void CrestLevelTimeDependentCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            data.UseCrestLevelTimeSeries = CrestLevelTimeDependentCheckBox.Checked;
            ConfigureUseCrestLevelTimeSeries();
        }

        private void ConfigureUseCrestLevelTimeSeries()
        {
            if (data.UseCrestLevelTimeSeries)
            {
                OpenCrestLevelTimeSeriesButton.Visible = true;
                OpenGateOpeningTimeSeriesButton.Enabled = false;
                textBoxCrestLevel.Visible = false;
            }
            else
            {
                OpenCrestLevelTimeSeriesButton.Visible = false;
                OpenGateOpeningTimeSeriesButton.Enabled = true;
                textBoxCrestLevel.Visible = true;
            }
        }

        private void GateTimeDependentCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var gateTimeDependentCheckBox = (CheckBox) sender;
           
            var gatedWeirFormula = (GatedWeirFormula)data.WeirFormula;
            gatedWeirFormula.UseLowerEdgeLevelTimeSeries = gateTimeDependentCheckBox.Checked;
            ConfigureUseLowerEdgeLevelTimeSeries(gatedWeirFormula);
        }

        private void ConfigureUseLowerEdgeLevelTimeSeries(GatedWeirFormula gatedWeirFormula)
        {
            if (gatedWeirFormula.UseLowerEdgeLevelTimeSeries)
            {
                OpenLowerEdgeLevelTimeSeriesButton.Visible = true;
                OpenGateOpeningTimeSeriesButton.Visible = true;

                textBoxGateOpening.Visible = false;
                textBoxLowerEdgeLevel.Visible = false;
            }
            else
            {
                OpenLowerEdgeLevelTimeSeriesButton.Visible = false;
                OpenGateOpeningTimeSeriesButton.Visible = false;

                textBoxGateOpening.Visible = true;
                textBoxLowerEdgeLevel.Visible = true;
            }

            // Do not enable Gate Opening to be time dependently set when Crest Level is also time dependent
            OpenGateOpeningTimeSeriesButton.Enabled = !data.UseCrestLevelTimeSeries;
        }

        private void OpenLowerEdgeLevelTimeSeriesButton_Click(object sender, EventArgs e)
        {
            var lowerEdgeLevelTimeSeries = ((GatedWeirFormula) (data.WeirFormula)).LowerEdgeLevelTimeSeries;
            var dialogData = (TimeSeries)lowerEdgeLevelTimeSeries.Clone(true);
            var editFunctionDialog = new EditFunctionDialog
                {
                    Text = "Time dependent lower edge level for Weir",
                    ColumnNames = new[] {"Date time", String.Format("Lower edge level [{0}]", LowerEdgeLevelLabel.Text)},
                    Data = dialogData
                };

            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                lowerEdgeLevelTimeSeries.Time.Clear();
                lowerEdgeLevelTimeSeries.Components[0].Clear();
                lowerEdgeLevelTimeSeries.Time.SetValues(dialogData.Time.Values);
                lowerEdgeLevelTimeSeries.Components[0].SetValues(dialogData.Components[0].Values);
            }
        }

        private void OpenGateOpeningTimeSeriesButton_Click(object sender, EventArgs e)
        {
            // Precondition (fail fast):
            if (data.UseCrestLevelTimeSeries)
            {
                throw new InvalidOperationException("Do not allow time varying gate opening when crest level is used in a time variant way.");
            }


            var lowerEdgeLevelTimeSeries = ((GatedWeirFormula) (data.WeirFormula)).LowerEdgeLevelTimeSeries;
            var dialogData = (TimeSeries)lowerEdgeLevelTimeSeries.Clone(true);
            dialogData.Components[0].SetValues(dialogData.Components[0].Values.Cast<double>().Select(v => v - data.CrestLevel));

            var editFunctionDialog = new EditFunctionDialog
                {
                    Text = "Time dependent gate opening for Weir",
                    ColumnNames = new[] {"Date time", String.Format("Gate opening [{0}]", GateOpeningUnitLabel.Text)},
                    Data = dialogData
                };

            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                lowerEdgeLevelTimeSeries.Time.Clear();
                lowerEdgeLevelTimeSeries.Components[0].Clear();
                lowerEdgeLevelTimeSeries.Time.SetValues(dialogData.Time.Values);
                lowerEdgeLevelTimeSeries.Components[0].SetValues(dialogData.Components[0].Values.Cast<double>().Select(v => v + data.CrestLevel));
            }
        }
    }
}