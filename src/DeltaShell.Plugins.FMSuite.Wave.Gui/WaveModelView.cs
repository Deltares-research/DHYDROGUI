using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui;
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
            }
        }

        private WaveModel data;
        private IGui gui;

        public object Data
        {
            get { return data; }
            set
            {
                data = (WaveModel)value;

                if (data != null)
                {
//                    ((INotifyPropertyChanged)data).PropertyChanged += OnModelPropertyChanged;

                    Text = "Settings: " + data.Name;
                }
            }
        }

        public Image Image { get; set; }
        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }
        public void SwitchToTab(string tabTitle)
        {
           
        }

        public IEnumerable<IFeature> SelectedFeatures { get; set; }
        public event EventHandler SelectedFeaturesChanged;
        public ILayer Layer { get; set; }
        public void OnActivated() { }
        public void OnDeactivated() { }
    }
}
