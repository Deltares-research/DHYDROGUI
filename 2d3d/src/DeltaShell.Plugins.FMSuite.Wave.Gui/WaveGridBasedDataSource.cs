using System.ComponentModel;
using DelftTools.Utils;
using GeoAPI.Extensions.Coverages;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    /// <summary>
    /// Fake data source: no features at all, the grid is the data..
    /// </summary>
    public class WaveGridBasedDataSource : FeatureCollection
    {
        private readonly IDiscreteGridPointCoverage grid;

        public WaveGridBasedDataSource(IDiscreteGridPointCoverage grid)
        {
            this.grid = grid;
            FeatureType = grid.GetType();
            ((INotifyPropertyChange) grid).PropertyChanged += OnGridPropertyChanged;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && grid != null)
            {
                ((INotifyPropertyChange) grid).PropertyChanged -= OnGridPropertyChanged;
            }
        }

        private void OnGridPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (grid.IsEditing)
            {
                return;
            }

            FireFeaturesChanged();
        }
    }
}