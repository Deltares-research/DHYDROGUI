using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.Gui;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    public class WaveTreeShortcut : TreeShortcut<WaveModel,WaveModelView>
    {
        public WaveTreeShortcut(string text, Bitmap image, WaveModel model, object data = null, IEnumerable<object> subItems = null)
            : base(text, image, model, data, subItems)
        {
        }
    }
}
