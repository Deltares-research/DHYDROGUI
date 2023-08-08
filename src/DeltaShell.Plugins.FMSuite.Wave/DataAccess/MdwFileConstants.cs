namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// Contains constant property names and values used for reading and writing MDW files.
    /// </summary>
    public static class MdwFileConstants
    {
        /// <summary>
        /// Gets the property name for the obstacle's name.
        /// </summary>
        public const string ObstaclePropertyName = "Name";

        /// <summary>
        /// Gets the property name for the obstacle's type.
        /// </summary>
        public const string ObstaclePropertyType = "Type";

        /// <summary>
        /// Gets the property name for the obstacle's transmission coefficient.
        /// </summary>
        public const string ObstaclePropertyTransmissionCoefficient = "TransmCoef";

        /// <summary>
        /// Gets the default value for the obstacle's transmission coefficient.
        /// </summary>
        public const double ObstacleDefaultValueTransmissionCoefficient = 0.0;

        /// <summary>
        /// Gets the property name for the obstacle's height.
        /// </summary>
        public const string ObstaclePropertyHeight = "Height";

        /// <summary>
        /// Gets the default value for the obstacle's height.
        /// </summary>
        public const double ObstacleDefaultValueHeight = 0.0;

        /// <summary>
        /// Gets the property name for the obstacle's alpha value.
        /// </summary>
        public const string ObstaclePropertyAlpha = "Alpha";

        /// <summary>
        /// Gets the default value for the obstacle's alpha value.
        /// </summary>
        public const double ObstacleDefaultValueAlpha = 0.0;

        /// <summary>
        /// Gets the property name for the obstacle's beta value.
        /// </summary>
        public const string ObstaclePropertyBeta = "Beta";

        /// <summary>
        /// Gets the default value for the obstacle's beta value.
        /// </summary>
        public const double ObstacleDefaultValueBeta = 0.0;

        /// <summary>
        /// Gets the property name for the obstacle's reflections.
        /// </summary>
        public const string ObstaclePropertyReflections = "Reflections";

        /// <summary>
        /// Gets the property name for the obstacle's reflection coefficient.
        /// </summary>
        public const string ObstaclePropertyReflectionCoefficient = "ReflecCoef";

        /// <summary>
        /// Gets the default value for the obstacle's reflection coefficient.
        /// </summary>
        public const double ObstacleDefaultValueReflectionCoefficient = 0.0;
    }
}