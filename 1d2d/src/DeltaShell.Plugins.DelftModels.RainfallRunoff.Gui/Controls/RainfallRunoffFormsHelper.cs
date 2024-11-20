using System.Windows.Forms;
using DelftTools.Functions;
using DelftTools.Utils.Globalization;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public static class RainfallRunoffFormsHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (RainfallRunoffFormsHelper));

        public static void ShowTableEditor(UserControl parent, IFunction function)
        {
            if (function == null)
            {
                log.Error("Cannot open table: data null");
                return;
            }

            var generatePanel = new Panel {Dock = DockStyle.Top, Height = 30};
            var generateButton = new Button
                {Text = "Generate timeseries", Dock = DockStyle.Left, Width = 150, Tag = function};
            var functionView = new FunctionView {Dock = DockStyle.Fill, Data = function};
            
            var form = new Form
                {Text = "Edit " + function.Name, Width = 800, Height = 600, ShowIcon = false, ShowInTaskbar = false};

            generatePanel.Controls.Add(generateButton);

            form.Controls.Add(functionView);
            form.Controls.Add(generatePanel);

            generateButton.Click += (s, e) =>
                {
                    var btn = (Button) s;
                    var timeseries = btn.Tag as ITimeSeries;
                    if (timeseries != null)
                    {
                        var generateDialog = new TimeSeriesGeneratorDialog();
                        generateDialog.SetData(timeseries.Time);
                        functionView.Data = null;
                        generateDialog.ShowDialog(btn.TopLevelControl);
                        functionView.Data = timeseries;
                    }
                };

            form.ShowDialog(parent);
            functionView.Data = null;
            form.Dispose();
        }

        public static void ApplyRealNumberFormatToDataBinding(Control control)
        {
            foreach (Binding binding in control.DataBindings)
            {
                binding.FormatInfo = RegionalSettingsManager.CurrentCulture;
                binding.FormatString = RegionalSettingsManager.RealNumberFormat;
            }
            foreach (Control subControl in control.Controls)
            {
                ApplyRealNumberFormatToDataBinding(subControl);
            }
        }
    }
}