namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    /// <summary>
    /// <see cref="IShapeVisitor"/> contains visit methods for different shapes.
    /// </summary>
    public interface IShapeVisitor
    {
        /// <summary>
        /// Visit method for defining actions of visitors when they visit a <see cref="GaussShape"/>
        /// </summary>
        /// <param name="gaussShape"> The visited <see cref="GaussShape"/></param>
        void Visit(GaussShape gaussShape);

        /// <summary>
        /// Visit method for defining actions of visitors when they visit a <see cref="JonswapShape"/>
        /// </summary>
        /// <param name="jonswapShape"> The visited <see cref="JonswapShape"/></param>
        void Visit(JonswapShape jonswapShape);

        /// <summary>
        /// Visit method for defining actions of visitors when they visit a <see cref="PiersonMoskowitzShape"/>
        /// </summary>
        /// <param name="piersonMoskowitzShape"> The visited <see cref="PiersonMoskowitzShape"/></param>
        void Visit(PiersonMoskowitzShape piersonMoskowitzShape);
    }
}