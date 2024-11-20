using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Ribbon
{
    [Entity]
    public class RibbonViewModelRegion
    {
        public RibbonViewModelRegion()
        {

            ActivateAddBoundaryToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.BoundaryToolName) {LayerType = typeof (AreaLayer)}
            };
            ActivateAddSourceSinkToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.SourceAndSinkToolName) { LayerType = typeof(AreaLayer) }
            }; 
            ActivateAddSourceToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.SourceToolName) { LayerType = typeof(AreaLayer) }
            }; 
            ActivateReverseLineToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.Reverse2DLineToolName) { LayerType = typeof(AreaLayer) }
            }; 
            ActivateGenerateEmbankmentsToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.GenerateEmbankmentsToolName) { LayerType = typeof(AreaLayer) }
            }; 
            ActivateMergeEmbankmentsToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.MergeEmbankmentsToolName) { LayerType = typeof(AreaLayer) }
            };
            ActivateGridWizardToolCommand = new RelayMapToolCommand
            {
                MapToolCommand = new MapToolCommand(FlowFMMapViewDecorator.GridWizardToolName) { LayerType = typeof(AreaLayer) }
            };

 
        }

        public RelayMapToolCommand ActivateAddBoundaryToolCommand { get; private set; }
        public RelayMapToolCommand ActivateAddSourceSinkToolCommand { get; private set; }
        public RelayMapToolCommand ActivateAddSourceToolCommand { get; private set; }
        public RelayMapToolCommand ActivateReverseLineToolCommand { get; private set; }
        public RelayMapToolCommand ActivateGenerateEmbankmentsToolCommand { get; private set; }  
        public RelayMapToolCommand ActivateMergeEmbankmentsToolCommand { get; private set; }
        public RelayMapToolCommand ActivateGridWizardToolCommand { get; private set; }

        public void RefreshButtons()
        {
            new[]
            {
                ActivateAddBoundaryToolCommand,
                ActivateAddSourceSinkToolCommand,
                ActivateAddSourceToolCommand,
                ActivateReverseLineToolCommand,
                ActivateGenerateEmbankmentsToolCommand,
                ActivateMergeEmbankmentsToolCommand,
                ActivateGridWizardToolCommand
            }.ForEach(c => c.Refresh());
        }
    }
}