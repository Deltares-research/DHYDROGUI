using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters
{
    /// <summary>
    /// <see cref="WindInputDataTypeToVisibilityConverter"/> computes the visibility based on a
    /// <see cref="WindInputDataType"/> value.
    /// </summary>
    /// <remarks>
    /// This needs to be a concrete class because WPF cannot instantiate generic converters.
    /// </remarks>
    /// <seealso cref="EnumToVisibilityConverter{HydrodynamicsInputDataType}" />
    [ValueConversion(typeof(WindInputDataType),
                     typeof(Visibility),
                     ParameterType = typeof(WindInputDataType))]
    public sealed class WindInputDataTypeToVisibilityConverter : EnumToVisibilityConverter<WindInputDataType>
    {
        /// <summary>
        /// Creates a new <see cref="WindInputDataTypeToVisibilityConverter"/>.
        /// </summary>
        public WindInputDataTypeToVisibilityConverter()
        {
            CollapseHidden = true;
        }
    }
}