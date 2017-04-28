using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    public class FlowFMTreeShortcut : TreeShortcut<WaterFlowFMModel,WaterFlowFMModelView>
    {
        public FlowFMTreeShortcut(string text, Bitmap image, WaterFlowFMModel model, object data = null, IEnumerable<object> subItems = null) 
            : base(text, image, model, data, subItems)
        {
        }
    }
}
