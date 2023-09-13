using System.IO;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="NoDependentsFileMigrateBehaviour"/> defines the migration of a
    /// property describing a path to a file without dependents (i.e. containing references to other files).
    /// </summary>
    /// <seealso cref="FileMigrateBehaviour"/>
    public sealed class NoDependentsFileMigrateBehaviour : FileMigrateBehaviour
    {
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
                                                string goalDirectory) : base(expectedKey, relativeDirectory, goalDirectory) {}

        protected override void HandleMigration(FileInfo filePathInfo, IniProperty property)
        {
            string goalPath = Path.Combine(GoalDirectory, Path.GetFileName(property.Value));

            filePathInfo.MoveTo(goalPath);
            property.Value = filePathInfo.Name;
        }
    }
}