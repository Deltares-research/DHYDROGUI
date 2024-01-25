using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Serialization
{
    /// <summary>
    /// Class for converting a <see cref="InitialFieldFileData"/> into an <see cref="IniData"/>.
    /// </summary>
    public sealed class InitialFieldFileDataConverter
    {
        private readonly InitialFieldConverter initialFieldConverter;

        /// <summary>
        /// Initialize a new instance of the <see cref="InitialFieldFileDataConverter"/> class.
        /// </summary>
        public InitialFieldFileDataConverter()
        {
            initialFieldConverter = new InitialFieldConverter();
        }

        /// <summary>
        /// Convert the provided initial field file data to an <see cref="IniData"/>.
        /// </summary>
        /// <param name="initialFieldFileData"> The initial field file data to convert. </param>
        /// <returns>
        /// A new <see cref="IniData"/> instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="initialFieldFileData"/> is <c>null</c>.
        /// </exception>
        public IniData Convert(InitialFieldFileData initialFieldFileData)
        {
            Ensure.NotNull(initialFieldFileData, nameof(initialFieldFileData));

            var iniData = new IniData();

            IniSection generalSection = GetGeneralSection(initialFieldFileData);

            iniData.AddSection(generalSection);

            foreach (InitialField initial in initialFieldFileData.InitialConditions)
            {
                iniData.AddSection(initialFieldConverter.ConvertInitialCondition(initial));
            }

            foreach (InitialField parameter in initialFieldFileData.Parameters)
            {
                iniData.AddSection(initialFieldConverter.ConvertParameter(parameter));
            }

            return iniData;
        }

        private static IniSection GetGeneralSection(InitialFieldFileData initialFieldFileData)
        {
            var generalSection = new IniSection(InitialFieldFileConstants.Headers.General);
            generalSection.AddProperty(InitialFieldFileConstants.Keys.FileVersion, initialFieldFileData.General.FileVersion);
            generalSection.AddProperty(InitialFieldFileConstants.Keys.FileType, initialFieldFileData.General.FileType);
            return generalSection;
        }
    }
}