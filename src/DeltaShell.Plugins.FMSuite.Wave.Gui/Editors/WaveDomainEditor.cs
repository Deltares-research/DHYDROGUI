using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Binding;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    public partial class WaveDomainEditor : UserControl, ILayerEditorView
    {
        public WaveDomainEditor()
        {
            InitializeComponent();

            circleRBtn.CheckedChanged += CircleRBtnOnCheckedChanged;

            bedlevelBox.DataSource = EnumBindingHelper.ToList<UsageFromFlowType>();
            bedlevelBox.DisplayMember = "Value";
            bedlevelBox.ValueMember = "Key";
            waterlevelBox.DataSource = EnumBindingHelper.ToList<UsageFromFlowType>();
            waterlevelBox.DisplayMember = "Value";
            waterlevelBox.ValueMember = "Key";
            velocityBox.DataSource = EnumBindingHelper.ToList<UsageFromFlowType>();
            velocityBox.DisplayMember = "Value";
            velocityBox.ValueMember = "Key";
            velocityTypeBox.DataSource = EnumBindingHelper.ToList<VelocityComputationType>();
            velocityTypeBox.DisplayMember = "Value";
            velocityTypeBox.ValueMember = "Key";
            windBox.DataSource = EnumBindingHelper.ToList<UsageFromFlowType>();
            windBox.DisplayMember = "Value";
            windBox.ValueMember = "Key";

            useDefaultDirSpaceCBox.CheckedChanged += UseDefaultDirSpaceCBoxOnCheckedChanged;
            useDefaultFreqSpaceCBox.CheckedChanged += UseDefaultFreqSpaceCBoxOnCheckedChanged;
            useDefaultMeteoCBox.CheckedChanged += UseDefaultMeteoCBoxOnCheckedChanged;
            useDefaultHydroCBox.CheckedChanged += UseDefaultHydroCBoxOnCheckedChanged;
        }

        private void UseDefaultMeteoCBoxOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            waveMeteoPanel.Visible = !useDefaultMeteoCBox.Checked;
        }

        private void UseDefaultHydroCBoxOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            hydroPanel.Visible = !useDefaultHydroCBox.Checked;
        }

        private void UseDefaultFreqSpaceCBoxOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            frequencyPanel.Visible = !useDefaultFreqSpaceCBox.Checked;
        }

        private void UseDefaultDirSpaceCBoxOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            directionalPanel.Visible = !useDefaultDirSpaceCBox.Checked;
        }

        public bool IsCoupledToFlow
        {
            set => hydroGroupBox.Visible = value;
        }

        private WaveDomainData data;

        public object Data
        {
            get => data;
            set
            {
                if (data != null)
                {
                    UnbindControls();
                }

                data = value as WaveDomainData;
                waveMeteoPanel.Data = data != null ? data.MeteoData : null;
                waveMeteoPanel.ImportFileIntoModelDirectory = s => ImportIntoModelDirectory(s);
                if (data != null)
                {
                    BindControls();

                    circleRBtn.CheckedChanged -= CircleRBtnOnCheckedChanged;

                    circleRBtn.Checked =
                        data.SpectralDomainData.DirectionalSpaceType == WaveDirectionalSpaceType.Circle;
                    sectorRBtn.Checked =
                        data.SpectralDomainData.DirectionalSpaceType == WaveDirectionalSpaceType.Sector;
                    EnableControlsForSpectralSpaceType(data.SpectralDomainData.DirectionalSpaceType ==
                                                       WaveDirectionalSpaceType.Circle);

                    circleRBtn.CheckedChanged += CircleRBtnOnCheckedChanged;
                }
            }
        }

        public Func<string, string> ImportIntoModelDirectory { private get; set; }

        private void BindControls()
        {
            SpectralDomainData spectralDomain = data.SpectralDomainData;

            useDefaultDirSpaceCBox.DataBindings.Add(new Binding("Checked", spectralDomain,
                                                                nameof(spectralDomain.UseDefaultDirectionalSpace)));
            nDirBox.DataBindings.Add(new Binding("Text", spectralDomain,
                                                 nameof(spectralDomain.NDir)));
            startDirBox.DataBindings.Add(new Binding("Text", spectralDomain,
                                                     nameof(spectralDomain.StartDir)));
            endDirBox.DataBindings.Add(new Binding("Text", spectralDomain,
                                                   nameof(spectralDomain.EndDir)));

            useDefaultFreqSpaceCBox.DataBindings.Add(new Binding("Checked", spectralDomain,
                                                                 nameof(spectralDomain.UseDefaultFrequencySpace)));
            nrOfFreqBox.DataBindings.Add(new Binding("Text", spectralDomain,
                                                     nameof(spectralDomain.NFreq)));
            lowFreqBox.DataBindings.Add(new Binding("Text", spectralDomain,
                                                    nameof(spectralDomain.FreqMin)));
            highFreqBox.DataBindings.Add(new Binding("Text", spectralDomain,
                                                     nameof(spectralDomain.FreqMax)));

            useDefaultMeteoCBox.DataBindings.Add(new Binding("Checked", data,
                                                             nameof(WaveDomainData.UseGlobalMeteoData)));

            HydroFromFlowSettings hydroData = data.HydroFromFlowData;
            useDefaultHydroCBox.DataBindings.Add(new Binding("Checked", hydroData,
                                                             nameof(hydroData.UseDefaultHydroFromFlowSettings)));
            bedlevelBox.DataBindings.Add(new Binding("SelectedValue", hydroData,
                                                     nameof(hydroData.BedLevelUsage)));
            waterlevelBox.DataBindings.Add(new Binding("SelectedValue", hydroData,
                                                       nameof(hydroData.WaterLevelUsage)));
            velocityBox.DataBindings.Add(new Binding("SelectedValue", hydroData,
                                                     nameof(hydroData.VelocityUsage)));
            velocityTypeBox.DataBindings.Add(new Binding("SelectedValue", hydroData,
                                                         nameof(hydroData.VelocityUsageType)));
            windBox.DataBindings.Add(new Binding("SelectedValue", hydroData,
                                                 nameof(hydroData.WindUsage)));
        }

        private void CircleRBtnOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            data.SpectralDomainData.DirectionalSpaceType = circleRBtn.Checked
                                                               ? WaveDirectionalSpaceType.Circle
                                                               : WaveDirectionalSpaceType.Sector;
            EnableControlsForSpectralSpaceType(circleRBtn.Checked);
        }

        private void EnableControlsForSpectralSpaceType(bool isCircle)
        {
            startDirBox.Enabled = startDirLabel.Enabled = sectorRBtn.Checked;
            endDirBox.Enabled = endDirLabel.Enabled = sectorRBtn.Checked;
        }

        private void UnbindControls()
        {
            nDirBox.DataBindings.Clear();
            startDirBox.DataBindings.Clear();
            endDirBox.DataBindings.Clear();

            nrOfFreqBox.DataBindings.Clear();
            lowFreqBox.DataBindings.Clear();
            highFreqBox.DataBindings.Clear();

            useDefaultMeteoCBox.DataBindings.Clear();

            bedlevelBox.DataBindings.Clear();
            waterlevelBox.DataBindings.Clear();
            velocityBox.DataBindings.Clear();
            velocityTypeBox.DataBindings.Clear();
            windBox.DataBindings.Clear();
        }

        public Image Image { get; set; }
        public void EnsureVisible(object item) {}
        public ViewInfo ViewInfo { get; set; }
        public IEnumerable<IFeature> SelectedFeatures { get; set; }
        public event EventHandler SelectedFeaturesChanged;
        public ILayer Layer { get; set; }
        public void OnActivated() {}
        public void OnDeactivated() {}
    }
}