using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="DelftIniMigrator"/> implements the interface with which to migrate
    /// delft ini files.
    /// </summary>
    /// <seealso cref="IDelftIniMigrator" />
    public class DelftIniMigrator : IDelftIniMigrator
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>> 
            migrationBehaviourMapping;
        private readonly IDelftIniReader iniReader;
        private readonly IDelftIniWriter iniWriter;
        private readonly bool removeOriginalIniFile;

        /// <summary>
        /// Creates a new <see cref="DelftIniMigrator"/>.
        /// </summary>
        /// <param name="migrationBehaviourMapping">
        /// The migration behaviour mapping, where the first key maps to
        /// category name and the second dictionary maps to property name.
        /// </param>
        /// <param name="iniReader">The ini reader.</param>
        /// <param name="iniWriter">The ini writer.</param>
        /// <param name="removeOriginalIniFile">
        /// Whether to remove the original source ini file.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public DelftIniMigrator(
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>> migrationBehaviourMapping,
            IDelftIniReader iniReader, 
            IDelftIniWriter iniWriter,
            bool removeOriginalIniFile)
        {
            Ensure.NotNull(migrationBehaviourMapping, nameof(migrationBehaviourMapping));
            Ensure.NotNull(iniReader, nameof(iniReader));
            Ensure.NotNull(iniWriter, nameof(iniWriter));

            this.migrationBehaviourMapping = migrationBehaviourMapping;
            this.iniReader = iniReader;
            this.iniWriter = iniWriter;
            this.removeOriginalIniFile = removeOriginalIniFile;
        }

        public void MigrateFile(Stream sourceFileStream, 
                                string sourceFilePath,
                                string targetFilePath, 
                                ILogHandler logHandler)
        {
            Ensure.NotNull(sourceFileStream, nameof(sourceFileStream));
            Ensure.NotNull(sourceFilePath, nameof(sourceFilePath));
            Ensure.NotNull(targetFilePath, nameof(targetFilePath));

            IList<DelftIniCategory> categories = 
                iniReader.ReadDelftIniFile(sourceFileStream, sourceFilePath);

            foreach (DelftIniCategory category in categories)
            {
                MigrateCategory(category, logHandler);
            }

            if (removeOriginalIniFile)
            {
                File.Delete(sourceFilePath);
            }

            iniWriter.WriteDelftIniFile(categories, targetFilePath);
        }

        private void MigrateCategory(DelftIniCategory category, ILogHandler logHandler)
        {
            if (!migrationBehaviourMapping.TryGetValue(category.Name, out IReadOnlyDictionary<string, IMigrationBehaviour> categoryMapping))
            {
                return;
            }

            foreach (DelftIniProperty property in category.Properties)
            {
                MigrateProperty(property, categoryMapping, logHandler);
            }
        }

        private static void MigrateProperty(DelftIniProperty property, 
                                            IReadOnlyDictionary<string, IMigrationBehaviour> categoryMapping,
                                            ILogHandler logHandler)
        {
            if (categoryMapping.TryGetValue(property.Name, out IMigrationBehaviour migrationBehaviour))
            {
                migrationBehaviour.MigrateProperty(property, logHandler);
            }
        }
    }
}