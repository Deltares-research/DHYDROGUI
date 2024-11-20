using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Threading;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public partial class GateView : UserControl, IReusableView
    {
        private readonly Button sillLevelButton;
        private readonly Button lowerEdgeLevelButton;
        private readonly Button openingWidthButton;

        private readonly GateViewData gateViewData;
        private IGate data;
        private bool handlingPropertyChanged;

        public object Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    UnSubscribeToGate();
                }
                data = (IGate)value;

                //set formula etc in data class
                bindingSourceGate.DataSource = Data ?? typeof(Gate);

                if (value == null)
                {
                    return;
                }

                RenderControls();

                if (data != null)
                {
                    SubscribeToGate();
                }
                ConfigureTimeDependentControls();
                UpdateUseSillWidth();
            }
        }

        private void UpdateUseSillWidth()
        {
            if (data == null) return;
            var useSillWidth = data.SillWidth > 0.0;
            checkBoxUseSillWidth.Checked = useSillWidth;
            sillWidthTextBox.Enabled = useSillWidth;
        }

        public GateView()
        {
            gateViewData = new GateViewData();
            InitializeComponent();
            sillLevelButton = CreateTimeseriesButton(SillLevelButtonClick);
            lowerEdgeLevelButton = CreateTimeseriesButton(LowerEdgeLevelButtonClick);
            openingWidthButton = CreateTimeseriesButton(OpeningWidthButtonClick);
            FillOpeningDirectionCombobox();
        }

        private void ConfigureTimeDependentControls()
        {
            if (data != null)
            {
                sillLevelCheckBox.Checked = data.UseSillLevelTimeSeries;
                ConfigureSillLevelTimeSeries();                

                lowerEdgeLevelCheckBox.Checked = data.UseLowerEdgeLevelTimeSeries;
                ConfigureLowerEdgeLevelTimeSeries();

                openingWidthCheckBox.Checked = data.UseOpeningWidthTimeSeries;
                ConfigureOpeningWidthTimeSeries();
            }
        }

        private void ConfigureOpeningWidthTimeSeries()
        {
            if (data.UseOpeningWidthTimeSeries)
            {
                // remove the textbox
                if (openingWidthContainer.Contains(openingWidthTextBox))
                {
                    openingWidthContainer.Controls.Remove(openingWidthTextBox);
                }

                // add the button
                if (!openingWidthContainer.Contains(openingWidthButton))
                {
                    openingWidthContainer.Controls.Add(openingWidthButton);
                }
            }
            else
            {
                // remove the button
                if (openingWidthContainer.Contains(openingWidthButton))
                {
                    openingWidthContainer.Controls.Remove(openingWidthButton);
                }

                // add the textbox
                if (!openingWidthContainer.Contains(openingWidthTextBox))
                {
                    openingWidthContainer.Controls.Add(openingWidthTextBox);
                }
            }
        }

        private void ConfigureSillLevelTimeSeries()
        {
            if (data.UseSillLevelTimeSeries)
            {
                // remove the textbox
                if (sillLevelContainer.Contains(sillLevelTextBox))
                {
                    sillLevelContainer.Controls.Remove(sillLevelTextBox);
                }

                // add the button
                if (!sillLevelContainer.Contains(sillLevelButton))
                {
                    sillLevelContainer.Controls.Add(sillLevelButton);
                }
            }
            else
            {
                // remove the button
                if (sillLevelContainer.Contains(sillLevelButton))
                {
                    sillLevelContainer.Controls.Remove(sillLevelButton);
                }

                // add the textbox
                if (!sillLevelContainer.Contains(sillLevelTextBox))
                {
                    sillLevelContainer.Controls.Add(sillLevelTextBox);
                }
            }
        }

        private void ConfigureLowerEdgeLevelTimeSeries()
        {
            if (data.UseLowerEdgeLevelTimeSeries)
            {
                // remove the textbox
                if (lowerEdgeLevelContainer.Contains(lowerEdgeLevelTextBox))
                {
                    lowerEdgeLevelContainer.Controls.Remove(lowerEdgeLevelTextBox);
                }

                // add the button
                if (!lowerEdgeLevelContainer.Contains(lowerEdgeLevelButton))
                {
                    lowerEdgeLevelContainer.Controls.Add(lowerEdgeLevelButton);
                }
            }
            else
            {
                // remove the button
                if (lowerEdgeLevelContainer.Contains(lowerEdgeLevelButton))
                {
                    lowerEdgeLevelContainer.Controls.Remove(lowerEdgeLevelButton);
                }

                // add the textbox
                if (!lowerEdgeLevelContainer.Contains(lowerEdgeLevelTextBox))
                {
                    lowerEdgeLevelContainer.Controls.Add(lowerEdgeLevelTextBox);
                }
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }

        private void FillOpeningDirectionCombobox()
        {
            var bindingList = new ThreadsafeBindingList<string>(SynchronizationContext.Current, gateViewData.GetGateOpeningTypes().Keys.ToList());

            //disable index changed
            openingDirectionComboBox.SelectedIndexChanged -= OpeningDirectionComboboxSelectionChanged;
            openingDirectionComboBox.DataSource = bindingList;
            openingDirectionComboBox.SelectedIndexChanged += OpeningDirectionComboboxSelectionChanged;
        }

        private void RenderControls()
        {
            // sync combobox items
            openingDirectionComboBox.SelectedItem = gateViewData.GetGateOpeningName(data.HorizontalOpeningDirection);
        }

        private static Button CreateTimeseriesButton(EventHandler handler)
        {
            var result = new Button {Dock = DockStyle.Fill, Text = "Time series..."};
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

        #region Eventing

        private void SubscribeToGate()
        {
            ((INotifyPropertyChanged) data).PropertyChanged += GatePropertyChanged;
        }

        private void UnSubscribeToGate()
        {
            ((INotifyPropertyChanged) data).PropertyChanged -= GatePropertyChanged;
        }

        private void OpeningWidthButtonClick(object sender, EventArgs e)
        {
            var dialogData = (TimeSeries)data.OpeningWidthTimeSeries.Clone(true);

            var editFunctionDialog = new EditFunctionDialog
            {
                Text = "Opening width time series for " + data.Name,
                ColumnNames = new[] {"Date time", String.Format("Opening width [m]")},
                ChartViewOption = ChartViewOptions.AllSeries,
                Data = dialogData,
            };

            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                data.OpeningWidthTimeSeries.Time.Clear();
                data.OpeningWidthTimeSeries.Components[0].Clear();
                data.OpeningWidthTimeSeries.Time.SetValues(dialogData.Time.Values);
                data.OpeningWidthTimeSeries.Components[0].SetValues(dialogData.Components[0].Values.Cast<double>());
            }
        }

        private void SillLevelButtonClick(object sender, EventArgs e)
        {
            var dialogData = (TimeSeries)data.SillLevelTimeSeries.Clone(true);

            var editFunctionDialog = new EditFunctionDialog
            {
                Text = "Sill level time series for " + data.Name,
                ColumnNames = new[] { "Date time", String.Format("Lower edge level [m]") },
                ChartViewOption = ChartViewOptions.AllSeries,
                Data = dialogData,
            };

            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                data.SillLevelTimeSeries.Time.Clear();
                data.SillLevelTimeSeries.Components[0].Clear();
                data.SillLevelTimeSeries.Time.SetValues(dialogData.Time.Values);
                data.SillLevelTimeSeries.Components[0].SetValues(dialogData.Components[0].Values.Cast<double>());
            }
        }

        private void LowerEdgeLevelButtonClick(object sender, EventArgs e)
        {
            var dialogData = (TimeSeries)data.LowerEdgeLevelTimeSeries.Clone(true);

            var editFunctionDialog = new EditFunctionDialog
            {
                Text = "Lower edge level time series for " + data.Name,
                ColumnNames = new[] {"Date time", String.Format("Lower edge level [m]")},
                ChartViewOption = ChartViewOptions.AllSeries,
                Data = dialogData,
            };

            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                data.LowerEdgeLevelTimeSeries.Time.Clear();
                data.LowerEdgeLevelTimeSeries.Components[0].Clear();
                data.LowerEdgeLevelTimeSeries.Time.SetValues(dialogData.Time.Values);
                data.LowerEdgeLevelTimeSeries.Components[0].SetValues(dialogData.Components[0].Values.Cast<double>());
            }
        }

        private void GatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (handlingPropertyChanged) return;

            handlingPropertyChanged = true;

            var gate = sender as IGate;
            if (gate == null)
            {
                handlingPropertyChanged = false;
                return;
            }

            if (nameof(gate.UseSillLevelTimeSeries) == e.PropertyName)
            {
                sillLevelCheckBox.Checked = gate.UseSillLevelTimeSeries;
                ConfigureSillLevelTimeSeries();
            }

            if (nameof(gate.UseOpeningWidthTimeSeries) == e.PropertyName)
            {
                openingWidthCheckBox.Checked = gate.UseOpeningWidthTimeSeries;
                ConfigureOpeningWidthTimeSeries();
            }

            if (nameof(gate.UseLowerEdgeLevelTimeSeries) == e.PropertyName)
            {
                lowerEdgeLevelCheckBox.Checked = gate.UseLowerEdgeLevelTimeSeries;
                ConfigureLowerEdgeLevelTimeSeries();
            }

            if (nameof(gate.SillWidth) == e.PropertyName)
            {
                UpdateUseSillWidth();
            }

            RenderControls();
            handlingPropertyChanged = false;
        }

        private void OpeningDirectionComboboxSelectionChanged(object sender, EventArgs e)
        {
            var selectedOpeningType = gateViewData.GetGateOpeningType((string) openingDirectionComboBox.SelectedItem);

            if (data.HorizontalOpeningDirection == selectedOpeningType)
            {
                return;
            }
            data.HorizontalOpeningDirection = selectedOpeningType;
        }


        private void SillLevelCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            data.UseSillLevelTimeSeries = sillLevelCheckBox.Checked;
            ConfigureSillLevelTimeSeries();
        }

        private void LowerEdgeLevelCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            data.UseLowerEdgeLevelTimeSeries = lowerEdgeLevelCheckBox.Checked;
            ConfigureLowerEdgeLevelTimeSeries();
        }

        private void OpeningWidthCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            data.UseOpeningWidthTimeSeries = openingWidthCheckBox.Checked;
            ConfigureOpeningWidthTimeSeries();
        }

        private void SillLevelTextBoxValidated(object sender, EventArgs e)
        {
            double sillLevel;
            if (ParseDouble(sillLevelTextBox.Text, out sillLevel))
            {
                data.SillLevel = sillLevel;
            }
        }

        private void DoorHeightTextBoxValidated(object sender, EventArgs e)
        {
            double doorHeight;
            if (ParseDouble(doorHeightTextBox.Text, out doorHeight))
            {
                data.DoorHeight = doorHeight;
            }
        }

        private void LowerEdgeLevelTextBoxValidated(object sender, EventArgs e)
        {
            double lowerEdgeLevel;
            if (ParseDouble(lowerEdgeLevelTextBox.Text, out lowerEdgeLevel))
            {
                data.LowerEdgeLevel = lowerEdgeLevel;
            }
        }

        private void OpeningWidthTextBoxValidated(object sender, EventArgs e)
        {
            double openingWidth;
            if (ParseDouble(openingWidthTextBox.Text, out openingWidth))
            {
                data.OpeningWidth = openingWidth;
            }
        }

        private void SillWidthTextBoxValidated(object sender, EventArgs e)
        {
            double sillWidth;
            if (ParseDouble(sillWidthTextBox.Text, out sillWidth))
            {
                data.SillWidth = sillWidth;
            }
        }

        private void CheckBoxUseSillWidthCheckedChanged(object sender, EventArgs e)
        {
            if (handlingPropertyChanged || Data == null) return;

            if (checkBoxUseSillWidth.Checked && data.SillWidth <= 0.0)
            {
                data.SillWidth = data.Geometry.Length;
            }

            if (!checkBoxUseSillWidth.Checked && data.SillWidth > 0.0)
            {
                data.SillWidth = 0.0;
            }
        }

        #endregion Eventing

        #region Reusable

        private bool locked;
        public event EventHandler LockedChanged;

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

        #endregion Reusable
    }
}
