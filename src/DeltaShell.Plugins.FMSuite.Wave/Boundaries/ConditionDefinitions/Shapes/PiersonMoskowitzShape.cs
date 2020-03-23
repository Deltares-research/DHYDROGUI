namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    /// <summary>
    /// <see cref="PiersonMoskowitzShape"/> defines the Pierson-Moskowitz <see cref="IBoundaryConditionShape"/>.
    /// </summary>
    /// <seealso cref="IBoundaryConditionShape" />
    /// <remarks>
    /// No data is associated with this <see cref="IBoundaryConditionShape"/>.
    /// </remarks>
    public class PiersonMoskowitzShape : IBoundaryConditionShape
    {
        /// <summary>
        /// Name that should be written as value for "SpShapeType" property in Mdw.
        /// </summary>
        public string XmlName { get; } = "Pierson-Moskowitz";
    }
}