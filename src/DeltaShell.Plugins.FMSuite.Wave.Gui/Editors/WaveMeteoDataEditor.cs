using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils;
using DelftTools.Utils.Binding;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Common.Wind;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    public partial class WaveMeteoDataEditor : UserControl
    {
        private readonly MeteoFileSelectionControl spiderWebFileControl = new MeteoFileSelectionControl
        {
            LabelText = "Spider web:",
            FileFilter = "spider web (*.spw)|*.spw"
        };

        private readonly MeteoFileSelectionControl xComponentFileControl = new MeteoFileSelectionControl
        {
            LabelText = "X component:",
            FileFilter = "uniform x series (*.wnd;*.amu)|*.wnd;*.amu"
        };

        private readonly MeteoFileSelectionControl yComponentFileControl = new MeteoFileSelectionControl
        {
            LabelText = "Y component:",
            FileFilter = "uniform y series (*.wnd;*.amv)|*.wnd;*.amv"
        };

        private readonly MeteoFileSelectionControl xyVectorFileControl = new MeteoFileSelectionControl
        {
            LabelText = "Wind velocity:",
            FileFilter = "uniform xy series (*.wnd)|*.wnd"
        };

        private WaveMeteoData meteoData;

        private Func<string, string> importIntoModelDirectory;

        public WaveMeteoDataEditor()
        {
            InitializeComponent();

            // without pressure
            inputTypeCBox.DataSource = EnumBindingHelper
                                       .ToList<WindDefinitionType>().Where(k => k.Key != WindDefinitionType.WindXYP)
                                       .ToList();
            inputTypeCBox.DisplayMember = "Value";
            inputTypeCBox.ValueMember = "Key";
        }

        public WaveMeteoData Data
        {
            get => meteoData;
            set
            {
                if (meteoData != null)
                {
                    ((INotifyPropertyChange) meteoData).PropertyChanged -= OnMeteoDataPropertyChanged;
                    Unbind();
                }

                meteoData = value;

                if (meteoData != null)
                {
                    ((INotifyPropertyChange) meteoData).PropertyChanged += OnMeteoDataPropertyChanged;
                    Bind();
                }

                UpdatePanel();
            }
        }

        public Func<string, string> ImportFileIntoModelDirectory
        {
            set
            {
                importIntoModelDirectory = value;
                xyVectorFileControl.ImportIntoDirectory = importIntoModelDirectory;
                xComponentFileControl.ImportIntoDirectory = importIntoModelDirectory;
                yComponentFileControl.ImportIntoDirectory = importIntoModelDirectory;
                spiderWebFileControl.ImportIntoDirectory = importIntoModelDirectory;
            }
        }

        private void OnMeteoDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdatePanel();
        }

        private void Bind()
        {
            inputTypeCBox.DataBindings.Add(new Binding(nameof(inputTypeCBox.SelectedValue), 
                                                       meteoData,
                                                       nameof(WaveMeteoData.FileType), 
                                                       false,
                                                       DataSourceUpdateMode.OnPropertyChanged));
            xyVectorFileControl.DataBindings.Add(new Binding(nameof(xyVectorFileControl.FileName), 
                                                             meteoData,
                                                             nameof(WaveMeteoData.XYVectorFilePath), 
                                                             false,
                                                             DataSourceUpdateMode.OnPropertyChanged));
            xComponentFileControl.DataBindings.Add(new Binding(nameof(xComponentFileControl.FileName), 
                                                               meteoData,
                                                               nameof(WaveMeteoData.XComponentFilePath), 
                                                               false,
                                                               DataSourceUpdateMode.OnPropertyChanged));
            yComponentFileControl.DataBindings.Add(new Binding(nameof(yComponentFileControl.FileName), 
                                                               meteoData,
                                                               nameof(WaveMeteoData.YComponentFilePath), 
                                                               false,
                                                               DataSourceUpdateMode.OnPropertyChanged));
            spiderWebFileControl.DataBindings.Add(new Binding(nameof(spiderWebFileControl.FileName), 
                                                              meteoData,
                                                              nameof(WaveMeteoData.SpiderWebFilePath), 
                                                              false,
                                                              DataSourceUpdateMode.OnPropertyChanged));
        }

        private void Unbind()
        {
            inputTypeCBox.DataBindings.Clear();
            xyVectorFileControl.DataBindings.Clear();
            xComponentFileControl.DataBindings.Clear();
            yComponentFileControl.DataBindings.Clear();
            spiderWebFileControl.DataBindings.Clear();
        }

        private void UpdatePanel()
        {
            flowLayoutPanel2.Controls.Clear();
            if (meteoData == null)
            {
                return;
            }

            spwButton.Image = Resources.hurricane2;
            spwButton.Enabled = true;

            switch (meteoData.FileType)
            {
                case WindDefinitionType.WindXY:
                    flowLayoutPanel2.Controls.Add(xyVectorFileControl);
                    break;
                case WindDefinitionType.WindXWindY:
                    flowLayoutPanel2.Controls.Add(xComponentFileControl);
                    flowLayoutPanel2.Controls.Add(yComponentFileControl);
                    break;
                case WindDefinitionType.SpiderWebGrid:
                    flowLayoutPanel2.Controls.Add(spiderWebFileControl);
                    spwButton.Enabled = false;
                    break;
                default:
                    throw new ArgumentException("Invalid wind definition type");
            }

            if (meteoData.HasSpiderWeb && meteoData.FileType != WindDefinitionType.SpiderWebGrid)
            {
                spwButton.Image = Resources.hurricane_del;
                flowLayoutPanel2.Controls.Add(spiderWebFileControl);
            }
        }

        private void spwButton_Click(object sender, EventArgs e)
        {
            meteoData.SpiderWebFilePath = "";
            meteoData.HasSpiderWeb = !meteoData.HasSpiderWeb;
            UpdatePanel();
        }
    }
}