using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums
{
    /// <summary>
    /// <see cref="ViewShapeType"/> defines the shapes corresponding with the
    /// subtypes of <see cref="IViewShape"/>.
    /// </summary>
    public enum ViewShapeType
    {
        /// <summary>
        /// The Gauss shape corresponding with <see cref="GaussViewShape"/>.
        /// </summary>
        Gauss = 1,

        /// <summary>
        /// The Jonswap shape corresponding with <see cref="JonswapViewShape"/>
        /// </summary>
        Jonswap = 2,

        /// <summary>
        /// The Pierson-Moskowitz shape corresponding with <see cref="PiersonMoskowitzViewShape"/>.
        /// </summary>
        PiersonMoskowitz = 3
    }
}