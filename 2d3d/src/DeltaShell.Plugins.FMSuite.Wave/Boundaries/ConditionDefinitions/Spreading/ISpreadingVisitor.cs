namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading
{
    /// <summary>
    /// <see cref="ISpreadingVisitor"/> contains visit methods for different spreading types.
    /// </summary>
    public interface ISpreadingVisitor
    {
        /// <summary>
        /// Visit method for defining actions of visitors when they visit a <see cref="DegreesDefinedSpreading"/>
        /// </summary>
        /// <param name="degreesDefinedSpreading"> The visited <see cref="DegreesDefinedSpreading"/></param>
        void Visit(DegreesDefinedSpreading degreesDefinedSpreading);

        /// <summary>
        /// Visit method for defining actions of visitors when they visit a <see cref="PowerDefinedSpreading"/>
        /// </summary>
        /// <param name="powerDefinedSpreading"> The visited <see cref="PowerDefinedSpreading"/></param>
        void Visit(PowerDefinedSpreading powerDefinedSpreading);
    }
}