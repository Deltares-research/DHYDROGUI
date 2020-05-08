using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.HydroNetworkEditor.Gui.Editors
{
    public class FMGatePropertiesRow : GatePropertiesRow
    {
        public FMGatePropertiesRow(IGate gate)
            : base(gate)
        {
            ((INotifyPropertyChanged) gate).PropertyChanged += OnPropertyChanged;
            UpdateTimeSerieStrings();
        }

        [Browsable(false)] // Hide it
        public override IBranch Branch
        {
            get
            {
                return base.Branch;
            }
        }

        [Browsable(false)] // Hide it
        public override double Chainage
        {
            get
            {
                return base.Chainage;
            }
        }

        [Browsable(false)] // Hide it, is not part of 3Di file format and thus not saved.
        public override string LongName
        {
            get
            {
                return base.LongName;
            }
            set
            {
                base.LongName = value;
            }
        }

        public override void Dispose()
        {
            ((INotifyPropertyChanged) gate).PropertyChanged -= OnPropertyChanged;
            base.Dispose();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                UpdateTimeSerieStrings();
            }
        }

        private void UpdateTimeSerieStrings()
        {
            OpeningWidthTimeSeriesString = string.Format("{0}_{1}.tim", gate.Name, KnownStructureProperties.GateOpeningWidth);
            TimeSeriesString = string.Format("{0}_{1}.tim", gate.Name, KnownStructureProperties.GateLowerEdgeLevel);
        }
    }
}