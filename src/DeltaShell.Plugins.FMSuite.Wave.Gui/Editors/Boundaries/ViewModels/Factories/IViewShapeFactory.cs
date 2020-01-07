using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.Factories
{
    /// <summary>
    /// <see cref="IViewShapeFactory"/> defines the interface to construct the different
    /// <see cref="IViewShape"/>.
    /// </summary>
    public interface IViewShapeFactory
    {
        /// <summary>
        /// Constructs the <see cref="IViewShape"/> corresponding with the
        /// provided <paramref name="type"/>.
        /// </summary>
        /// <param name="type"> The <see cref="IViewShape"/> to construct. </param>
        /// <returns>
        /// An <see cref="IViewShape"/> corresponding with the provided
        /// <paramref name="type"/>. If <paramref name="type"/> is not a child
        /// of <see cref="IViewShape"/> then <c>null</c> will be returned.
        /// </returns>
        IViewShape ConstructFromType(Type type);

        /// <summary>
        /// Constructs the <see cref="IViewShape"/> corresponding with the
        /// provided <paramref name="shape"/>.
        /// </summary>
        /// <param name="shape"> The shape for which to construct a <see cref="IViewShape"/>. </param>
        /// <returns>
        /// The <see cref="IViewShape"/> corresponding with <paramref name="shape"/>, if no
        /// corresponding <see cref="IViewShape"/> exists, null is returned.
        /// </returns>
        IViewShape ConstructFromShape(IBoundaryConditionShape shape);

    }
}