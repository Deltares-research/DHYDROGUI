using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes
{
    public interface IVisitableBoundaryConditionShape
    {
        /// <summary>
        /// Method for accepting IShapeVisitor visitor of the visitor design pattern,
        /// used for the export.
        /// </summary>
        /// <param name="visitor">Visitor who wants to visit this object</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="visitor"/>
        /// is <c>null</c>.
        /// </exception>
        void AcceptVisitor(IShapeVisitor visitor);
    }
}