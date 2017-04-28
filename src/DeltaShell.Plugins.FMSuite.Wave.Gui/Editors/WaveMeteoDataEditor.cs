using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils;
using DelftTools.Utils.Binding;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    public partial class WaveMeteoDataEditor : UserControl
    {
        private WaveMeteoData meteoData;

        public WaveMeteoDataEditor()
        {
            InitializeComponent();

            // without pressure
            inputTypeCBox.DataSource = EnumBindingHelper.ToList<WindDefinitionType>().Where(k => k.Key != WindDefinitionType.WindXYP).ToList();
            inputTypeCBox.DisplayMember = "Value";
            inputTypeCBox.ValueMember = "Key";
        }

        public WaveMeteoData Data
        {
            get { return meteoData; }
            set
            {
                if (meteoData != null)
                {
                    ((INotifyPropertyChange)meteoData).PropertyChanged -= OnMeteoDataPropertyChanged;
                    Unbind();
                }

                meteoData = value;

                if (meteoData != null)
                {
                    ((INotifyPropertyChange)meteoData).PropertyChanged += OnMeteoDataPropertyChanged;
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
            inputTypeCBox.DataBindings.Add(new Binding("SelectedValue", meteoData,
                TypeUtils.GetMemberName<WaveMeteoData>(m => m.FileType), false, DataSourceUpdateMode.OnPropertyChanged));
            xyVectorFileControl.DataBindings.Add(new Binding("FileName", meteoData,
                TypeUtils.GetMemberName<WaveMeteoData>(m => m.XYVectorFileName), false,
                DataSourceUpdateMode.OnPropertyChanged));
            xComponentFileControl.DataBindings.Add(new Binding("FileName", meteoData,
                TypeUtils.GetMemberName<WaveMeteoData>(m => m.XComponentFileName), false,
                DataSourceUpdateMode.OnPropertyChanged));
            yComponentFileControl.DataBindings.Add(new Binding("FileName", meteoData,
                TypeUtils.GetMemberName<WaveMeteoData>(m => m.YComponentFileName), false,
                DataSourceUpdateMode.OnPropertyChanged));
            spiderWebFileControl.DataBindings.Add(new Binding("FileName", meteoData,
                TypeUtils.GetMemberName<WaveMeteoData>(m => m.SpiderWebFileName), false,
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
            if (meteoData == null) return;

            spwButton.Image = Common.Gui.Properties.Resources.hurricane2;
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
                spwButton.Image = Common.Gui.Properties.Resources.hurricane_del;
                flowLayoutPanel2.Controls.Add(spiderWebFileControl);
            }
        }

        private readonly MeteoFileSelectionControl spiderWebFileControl = new MeteoFileSelectionControl
        {
            LabelText = "Spider web:",
            FileFilter = "spider web (*.spw)|*.spw"
        };
        private readonly MeteoFileSelectionControl xComponentFileControl = new MeteoFileSelectionControl
        {
            LabelText = "X component:",
            FileFilter = "uniform x series (*.wnd)|*.wnd"
        };
        private readonly MeteoFileSelectionControl yComponentFileControl = new MeteoFileSelectionControl
        {
            LabelText = "Y component:",
            FileFilter = "uniform y series (*.wnd)|*.wnd"
        };
        private readonly MeteoFileSelectionControl xyVectorFileControl = new MeteoFileSelectionControl
        {
            LabelText = "Wind velocity:",
            FileFilter = "uniform xy series (*.wnd)|*.wnd"
        };

        private Func<string, string> importIntoModelDirectory;

        private void spwButton_Click(object sender, EventArgs e)
        {
            meteoData.SpiderWebFileName = "";
            meteoData.HasSpiderWeb = !meteoData.HasSpiderWeb;
            UpdatePanel();
        }
    }
}
