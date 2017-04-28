namespace DelftTools.Hydro.Structures
{
    public interface IGroundLayer
    {
        /// <summary>
        /// Define a ground layer?
        /// </summary>
        bool GroundLayerEnabled { get; set; }

        /// <summary>
        /// Roughness of the groundlayer.
        /// </summary>
        double GroundLayerRoughness { get; set; }

        /// <summary>
        /// Thickness/depth of the groundlayer
        /// </summary>
        double GroundLayerThickness { get; set; }
    }
}