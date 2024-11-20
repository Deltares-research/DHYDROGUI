using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories
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
        /// <paramref name="type"/>. If <paramref name="type"/> is not a
        /// defined <see cref="ViewShapeType"/>, then null will be returned.
        /// </returns>
        IViewShape ConstructFromType(ViewShapeType type);

        /// <summary>
        /// Constructs the <see cref="IViewShape"/> corresponding with the
        /// provided <paramref name="shape"/>.
        /// </summary>
        /// <param name="shape"> The shape for which to construct a <see cref="IViewShape"/>. </param>
        /// <returns>
        /// The <see cref="IViewShape"/> corresponding with <paramref name="shape"/>.
        /// </returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="shape"/> is not defined.
        /// </exception>
        IViewShape ConstructFromShape(IBoundaryConditionShape shape);

        /// <summary>
        /// Gets the list of concrete <see cref="IViewShape"/> types.
        /// </summary>
        /// <returns>
        /// The list of concrete <see cref="IViewShape"/> types.
        /// </returns>
        IReadOnlyList<Type> GetViewShapeTypesList();
    }
}