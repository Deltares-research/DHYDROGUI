using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes
{
    /// <summary>
    /// <see cref="IViewShape"/> wraps the <see cref="IBoundaryConditionShape"/>
    /// and presents it to the View layer.
    /// </summary>
    public interface IViewShape
    {
        /// <summary>
        /// Gets the underlying <see cref="IBoundaryConditionShape"/>.
        /// </summary>
        IBoundaryConditionShape ObservedShape { get; }
    }
}