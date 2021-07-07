using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common.Gui.MapLayers;

namespace DeltaShell.NGHS.Common.Gui.PropertyGrid
{
    public class WmtsLayerProperties : ObjectProperties<WmtsLayer>
    {
        /// <summary>
        /// Expose supported schemas for the current layer (used by WtmsLayerSrsTypeEditor)
        /// </summary>
        [Browsable(false)]
        public IEnumerable<string> SupportedEpsgs
        {
            get
            {
                return data.TileSources.Select(t => t.Schema.Srs);
            }
        }

        /// <summary>
        /// Epsg code (schema.Src) of the <see cref="WmtsLayer.SelectedTileSource"/>
        /// </summary>
        [Editor(typeof(WtmsLayerSrsTypeEditor), typeof(UITypeEditor))]
        [Description("Current projection of the tile layer")]
        public string Epsg
        {
            get
            {
                return data.SelectedTileSource?.Schema?.Srs;
            }
            set
            {
                data.SelectedTileSource = data.TileSources.FirstOrDefault(t => t.Schema.Srs == value);
            }
        }
    }
}