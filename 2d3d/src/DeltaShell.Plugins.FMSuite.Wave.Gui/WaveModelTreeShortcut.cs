using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.Gui;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    /// <summary>
    /// <see cref="WaveModelTreeShortcut"/> implements a TreeView short cut for
    /// wave model properties.
    /// </summary>
    /// <seealso cref="ModelTreeShortcut"/>
    public class WaveModelTreeShortcut : ModelTreeShortcut
    {
        /// <summary>
        /// Creates a new <see cref="WaveModelTreeShortcut"/>.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="image">The image.</param>
        /// <param name="model">The model.</param>
        /// <param name="value">The data values.</param>
        /// <param name="shortCutType">Short type of the cut.</param>
        /// <param name="childObjects">The child objects.</param>
        public WaveModelTreeShortcut(string text,
                                     Bitmap image,
                                     WaveModel model,
                                     object value,
                                     ShortCutType shortCutType = ShortCutType.SettingsTab,
                                     IEnumerable<object> childObjects = null)
            : base(text, image, model, value, shortCutType, childObjects) {}

        /// <summary>
        /// Gets the wave model.
        /// </summary>
        /// <value>
        /// The wave model.
        /// </value>
        public WaveModel WaveModel => (WaveModel) Model;
    }
}