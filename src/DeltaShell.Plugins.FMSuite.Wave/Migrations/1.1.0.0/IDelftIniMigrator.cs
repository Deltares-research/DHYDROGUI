using DeltaShell.NGHS.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    public interface IDelftIniMigrator
    {
        void MigrateFile(string srcFile, string targetDirectory, ILogHandler logHandler);
    }
}