using System.Windows;
using System.Windows.Data;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters
{
    /// <summary>
    /// <see cref="HydrodynamicsInputDataTypeToVisibilityConverter"/> computes the visibility of the
    /// use spider web control based on a <see cref="WindInputType"/> value.
    /// </summary>
    /// <remarks>
    /// This needs to be a concrete class because WPF cannot instantiate generic converters.
    /// </remarks>
    /// <seealso cref="EnumToVisibilityConverter{WindInputType}" />
    [ValueConversion(typeof(WindInputType),
                     typeof(Visibility),
                     ParameterType = typeof(WindInputType))]
    public class UseSpiderWebVisibilityConverter : EnumToVisibilityConverter<WindInputType>
    {
        /// <summary>
        /// Creates a new <see cref="UseSpiderWebVisibilityConverter"/>.
        /// </summary>
        public UseSpiderWebVisibilityConverter()
        {
            CollapseHidden = true;
            InvertVisibility = true;
        }
    }
}