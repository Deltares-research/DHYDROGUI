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
        private readonly string goalDirectory;

        /// <summary>
        /// Creates a new <see cref="NoDependentsFileMigrateBehaviour"/>.
        /// </summary>
        /// <param name="expectedKey">The expected key.</param>
        /// <param name="goalDirectory">The goal directory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public NoDependentsFileMigrateBehaviour(string expectedKey, 
                                                string goalDirectory)
        {
            Ensure.NotNull(expectedKey, nameof(expectedKey));
            Ensure.NotNull(goalDirectory, nameof(goalDirectory));

            this.expectedKey = expectedKey;
            this.goalDirectory = goalDirectory;
        }

        public void MigrateProperty(DelftIniProperty property, 
                                                ILogHandler logHandler)
        {
            Ensure.NotNull(property, nameof(property));

            if (property.Name != expectedKey)
            {
                return;
            }

            if (File.Exists(property.Value))
                HandleMigration(property);
            else
                HandleNotExists(property, logHandler);
        }

        private void HandleNotExists(DelftIniProperty property, ILogHandler logHandler)
        {
            var warningMsg = 
                $"The file associated with property {expectedKey}, {property.Value}, does not exist, the property is set to an empty string.";
            logHandler?.ReportWarning(warningMsg);

            property.Value = string.Empty;
        }

        private void HandleMigration(DelftIniProperty property)
        {
            string goalPath = Path.Combine(goalDirectory, Path.GetFileName(property.Value));
            File.Move(property.Value, goalPath);
            property.Value = goalPath;
        }
    }
}