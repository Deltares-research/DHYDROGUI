using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.WaveOutputData
{
    public class CollectPropertyValueWithDependentsBehaviour : IIniPropertyBehaviour
    {
        private readonly string propertyKey;
        private readonly string relativeDirectory;
        private readonly IIniFileOperator iniFileOperator;

        /// <summary>
        /// Creates a new <see cref="CollectPropertyValueWithDependentsBehaviour"/>.
        /// </summary>
        /// <param name="propertyKey">Name of the property.</param>
        /// <param name="relativeDirectory">The relative directory.</param>
        /// <param name="iniFileOperator">The ini file operator.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public CollectPropertyValueWithDependentsBehaviour(string propertyKey,
                                                           string relativeDirectory,
                                                           IIniFileOperator iniFileOperator)
        {
            Ensure.NotNull(propertyKey, nameof(propertyKey));
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));
            Ensure.NotNull(iniFileOperator, nameof(iniFileOperator));

            this.propertyKey = propertyKey;
            this.relativeDirectory = relativeDirectory;
            this.iniFileOperator = iniFileOperator;
        }

        public void Invoke(IniProperty property, ILogHandler logHandler)
        {
            Ensure.NotNull(property, nameof(property));

            string fullPath = Path.Combine(relativeDirectory, property.Value ?? "");
            var filePathInfo = new FileInfo(fullPath);

            if (property.Key.Equals(propertyKey) && filePathInfo.Exists)
            {
                FileStream stream = filePathInfo.Open(FileMode.Open);
                iniFileOperator.Invoke(stream, filePathInfo.FullName, logHandler);
                stream?.Dispose();
            }
        }
    }
}