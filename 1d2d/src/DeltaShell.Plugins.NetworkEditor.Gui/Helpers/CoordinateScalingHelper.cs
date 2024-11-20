namespace DeltaShell.Plugins.NetworkEditor.Gui.Helpers
{
    /// <summary>
    /// A helper class to convert values from a regular (x,y) coordinate system, with (0,0) located at bottom left, y pointing upward and x pointing to the right 
    /// to a (x,y) coordinate system where (0,0) is located in the upper left corner, with y pointing downward and x pointing to the right. 
    /// Used for scaling for instance real world coordinates to screen pixels
    /// </summary>
    public static class CoordinateScalingHelper
    {
        /* Scaling from (x, y) to (x, y) coordinate system.
         * 
         * From:
         *         *------------------------*
         *         |                        |
         *         ^                        |
         *         |                        |
         *         *---->-------------------*
         *      (0,0)
         * 
         * To: 
         * 
         *      (0,0)
         *         *---->-------------------* 
         *         |                        |
         *         v                        |
         *         |                        |
         *         *------------------------*
         */

        /// <summary>
        /// Scales x-values from a regular (x (directed to the right), y (directed upward)) coordinate system to a (x (directed to the right), y (directed downward)) coordinate system
        /// </summary>
        /// <param name="x">x-value to scale</param>
        /// <param name="minX">minimum x value of the original x-value range</param>
        /// <param name="maxX">maximum x value of the original x-value range</param>
        /// <param name="targetWidth">width of the target x-value range</param>
        /// <returns>Scaled x-value</returns>
        public static double ScaleX(double x, double minX, double maxX, double targetWidth)
        {
            return (x - minX) / (maxX - minX) * targetWidth;
        }

        /// <summary>
        /// Scales y-values from a regular (x (directed to the right), y (directed upward)) coordinate system to a (x (directed to the right), y (directed downward)) coordinate system
        /// </summary>
        /// <param name="y">y-value to scale</param>
        /// <param name="minY">minimum y-value of the original y-value range</param>
        /// <param name="maxY">maximum y-value of the original y-value range</param>
        /// <param name="targetHeight">height of the target y-value range</param>
        /// <returns>Scaled y-value</returns>
        public static double ScaleY(double y, double minY, double maxY, double targetHeight)
        { 
            return targetHeight - (y - minY) / (maxY - minY) * targetHeight;
        }

        /// <summary>
        /// Scales a width in the original coordinate system to a width in the target coordinate system
        /// </summary>
        /// <param name="width">original width</param>
        /// <param name="minX">minium x-value of the original x-value range</param>
        /// <param name="maxX">maximum x-value of the original x-value range</param>
        /// <param name="targetWidth">width of the target x-value range</param>
        /// <returns>Scaled width</returns>
        public static double ScaleWidth(double width, double minX, double maxX, double targetWidth)
        {
            return width / (maxX - minX) * targetWidth;
        }

        /// <summary>
        /// Scales a height from the original coordinate system to a height in the target coordinate system
        /// </summary>
        /// <param name="height">original height</param>
        /// <param name="minY">minimum y-value of the original y-value range</param>
        /// <param name="maxY">maximum y-value of the original y-value range</param>
        /// <param name="targetHeight">height of the target y-value range</param>
        /// <returns>Scaled height</returns>
        public static double ScaleHeight(double height, double minY, double maxY, double targetHeight)
        {
            return height / (maxY - minY) * targetHeight;
        }
    }
}