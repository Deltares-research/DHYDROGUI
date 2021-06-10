using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters
{
    /// <summary>
    /// <see cref="HydrodynamicsInputDataTypeToVisibilityConverter"/> computes the visibility based on a
    /// <see cref="HydrodynamicsInputDataType"/> value.
    /// </summary>
    /// <remarks>
    /// This needs to be a concrete class because WPF cannot instantiate generic converters.
    /// </remarks>
    /// <seealso cref="EnumToVisibilityConverter{HydrodynamicsInputDataType}" />
    [ValueConversion(typeof(HydrodynamicsInputDataType),
                     typeof(Visibility),
                     ParameterType = typeof(HydrodynamicsInputDataType))]
    public sealed class HydrodynamicsInputDataTypeToVisibilityConverter : EnumToVisibilityConverter<HydrodynamicsInputDataType>
    {
        /// <summary>
        /// Creates a new <see cref="HydrodynamicsInputDataTypeToVisibilityConverter"/>.
        /// </summary>
        public HydrodynamicsInputDataTypeToVisibilityConverter()
        {
            CollapseHidden = true;
        }
    }
}