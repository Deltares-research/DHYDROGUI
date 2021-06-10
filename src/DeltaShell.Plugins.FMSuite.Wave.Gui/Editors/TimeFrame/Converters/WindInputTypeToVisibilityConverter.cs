using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters
{
    /// <summary>
    /// <see cref="WindInputTypeToVisibilityConverter"/> computes the visibility based on a
    /// <see cref="WindInputType"/> value.
    /// </summary>
    /// <remarks>
    /// This needs to be a concrete class because WPF cannot instantiate generic converters.
    /// </remarks>
    /// <seealso cref="EnumToVisibilityConverter{HydrodynamicsInputDataType}" />
    [ValueConversion(typeof(WindInputType),
                     typeof(Visibility),
                     ParameterType = typeof(WindInputType))]
    public sealed class WindInputTypeToVisibilityConverter : EnumToVisibilityConverter<WindInputType>
    {
        public WindInputTypeToVisibilityConverter()
        {
            CollapseHidden = true;
        }
    }
}