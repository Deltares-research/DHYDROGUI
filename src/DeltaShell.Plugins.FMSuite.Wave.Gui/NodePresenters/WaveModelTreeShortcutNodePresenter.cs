using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters
{
    public class WaveModelTreeShortcutNodePresenter : ModelTreeShortcutNodePresenterBase<WaveModelTreeShortcut>
    {
        protected override void OpenGridEditor(WaveModelTreeShortcut shortcut)
        {
            var waveModel = shortcut.WaveModel;
            var o = shortcut.Data as CurvilinearGrid;
            if (waveModel == null || !o.IsEditable) return;
            
            var waveDomainData = WaveDomainHelper.GetAllDomains(waveModel.OuterDomain).First(d => Equals(d.Grid, o));

            WaveGridEditor.LaunchGridEditor(waveModel, waveDomainData);

            var centralMap = GetProjectItemMapView(shortcut);
            if (centralMap == null) return;

            // grid has been replaced, resolve through domain:
            var layer = centralMap.MapView.GetLayerForData(waveDomainData.Grid);
            if (layer == null) return;

            layer.Map.ZoomToExtents();
        }
    }
}