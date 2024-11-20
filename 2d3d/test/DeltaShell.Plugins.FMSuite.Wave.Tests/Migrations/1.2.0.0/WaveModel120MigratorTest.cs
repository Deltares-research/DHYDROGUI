using System;
using System.Collections.Generic;
using System.Data;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._2._0._0;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations._1._2._0._0
{
    [TestFixture]
    public class WaveModel120MigratorTest
    {
        [Test]
        [TestCaseSource(nameof(Migrate_ArgumentNullCases))]
        public void Migrate_ArgumentNull_ThrowsArgumentNullException(IDbConnection dbConnection, Version projectVersion, string expParamName)
        {
            // Call
            void Call() => WaveModel120Migrator.Migrate(dbConnection, projectVersion);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        [TestCaseSource(nameof(Migrate_ProjectVersionGreaterThanOrEqualToVersionCases))]
        public void Migrate_ProjectVersionGreaterThanOrEqualToVersion_Returns(Version version)
        {
            var dbConnection = Substitute.For<IDbConnection>();
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            WaveModel120Migrator.Migrate(dbConnection, version, logHandler);

            // Assert
            Assert.That(dbConnection.ReceivedCalls(), Is.Empty);
        }

        [Test]
        [TestCaseSource(nameof(Migrate_ProjectVersionSmallerThanVersionCases))]
        public void Migrate_ProjectVersionSmallerThanVersion_ExecutesCorrectCommand(Version version)
        {
            var dbConnection = Substitute.For<IDbConnection>();
            var logHandler = Substitute.For<ILogHandler>();
            var command = Substitute.For<IDbCommand>();

            dbConnection.CreateCommand().Returns(command);

            // Call
            WaveModel120Migrator.Migrate(dbConnection, version, logHandler);

            // Assert
            dbConnection.Received(1).CreateCommand();
            command.Received(1).CommandText = "PRAGMA foreign_keys = off; " +
                                              "DELETE FROM project_item WHERE id IN (SELECT project_item_id FROM IDataItem WHERE value_type2 = 'DeltaShell.Plugins.FMSuite.Wave.IO.WavmFileFunctionStore, DeltaShell.Plugins.FMSuite.Wave'); " +
                                              "DELETE FROM DataItem WHERE value_type = 'DeltaShell.Plugins.FMSuite.Wave.IO.WavmFileFunctionStore, DeltaShell.Plugins.FMSuite.Wave'; " +
                                              "DELETE FROM IDataItem WHERE value_type2 = 'DeltaShell.Plugins.FMSuite.Wave.IO.WavmFileFunctionStore, DeltaShell.Plugins.FMSuite.Wave'; " +
                                              "UPDATE IDataItem SET model_list_index = (SELECT COUNT(model_list_index) FROM IDataItem AS B WHERE B.model_list_index < IDataItem.model_list_index AND B.model_id = IDataItem.model_id) WHERE model_list_index IS NOT NULL; " +
                                              "DROP TABLE IF EXISTS wavm_function_store; " +
                                              "PRAGMA foreign_keys = on;";
            command.Received(1).ExecuteNonQuery();
        }

        private static IEnumerable<TestCaseData> Migrate_ArgumentNullCases()
        {
            yield return new TestCaseData(null, new Version(), "dbConnection");
            yield return new TestCaseData(Substitute.For<IDbConnection>(), null, "projectVersion");
        }

        private static IEnumerable<Version> Migrate_ProjectVersionGreaterThanOrEqualToVersionCases()
        {
            yield return new Version(1, 3, 0, 0);
            yield return new Version(1, 3, 0, 1);
            yield return new Version(1, 3, 1, 0);
            yield return new Version(1, 4, 0, 0);
        }

        private static IEnumerable<Version> Migrate_ProjectVersionSmallerThanVersionCases()
        {
            yield return new Version(1, 2, 2, 2);
            yield return new Version(1, 2, 0, 0);
            yield return new Version(1, 1, 0, 0);
        }
    }
}