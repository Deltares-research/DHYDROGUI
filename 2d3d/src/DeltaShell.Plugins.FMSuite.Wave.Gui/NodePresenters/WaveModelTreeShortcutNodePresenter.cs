using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters
{
    public class WaveModelTreeShortcutNodePresenter : ModelTreeShortcutNodePresenterBase<WaveModelTreeShortcut>
    {
        protected override void OpenGridEditor(WaveModelTreeShortcut shortcut)
        {
            WaveModel waveModel = shortcut.WaveModel;
            var o = shortcut.Value as CurvilinearGrid;
            if (waveModel == null || !o.IsEditable)
            {
                return;
            }

            IWaveDomainData waveDomainData =
                WaveDomainHelper.GetAllDomains(waveModel.OuterDomain).FirstOrDefault(d => Equals(d.Grid, o));

            if (waveDomainData is null)
            {
                return;
            }

            WaveGridEditor.LaunchGridEditor(waveModel, waveDomainData);

            ProjectItemMapView centralMap = GetProjectItemMapView(shortcut);
            if (centralMap == null)
            {
                return;
            }

            // grid has been replaced, resolve through domain:
            ILayer layer = centralMap.MapView.GetLayerForData(waveDomainData.Grid);
            if (layer == null)
            {
                return;
            }

            layer.Map.ZoomToExtents();
        }
    }
}