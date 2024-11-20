using System;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Extensions
{
    /// <summary>
    /// <see cref="WindInputTypeExtensions"/> provides extensions methods related to
    /// the <see cref="WindInputType"/> enumeration.
    /// </summary>
    public static class WindInputTypeExtensions
    {
        /// <summary>
        /// Converts the <paramref name="inputType"/> to its corresponding <see cref="WindDefinitionType"/>.
        /// </summary>
        /// <param name="inputType">The input type.</param>
        /// <returns>
        /// The corresponding <see cref="WindDefinitionType"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the provided <paramref name="inputType"/> cannot be converted.
        /// </exception>
        public static WindDefinitionType ConvertToWindDefinitionType(this WindInputType inputType)
        {
            switch (inputType)
            {
                case WindInputType.SpiderWebGrid:
                    return WindDefinitionType.SpiderWebGrid;
                case WindInputType.WindVector:
                    return WindDefinitionType.WindXY;
                case WindInputType.XYComponents:
                    return WindDefinitionType.WindXWindY;
                default:
                    throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null);
            }
        }

        /// <summary>
        /// Converts the <paramref name="meteoDataFileType"/> to its corresponding <see cref="WindInputType"/>.
        /// </summary>
        /// <param name="meteoDataFileType">The meteo data file type.</param>
        /// <returns>
        /// The corresponding <see cref="WindInputType"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the provided <paramref name="meteoDataFileType"/> cannot be converted.
        /// </exception>
        public static WindInputType ConvertToWindInputType(this WindDefinitionType meteoDataFileType)
        {
            switch (meteoDataFileType)
            {
                case WindDefinitionType.SpiderWebGrid:
                    return WindInputType.SpiderWebGrid;
                case WindDefinitionType.WindXWindY:
                    return WindInputType.XYComponents;
                case WindDefinitionType.WindXY:
                case WindDefinitionType.WindXYP:
                    return WindInputType.WindVector;
                default:
                    throw new ArgumentOutOfRangeException(nameof(meteoDataFileType), meteoDataFileType, null);
            }
        }
    }
}
