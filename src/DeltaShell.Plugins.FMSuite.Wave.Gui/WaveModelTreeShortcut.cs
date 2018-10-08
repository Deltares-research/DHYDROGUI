using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.Gui;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    public class WaveModelTreeShortcut : ModelTreeShortcut
    {
        public WaveModelTreeShortcut(string text, Bitmap image, WaveModel model, object data, ShortCutType shortCutType = ShortCutType.SettingsTab, IEnumerable<object> childObjects = null)
            : base(text, image, model, data, shortCutType, childObjects)
        {
        }

        public WaveModel WaveModel
        {
            get { return (WaveModel)Model; }
        }
    }
}