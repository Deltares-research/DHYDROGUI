using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents
{
    public interface IVisitableBoundaryConditionDataComponent
    {
        /// <summary>
        /// Method for accepting IDataComponentVisitor visitor of the visitor design pattern,
        /// used for the export.
        /// </summary>
        /// <param name="visitor">Visitor who wants to visit this object</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="visitor"/>
        /// is <c>null</c>.
        /// </exception>
        void AcceptVisitor(IDataComponentVisitor visitor);
    }
}