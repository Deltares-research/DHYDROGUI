using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers
{
    [Entity(FireOnCollectionChange = false)]
    public class HydroAreaLayer : HydroRegionMapLayer
    {
        private HydroArea hydroArea;
        private bool layersInitialized;

        public override IEventedList<ILayer> Layers
        {
            get
            {
                if (!layersInitialized)
                {
                    InitializeLayers();
                }

                return base.Layers;
            }
            set => base.Layers = value;
        }

        [Aggregation]
        public virtual HydroArea HydroArea
        {
            get => hydroArea;
            set
            {
                if (hydroArea != null && hydroArea.Equals(value))
                {
                    return;
                }

                hydroArea = value;
                Name = hydroArea.Name;
                layersInitialized = false;
                ShowInLegend = false;
            }
        }

        private void InitializeLayers()
        {
            if (hydroArea == null)
            {
                return;
            }

            layersInitialized = true; // set it here since it is accessed locally

            base.Name = hydroArea.Name;

            LayersReadOnly = true;
        }
    }
}