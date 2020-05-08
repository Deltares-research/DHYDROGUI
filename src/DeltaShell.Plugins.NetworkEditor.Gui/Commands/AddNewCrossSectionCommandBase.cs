using System;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public abstract class AddNewCrossSectionCommandBase : NetworkEditorCommand
    {
        public override bool Checked
        {
            get
            {
                bool commandIsActive = this == ActiveAddNewCrossSectionCommand;

                return MapView != null && CurrentTool != null && CurrentTool.IsActive && commandIsActive;
            }
        }

        protected abstract Cursor Cursor { get; }

        /// <summary>
        /// Creation delegate to be inserted in the layer when this command is activated
        /// </summary>
        //protected abstract Func<IFeature> CreateNew { get; }

        /// <summary>
        /// Name of the maptool associated with this command
        /// </summary>
        protected abstract IMapTool CurrentTool { get; }

        protected Func<ILayer, IFeature> CreateNew
        {
            get
            {
                return CreateDefault;
            }
        }

        protected override void OnExecute(params object[] arguments)
        {
            ActiveAddNewCrossSectionCommand = this;
            //activate the crossection map tool
            MapControl mapControl = MapView.MapControl;

            //set the geotype. HACK #1
            HydroRegionMapLayer hydroRegionMapLayer = mapControl.Map.GetAllLayers(true).OfType<HydroRegionMapLayer>().FirstOrDefault(l => l.Region is IHydroNetwork);
            if (hydroRegionMapLayer == null)
            {
                return;
            }

            VectorLayer crossSectionLayer = hydroRegionMapLayer.Layers.OfType<VectorLayer>().FirstOrDefault(l => l.DataSource.FeatureType == typeof(CrossSection));
            crossSectionLayer.FeatureEditor.CreateNewFeature = CreateNew;

            var maptool = (MapTool) CurrentTool;
            maptool.Cursor = Cursor;

            mapControl.ActivateTool(CurrentTool);
        }

        protected abstract ICrossSection CreateDefault(ILayer layer);
        private static AddNewCrossSectionCommandBase ActiveAddNewCrossSectionCommand { get; set; }
    }
}