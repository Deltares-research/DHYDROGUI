using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using SharpMap.Api.Layers;
using SharpMap.UI.Helpers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public abstract class AddNewCatchmentCommand : Command
    {
        private readonly CatchmentType catchmentType;

        private AddNewCatchmentCommand(CatchmentType catchmentType)
        {
            this.catchmentType = catchmentType;
        }

        public override bool Checked
        {
            get
            {
                var featureEditor = FeatureEditors.FirstOrDefault();
                return featureEditor != null && featureEditor.NewCatchmentType == catchmentType && GetCurrentTool().IsActive;
            }
        }

        public override bool Enabled
        {
            get { return FeatureEditors.Any(); }
        }

        private IEnumerable<CatchmentFeatureEditor> FeatureEditors
        {
            get
            {
                var mapView = GetMapView();
                if (mapView == null || mapView.Map == null || GetCurrentTool() == null)
                {
                    return Enumerable.Empty<CatchmentFeatureEditor>();
                }

                var regionMapLayers = mapView.Map.GetAllVisibleLayers(true).OfType<HydroRegionMapLayer>().Where(l => l.Region is IDrainageBasin);
                var selectMany = regionMapLayers.SelectMany(GetSubLayersRecursive);
                var catchmentFeatureEditors = selectMany.Select(l => l.FeatureEditor)
                                                        .Where(e => e != null)
                                                        .OfType<CatchmentFeatureEditor>();

                return catchmentFeatureEditors;
            }
        }

        private IEnumerable<ILayer> GetSubLayersRecursive(ILayer layer)
        {
            var groupLayer = layer as IGroupLayer;
            if (groupLayer != null)
            {
                foreach (var subLayer in groupLayer.Layers.SelectMany(GetSubLayersRecursive))
                {
                    yield return subLayer;
                }
            }

            yield return layer;
        }

        protected override void OnExecute(params object[] arguments)
        {
            var newLineTool = (NewLineTool) GetCurrentTool();
            newLineTool.Cursor = MapCursors.CreateArrowOverlayCuror(catchmentType.Icon);
            GetMapView().MapControl.ActivateTool(newLineTool);

            // setup feature editors
            foreach (var featureEditor in FeatureEditors)
            {
                featureEditor.NewCatchmentType = catchmentType;
            }
        }

        private static MapView GetMapView()
        {
            return NetworkEditorGuiPlugin.GetFocusedMapView();
        }
        
        private static IMapTool GetCurrentTool()
        {
            return GetMapView().MapControl.GetToolByName(HydroRegionEditorMapTool.AddCatchmentToolName);
        }

        public class AddNewUnpavedCommand : AddNewCatchmentCommand
        {
            public AddNewUnpavedCommand(): base(CatchmentType.Unpaved) {}
        }

        public class AddNewPavedCommand : AddNewCatchmentCommand
        {
            public AddNewPavedCommand() : base(CatchmentType.Paved) {}
        }

        public class AddNewGreenHouseCommand : AddNewCatchmentCommand
        {
            public AddNewGreenHouseCommand() : base(CatchmentType.GreenHouse) {}
        }

        public class AddNewOpenWaterCommand : AddNewCatchmentCommand
        {
            public AddNewOpenWaterCommand() : base(CatchmentType.OpenWater) {}
        }

        public class AddNewSacramentoCommand: AddNewCatchmentCommand
        {
            public AddNewSacramentoCommand() : base(CatchmentType.Sacramento) { }
        }

        public class AddNewHbvCommand: AddNewCatchmentCommand
        {
            public AddNewHbvCommand() : base(CatchmentType.Hbv) { }
        }
    }
}