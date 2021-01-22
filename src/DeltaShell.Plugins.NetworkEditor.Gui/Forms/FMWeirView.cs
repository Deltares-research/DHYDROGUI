using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    public partial class FMWeirView : UserControl, IReusableView
    {
        private readonly Button crestLevelButton;

        private IWeir data;
        private bool handlingPropertyChanged;

        public FMWeirView()
        {
            InitializeComponent();
            crestLevelButton = CreateTimeseriesButton(CrestLevelButton_Click);
        }

        public object Data
        {
            get
            {
                return data;
            }
            set
            {
                if (data != null)
                {
                    ((INotifyPropertyChanged) data).PropertyChanged -= WeirPropertyChanged;
                }

                data = (IWeir) value;

                //set formula etc in data class
                bindingSourceWeir.DataSource = Data ?? typeof(Weir);
                bindingSourceFormula.DataSource = data != null && data.WeirFormula != null
                                                      ? (object) data.WeirFormula
                                                      : typeof(SimpleWeirFormula);

                if (value == null)
                {
                    return;
                }

                // nothing to do here

                if (data != null)
                {
                    ((INotifyPropertyChanged) data).PropertyChanged += WeirPropertyChanged;
                }

                ConfigureTimeDependentControls();
                UpdateUseCrestLevel();
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }
        public void EnsureVisible(object item) {}

        private void UpdateUseCrestLevel()
        {
            if (data == null)
            {
                return;
            }

            bool useCrestLevel = data.CrestWidth > 0.0;
            checkBoxUseCrestWidth.Checked = useCrestLevel;
            textBoxCrestWidth.Enabled = useCrestLevel;
        }

        private void ConfigureTimeDependentControls()
        {
            if (data != null)
            {
                crestLevelCheckBox.Checked = data.UseCrestLevelTimeSeries;
                ConfigureCrestLevelTimeSeries();
            }
        }

        private void ConfigureCrestLevelTimeSeries()
        {
            if (data.UseCrestLevelTimeSeries)
            {
                // remove the textbox
                if (crestLevelContainer.Contains(crestLevelTextBox))
                {
                    crestLevelContainer.Controls.Remove(crestLevelTextBox);
                }

                // add the button
                if (!crestLevelContainer.Contains(crestLevelButton))
                {
                    crestLevelContainer.Controls.Add(crestLevelButton);
                }
            }
            else
            {
                // remove the button
                if (crestLevelContainer.Contains(crestLevelButton))
                {
                    crestLevelContainer.Controls.Remove(crestLevelButton);
                }

                // add the textbox
                if (!crestLevelContainer.Contains(crestLevelTextBox))
                {
                    crestLevelContainer.Controls.Add(crestLevelTextBox);
                }
            }
        }

        private static Button CreateTimeseriesButton(EventHandler handler)
        {
            var result = new Button
            {
                Dock = DockStyle.Fill,
                Text = "Time series..."
            };
            result.Click += handler;

            return result;
        }

        private static bool ParseDouble(string input, out double result)
        {
            double number;
            if (double.TryParse(input, out number) && !double.IsNaN(number) && double.IsInfinity(number))
            {
                result = number;
                return true;
            }

            result = double.NaN;
            return false;
        }

        private void LateralCoefficientTextBoxValidated(object sender, EventArgs e)
        {
            double coeff;
            if (ParseDouble(lateralCoefficientTextBox.Text, out coeff))
            {
                ((SimpleWeirFormula) data.WeirFormula).LateralContraction = coeff;
            }
        }

        private void CrestLevelTextBoxValidated(object sender, EventArgs e)
        {
            double crestLevel;
            if (ParseDouble(crestLevelTextBox.Text, out crestLevel))
            {
                data.CrestLevel = crestLevel;
            }
        }

        private void CrestWidthTextBoxValidated(object sender, EventArgs e)
        {
            double crestWidth;
            if (ParseDouble(textBoxCrestWidth.Text, out crestWidth))
            {
                data.CrestWidth = crestWidth;
            }
        }

        private void CheckBoxUseCrestWidthCheckedChanged(object sender, EventArgs e)
        {
            if (handlingPropertyChanged || Data == null)
            {
                return;
            }

            if (checkBoxUseCrestWidth.Checked && data.CrestWidth <= 0.0)
            {
                data.CrestWidth = data.Geometry.Length;
            }

            if (!checkBoxUseCrestWidth.Checked && data.CrestWidth > 0.0)
            {
                data.CrestWidth = 0.0;
            }
        }

        #region Eventing

        private void CrestLevelButton_Click(object sender, EventArgs e)
        {
            var dialogData = (TimeSeries) data.CrestLevelTimeSeries.Clone(true);

            var editFunctionDialog = new EditFunctionDialog
            {
                Text = $"{GuiParameterNames.CrestLevel} time series for " + data.Name,
                ColumnNames = new[]
                {
                    "Date time",
                    $"{GuiParameterNames.CrestLevel} [m]"
                },
                ChartViewOption = ChartViewOptions.AllSeries,
                Data = dialogData,
                ShowOnlyFirstWordInColumnHeadersOnLoad = false
            };

            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                data.CrestLevelTimeSeries.Time.Clear();
                data.CrestLevelTimeSeries.Components[0].Clear();
                data.CrestLevelTimeSeries.Time.SetValues(dialogData.Time.Values);
                data.CrestLevelTimeSeries.Components[0].SetValues(dialogData.Components[0].Values.Cast<double>());
            }
        }

        private void WeirPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (handlingPropertyChanged)
            {
                return;
            }

            handlingPropertyChanged = true;

            var weir = sender as IWeir;
            if (weir == null)
            {
                handlingPropertyChanged = false;
                return;
            }

            if (nameof(weir.UseCrestLevelTimeSeries) == e.PropertyName)
            {
                crestLevelCheckBox.Checked = weir.UseCrestLevelTimeSeries;
                ConfigureCrestLevelTimeSeries();
            }

            if (nameof(weir.CrestWidth) == e.PropertyName)
            {
                UpdateUseCrestLevel();
            }

            // nothing to do here
            handlingPropertyChanged = false;
        }

        private void CrestLevelCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            data.UseCrestLevelTimeSeries = crestLevelCheckBox.Checked;
            ConfigureCrestLevelTimeSeries();
        }

        #endregion Eventing

        #region Reusable

        private bool locked;
        public event EventHandler LockedChanged;

        public bool Locked
        {
            get
            {
                return locked;
            }
            set
            {
                locked = value;
                if (LockedChanged != null)
                {
                    LockedChanged(this, new EventArgs());
                }
            }
        }

        #endregion Reusable
    }
}