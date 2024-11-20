namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public enum DataTableInterpolationType
    {
        /// <summary>
        /// Apply constant (0-order) interpolation to data-table data.
        /// </summary>
        Block,

        /// <summary>
        /// Apply linear (1st-order) interpolation to data-table data.
        /// </summary>
        Linear
    }
}