using System.Collections.Generic;
using System.Data;
using DelftTools.Shell.Core.Dao;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="LegacyLoader"/> for the <see cref="WaveModel"/> to migrate
    /// to the directory structure associated with file format version 1.2.0.0.
    /// </summary>
    /// <seealso cref="LegacyLoader" />
    public class WaveModel110LegacyLoader : LegacyLoader
    {
        public override void OnAfterInitialize(object entity, IDbConnection dbConnection)
        {

        }
    }
}