using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.DataEditorGenerator;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors.Buttons;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    public partial class WaveModelView : UserControl, ILayerEditorView, ITabbedModelView
    {
        public WaveModelView()
        {
            InitializeComponent();
        }

        public IGui Gui
        {
            get { return gui; }
            set
            {
                gui = value;
                if (coordinateSystemButton != null)
                {
                    coordinateSystemButton.Gui = Gui;
                }
            }
        }

        private WaveModel data;
        private DataEditor generatedEditor;
        private SetCoordinateSystemButton coordinateSystemButton;
        private IGui gui;

        public object Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    ((INotifyPropertyChanged)data).PropertyChanged -= OnModelPropertyChanged;
                    Controls.Remove(generatedEditor);
                    generatedEditor.Data = null;
                    generatedEditor.Dispose();
                    generatedEditor = null;
                }

                data = (WaveModel)value;

                if (data != null)
                {
                    ((INotifyPropertyChanged)data).PropertyChanged += OnModelPropertyChanged;

                    Text = "Settings: " + data.Name;

                    var objectDescription = WaveModelUIDescription.Extract(data);

                    // coordinate sys. button:
                    coordinateSystemButton = new SetCoordinateSystemButton()
                    {
                        CoordinateSystemFilter = WaveModel.IsValidCoordinateSystem,
                        Gui = Gui
                    };
                    var setCoordinateSystemBtn = new FieldUIDescription(d => this, null)
                                {
                                    Category = KnownWaveCategories.GeneralCategory,
                                    SubCategory = "Settings",
                                    CustomControlHelper = coordinateSystemButton
                                };
                    
                    // flow coupling checkbox:
                    var flowCouplingCheckBox = new FieldUIDescription(o => data.IsCoupledToFlow,
                        (d, v) => data.IsCoupledToFlow = (bool) v)
                    {
                        Category = KnownWaveCategories.GeneralCategory,
                        SubCategory = "Hydrodynamics",
                        Label = "Coupled to DFlowFM",
                        Name = "IsCoupledToFlow",
                        ValueType = typeof(bool),
                        ToolTip = "When enabled, run coupled to DFlowFM core"
                    };

                    var startTime = new FieldUIDescription(o => data.StartTime, (d, v) => data.StartTime = (DateTime)v,
                        (d) => data.IsCoupledToFlow)
                    {
                        Category = KnownWaveCategories.GeneralCategory,
                        SubCategory = "Coupling time frame",
                        Label = "Start time",
                        Name = "StartTime",
                        ValueType = typeof(DateTime),
                        ToolTip = "Start time within the coupled model run"
                    };

                    var stopTime = new FieldUIDescription(o => data.StopTime, (d, v) => data.StopTime = (DateTime)v,
                        (d) => data.IsCoupledToFlow)
                    {
                        Category = KnownWaveCategories.GeneralCategory,
                        SubCategory = "Coupling time frame",
                        Label = "Stop time",
                        Name = "StopTime",
                        ValueType = typeof(DateTime),
                        ToolTip = "Stop time within the coupled model run"
                    };

                    var timeStep = new FieldUIDescription(o => data.TimeStep, (d, v) => data.TimeStep = (TimeSpan) v,
                        (d) => data.IsCoupledToFlow)
                    {
                        Category = KnownWaveCategories.GeneralCategory,
                        SubCategory = "Coupling time frame",
                        Label = "Timestep",
                        Name = "TimeStep",
                        ValueType = typeof (TimeSpan),
                        ToolTip = "Coupling time step"
                    };

                    objectDescription.FieldDescriptions =
                        new[] { setCoordinateSystemBtn, flowCouplingCheckBox, startTime, stopTime, timeStep }.Concat(objectDescription.FieldDescriptions)
                                                      .ToList();

                    generatedEditor = DataEditorGeneratorSwf.GenerateView(objectDescription);

                    // for all floating point values: use culture invariant representation:
                    foreach (
                        var source in
                            generatedEditor.Bindings.Where(b => b.FieldDescription.ValueType.IsNumericalType()))
                    {
                        source.FieldDescription.Culture = CultureInfo.InvariantCulture;
                    }

                    generatedEditor.Dock = DockStyle.Fill;
                    generatedEditor.Data = data;
                    Controls.Add(generatedEditor);
                }
            }
        }

        private static readonly string CoordinateSystemPropertyName = TypeUtils.GetMemberName<WaveModel>(m => m.CoordinateSystem);
        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == CoordinateSystemPropertyName && coordinateSystemButton != null)
                coordinateSystemButton.UpdateLabelText();
        }

        public Image Image { get; set; }
        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }
        public IEnumerable<IFeature> SelectedFeatures { get; set; }
        public event EventHandler SelectedFeaturesChanged;
        public ILayer Layer { get; set; }
        public void OnActivated() { }
        public void OnDeactivated() { }

        public void SwitchToTab(string tabTitle)
        {
            var tabControl = generatedEditor.Controls.OfType<TabControl>().First();
            foreach (TabPage tab in tabControl.TabPages)
            {
                if (tab.Text.Equals(tabTitle))
                {
                    tabControl.SelectTab(tab);
                    return;
                }
            }
        }
    }
}
