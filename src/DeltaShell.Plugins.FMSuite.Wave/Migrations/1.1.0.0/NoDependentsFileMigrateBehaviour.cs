using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DelftIniObjects;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="NoDependentsFileMigrateBehaviour"/> defines the migration of a
    /// property describing a path to a file without dependents (i.e. containing references to other files).
    /// </summary>
    /// <seealso cref="IMigrationBehaviour" />
    public class NoDependentsFileMigrateBehaviour : IMigrationBehaviour
    {
        private readonly string expectedKey;
        private readonly string relativeDirectory;
        private readonly string goalDirectory;

        /// <summary>
        /// Creates a new <see cref="NoDependentsFileMigrateBehaviour"/>.
        /// </summary>
        /// <param name="expectedKey">The expected key.</param>
        /// <param name="relativeDirectory">The path relative to which property values are evaluated.</param>
        /// <param name="goalDirectory">The goal directory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public NoDependentsFileMigrateBehaviour(string expectedKey,
                                                string relativeDirectory,
                                                string goalDirectory)
        {
            Ensure.NotNull(expectedKey, nameof(expectedKey));
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));
            Ensure.NotNull(goalDirectory, nameof(goalDirectory));

            this.expectedKey = expectedKey;
            this.relativeDirectory = relativeDirectory;
            this.goalDirectory = goalDirectory;
        }

        public void MigrateProperty(DelftIniProperty property, 
                                                ILogHandler logHandler)
        {
            Ensure.NotNull(property, nameof(property));

            if (property.Name != expectedKey ||
                property.Value.Trim() == string.Empty)
            {
                return;
            }

            var filePathInfo = new FileInfo(Path.Combine(relativeDirectory, property.Value));

            if (filePathInfo.Exists)
                HandleMigration(filePathInfo, property);
            else
                HandleNotExists(filePathInfo, property, logHandler);
        }

        private void HandleNotExists(FileInfo filePathInfo, DelftIniProperty property, ILogHandler logHandler)
        {
            var warningMsg = 
                $"The file associated with property {expectedKey}, {Path.GetFileName(property.Value)} at {filePathInfo.FullName}, does not exist, the property is set to an empty string.";
            logHandler?.ReportWarning(warningMsg);

            property.Value = string.Empty;
        }

        private void HandleMigration(FileInfo filePathInfo, DelftIniProperty property)
        {
            string goalPath = Path.Combine(goalDirectory, Path.GetFileName(property.Value));

            filePathInfo.MoveTo(goalPath);
            property.Value = filePathInfo.Name;
        }
    }
}