using DelftTools.Utils.Guards;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Serialization
{
    /// <summary>
    /// Class for converting a <see cref="InitialField"/> into an <see cref="IniSection"/>.
    /// </summary>
    public sealed class InitialFieldConverter
    {
        /// <summary>
        /// Convert the provided initial condition field to an INI section.
        /// </summary>
        /// <param name="initialCondition"> The initial condition field to convert. </param>
        /// <returns>
        /// A new <see cref="IniSection"/> instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="initialCondition"/> is <c>null</c>.
        /// </exception>
        public IniSection ConvertInitialCondition(InitialField initialCondition)
        {
            Ensure.NotNull(initialCondition, nameof(initialCondition));

            var iniSection = new IniSection(InitialFieldFileConstants.Headers.Initial);
            ConvertProperties(initialCondition, iniSection);

            return iniSection;
        }

        /// <summary>
        /// Convert the provided parameter field to an INI section.
        /// </summary>
        /// <param name="parameter"> The parameter field to convert. </param>
        /// <returns>
        /// A new <see cref="IniSection"/> instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="parameter"/> is <c>null</c>.
        /// </exception>
        public IniSection ConvertParameter(InitialField parameter)
        {
            Ensure.NotNull(parameter, nameof(parameter));

            var iniSection = new IniSection(InitialFieldFileConstants.Headers.Parameter);
            ConvertProperties(parameter, iniSection);

            return iniSection;
        }

        private static void ConvertProperties(InitialField initialField, IniSection iniSection)
        {
            iniSection.AddProperty(InitialFieldFileConstants.Keys.Quantity, initialField.Quantity.GetDescription());
            iniSection.AddProperty(InitialFieldFileConstants.Keys.DataFile, initialField.DataFile);
            iniSection.AddProperty(InitialFieldFileConstants.Keys.DataFileType, initialField.DataFileType.GetDescription());

            if (initialField.DataFileType == InitialFieldDataFileType.OneDField)
            {
                return;
            }

            iniSection.AddProperty(InitialFieldFileConstants.Keys.InterpolationMethod, initialField.InterpolationMethod.GetDescription());
            iniSection.AddProperty(InitialFieldFileConstants.Keys.Operand, initialField.Operand.GetDescription());

            if (initialField.InterpolationMethod == InitialFieldInterpolationMethod.Averaging)
            {
                iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingType, initialField.AveragingType.GetDescription());
                iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingRelSize, initialField.AveragingRelSize);
                iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingNumMin, initialField.AveragingNumMin);
                iniSection.AddProperty(InitialFieldFileConstants.Keys.AveragingPercentile, initialField.AveragingPercentile);
            }

            AddBool(iniSection, InitialFieldFileConstants.Keys.ExtrapolationMethod, initialField.ExtrapolationMethod);
            iniSection.AddProperty(InitialFieldFileConstants.Keys.LocationType, initialField.LocationType.GetDescription());

            if (initialField.DataFileType == InitialFieldDataFileType.Polygon)
            {
                iniSection.AddProperty(InitialFieldFileConstants.Keys.Value, initialField.Value);
            }
        }

        private static void AddBool(IniSection iniSection, string key, bool value)
        {
            iniSection.AddProperty(key, value ? "yes" : "no");
        }
    }
}