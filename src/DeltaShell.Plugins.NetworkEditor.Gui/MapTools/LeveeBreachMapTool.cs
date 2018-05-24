using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using GeoAPI.Geometries;
using log4net.Core;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public class LeveeBreachMapTool: Feature2DLineTool
    {
        public LeveeBreachMapTool(string targetLayerName, string name, Bitmap icon) : base(targetLayerName, name, icon)
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
