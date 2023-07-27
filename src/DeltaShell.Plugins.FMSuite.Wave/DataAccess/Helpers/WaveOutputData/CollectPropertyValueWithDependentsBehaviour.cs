using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.WaveOutputData
{
    public class CollectPropertyValueWithDependentsBehaviour : IDelftIniPropertyBehaviour
    {
        private readonly string propertyName;
        private readonly string relativeDirectory;
        private readonly IDelftIniFileOperator iniFileOperator;

        /// <summary>
        /// Creates a new <see cref="CollectPropertyValueWithDependentsBehaviour"/>.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="relativeDirectory">The relative directory.</param>
        /// <param name="iniFileOperator">The ini file operator.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public CollectPropertyValueWithDependentsBehaviour(string propertyName,
                                                           string relativeDirectory,
                                                           IDelftIniFileOperator iniFileOperator)
        {
            Ensure.NotNull(propertyName, nameof(propertyName));
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));
            Ensure.NotNull(iniFileOperator, nameof(iniFileOperator));

            this.propertyName = propertyName;
            this.relativeDirectory = relativeDirectory;
            this.iniFileOperator = iniFileOperator;
        }

        public void Invoke(DelftIniProperty property, ILogHandler logHandler)
        {
            Ensure.NotNull(property, nameof(property));

            string fullPath = Path.Combine(relativeDirectory, property.Value ?? "");
            var filePathInfo = new FileInfo(fullPath);

            if (property.Name.Equals(propertyName) && filePathInfo.Exists)
            {
                FileStream stream = filePathInfo.Open(FileMode.Open);
                iniFileOperator.Invoke(stream, filePathInfo.FullName, logHandler);
                stream?.Dispose();
            }
        }
    }
}