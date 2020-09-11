using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="NoDependentsFileMigrateBehaviour"/> defines the base migration of a
    /// property describing a path to a file. When the file at the specified property
    /// exists it is migrated, otherwise <see cref="HandleNotExists"/> is executed.
    ///
    /// Implementation of the actual file migration is left to the specific base class.
    /// </summary>
    /// <seealso cref="IMigrationBehaviour" />
    public abstract class FileMigrateBehaviour : IMigrationBehaviour
    {
        /// <summary>
        /// The key of the <see cref="DelftIniProperty"/> this
        /// <see cref="FileMigrateBehaviour"/> migrates.
        /// </summary>
        protected readonly string ExpectedKey;

        /// <summary>
        /// The directory to which relative paths obtained from
        /// the <see cref="DelftIniProperty.Value"/> are resolved.
        /// </summary>
        protected readonly string RelativeDirectory;

        /// <summary>
        /// The directory to which files are migrated.
        /// </summary>
        protected readonly string GoalDirectory;

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

            ExpectedKey = expectedKey;
            RelativeDirectory = relativeDirectory;
            GoalDirectory = goalDirectory;
        }

        public void MigrateProperty(DelftIniProperty property, ILogHandler logHandler)
        {
            Ensure.NotNull(property, nameof(property));

            if (property.Name != ExpectedKey || 
                property.Value.Trim() == string.Empty)
            {
                return;
            }

            var filePathInfo = new FileInfo(Path.Combine(RelativeDirectory, property.Value));

            if (filePathInfo.Exists)
                HandleMigration(filePathInfo, property);
            else
                HandleNotExists(filePathInfo, property, logHandler);
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
        protected abstract void HandleMigration(FileInfo filePathInfo, DelftIniProperty property);

        /// <summary>
        /// Handles the exception behaviour of a file path that does not exist.
        /// </summary>
        /// <param name="filePathInfo">The file path information.</param>
        /// <param name="property">The property.</param>
        /// <param name="logHandler">The log handler.</param>
        /// <remarks>
        /// <paramref name="filePathInfo"/> and <paramref name="property"/> are
        /// guaranteed to be not null. Furthermore, the <paramref name="filePathInfo"/>
        /// has a non-existing file.
        ///
        /// It is assumed that this handles any changes to the property, and log any message
        /// deemed necessary.
        /// </remarks>
        protected virtual void HandleNotExists(FileInfo filePathInfo, DelftIniProperty property, ILogHandler logHandler)
        {
            logHandler?.ReportWarningFormat(Resources.FileMigrateBehaviour_HandleNotExists_The_file_associated_with_property__0____1__at__2___does_not_exist_and_thus_is_not_migrated_,
                                            ExpectedKey, Path.GetFileName(property.Value), filePathInfo.FullName);
        }
    }
}