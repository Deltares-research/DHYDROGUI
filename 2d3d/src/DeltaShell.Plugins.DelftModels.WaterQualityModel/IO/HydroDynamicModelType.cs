namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public enum HydroDynamicModelType
    {
        Undefined,

        /// <summary>
        /// Model based on unstructured sigma model.
        /// </summary>
        Unstructured,
        Curvilinear,
        FiniteElements,
        HydroNetwork
    }
}