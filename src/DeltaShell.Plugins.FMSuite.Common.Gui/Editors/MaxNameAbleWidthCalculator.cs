using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Utils;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    /// <summary>
    /// Class which calculates the maximum width of the Nameables passed into it.
    /// </summary>
    public class MaxNameableWidthCalculator
    {
        /// <summary>
        /// Method to retrieve the maxWidth of nameable items.
        /// The method retrieves the text size based on the font and calculate the width of an item.
        /// The largest width is returned.
        /// </summary>
        /// <param name="items">Nameable items for which the max text width is to be retrieved.</param>
        /// <param name="font">Font used for calculating the text width.</param>
        /// <returns>The maximum item width, when <paramref name="items"/> is empty will return <c>0</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> or <paramref name="font"/> is null.</exception>
        public int GetMaxNameableWidth(IEnumerable<INameable> items, Font font)
        {
            Ensure.NotNull(items, nameof(items));
            Ensure.NotNull(font, nameof(font));

            var maxItemWidth = 0;

            foreach (INameable item in items)
            {
                int textWidth = TextRenderer.MeasureText(item.Name, font).Width;
                maxItemWidth = Math.Max(maxItemWidth, textWidth);
            }

            return maxItemWidth;
        }
    }
}