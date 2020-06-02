using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui
{
    public class FmModelTreeShortcut : ModelTreeShortcut
    {
        public FmModelTreeShortcut(string text, Bitmap image, WaterFlowFMModel model, object data, ShortCutType shortCutType = ShortCutType.SettingsTab, IEnumerable<object> childObjects = null)
            : base(text, image, model, data, shortCutType, childObjects) {}

        public WaterFlowFMModel FlowFmModel
        {
            get
            {
                return (WaterFlowFMModel) Model;
            }
        }
    }
}