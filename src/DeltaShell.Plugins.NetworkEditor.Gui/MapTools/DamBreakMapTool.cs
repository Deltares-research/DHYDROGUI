using System;
using System.Drawing;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using GeoAPI.Geometries;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public class DamBreakMapTool: Feature2DLineTool
    {
        public DamBreakMapTool(string targetLayerName, string name, Bitmap icon) : base(targetLayerName, name, icon)
        {
        }

        public override void Execute()
        {
            base.Execute();

            var featureCount = VectorLayer.DataSource.GetFeatureCount();

            var feature = featureCount > 0
                ? VectorLayer.DataSource.GetFeature(featureCount - 1)
                : null;
        }

    }
}
