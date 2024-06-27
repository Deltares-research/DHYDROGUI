using System.IO;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="FileMigrateBehaviour"/> defines the base migration of a
    /// property describing a path to a file. When the file at the specified property
    /// exists it is migrated, otherwise a warning is logged.
    /// Implementation of the actual file migration is left to the specific base class.
    /// </summary>
    /// <seealso cref="IIniPropertyBehaviour"/>
    public abstract class FileMigrateBehaviour : IIniPropertyBehaviour
    {
        /// <summary>
        /// The directory to which files are migrated.
        /// </summary>
        protected readonly string GoalDirectory;

        private readonly string expectedKey;
        private readonly string relativeDirectory;

        /// <summary>
        /// Creates a new <see cref="NoDependentsFileMigrateBehaviour"/>.
        /// </summary>
        /// <param name="expectedKey">The expected key.</param>
        /// <param name="relativeDirectory">The path relative to which property values are evaluated.</param>
        /// <param name="goalDirectory">The goal directory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        protected FileMigrateBehaviour(string expectedKey,
                                       string relativeDirectory,
                                       string goalDirectory)
        {
            Ensure.NotNull(expectedKey, nameof(expectedKey));
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));
            Ensure.NotNull(goalDirectory, nameof(goalDirectory));

            this.expectedKey = expectedKey;
            this.relativeDirectory = relativeDirectory;
            GoalDirectory = goalDirectory;
        }

        public void Invoke(IniProperty property, ILogHandler logHandler)
        {
            Ensure.NotNull(property, nameof(property));

            if (property.Key != expectedKey ||
                property.Value.Trim() == string.Empty)
            {
                return;
            }

            var filePathInfo = new FileInfo(Path.Combine(relativeDirectory, property.Value));

            if (filePathInfo.Exists)
            {
                HandleMigration(filePathInfo, property);
            }
            else
            {
                HandleNotExists(filePathInfo, property, logHandler);
            }
        }

        /// <summary>
        /// Handles the migration of the property and its associated file.
        /// </summary>
        /// <param name="filePathInfo">The <see cref="FileInfo"/> of the provided path.</param>
        /// <param name="property">The property.</param>
        /// <remarks>
        /// <paramref name="filePathInfo"/> and <paramref name="property"/> are
        /// guaranteed to be not null. Furthermore, the <paramref name="filePathInfo"/>
        /// has an existing file.
        /// </remarks>
        protected abstract void HandleMigration(FileInfo filePathInfo, IniProperty property);

        private void HandleNotExists(FileInfo filePathInfo, IniProperty property, ILogHandler logHandler)
        {
            logHandler?.ReportWarningFormat(Resources.FileMigrateBehaviour_HandleNotExists_The_file_associated_with_property__0____1__at__2___does_not_exist_and_thus_is_not_migrated_,
                                            expectedKey, Path.GetFileName(property.Value), filePathInfo.FullName);
        }
    }
}