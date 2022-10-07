using System;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Adapters;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.ViewModels
{
    public class MeteoEditorViewModel : BaseMeteoEditorViewModel<IMeteoData> {
        public MeteoEditorViewModel(IMeteoData meteoData,
                                       IMeteoStationsListViewModel meteoStationsListViewModel,
                                       ITableViewMeteoStationSelectionAdapter tableSelectionAdapter,
                                       ITimeDependentFunctionSplitter functionSplitter,
                                       DateTime modelStartTime, DateTime modelEndTime) : base(meteoData,
                                                                                              meteoStationsListViewModel,
                                                                                              tableSelectionAdapter,
                                                                                              functionSplitter,
                                                                                              modelStartTime,
                                                                                              modelEndTime)
        {
            RefreshTimeSeries();
        }
        
        #region IDisposable
        private bool disposed = false;
        
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            base.Dispose(disposing);

            disposed = true;
        }
        #endregion
    }
}