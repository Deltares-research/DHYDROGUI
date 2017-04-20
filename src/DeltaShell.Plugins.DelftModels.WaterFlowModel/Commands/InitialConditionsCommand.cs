/*
 * 
 * THIS DOES NOT NEED TO BE HERE, DEPENDENCY ON GIS PLUGINS ARE NOT ALLOWED!
 * 
using DeltaShell.Plugins.NetworkEditor.Layers;
using DeltaShell.Plugins.SharpMapGis.Forms;
using DelftTools.Controls;
using SharpMap;
using SharpMap.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Commands
{
    class InitialConditionsCommand : Command
    {
        protected MapView MapView
        {
            get
            {
                IView activeView = WaterFlowModel1DGuiPlugin.Input.Gui.DocumentViews.ActiveView;
                return activeView as MapView;
            }
        }

        protected override void OnExecute(params object[] arguments)
        {
            MapView.MapControl.ActivateTool(CurrentTool);
        }

        public override bool Enabled
        {
            get
            {
                return GetCurrentNetworkMapLayer() != null;
            }
        }

        protected HydroNetworkMapLayer GetCurrentNetworkMapLayer()
        {
            if (MapView == null)
            {
                return null;
            }

            Map map = (Map)MapView.Data;

            foreach (ILayer layer in map.Layers)
            {
                if (layer is HydroNetworkMapLayer)
                {
                    return (HydroNetworkMapLayer)layer;
                }
            }

            return null;
        }

        protected IMapTool CurrentTool
        {
            get
            {
                return MapView.MapControl.GetToolByName("Add Initial Conditions");
            }
        }

    }
}*/