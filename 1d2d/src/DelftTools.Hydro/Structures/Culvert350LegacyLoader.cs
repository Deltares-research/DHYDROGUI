using System.Data;
using DelftTools.Shell.Core.Dao;

namespace DelftTools.Hydro.Structures
{
    public class Culvert350LegacyLoader : LegacyLoader
    {
        public override void OnAfterInitialize(object entity, IDbConnection dbConnection)
        {
            var culvert = entity as Culvert;
            if (culvert != null)
            {
                var command = dbConnection.CreateCommand();
                command.CommandText = string.Format(
                    "SELECT IsSiphon FROM features WHERE type = 'branch_structure_culvert' AND Name = '{0}' AND branch_id = '{1}'",
                    culvert.Name, culvert.Branch.Id);

                var isSiphon = command.ExecuteScalar();
                if (isSiphon != null) // shouldn't happen, suggests corrupt database
                {
                    culvert.CulvertType = culvert.BendLossCoefficient > 0
                                              ? CulvertType.InvertedSiphon
                                              : CulvertType.Culvert;

                }
            }

            base.OnAfterInitialize(entity, dbConnection);
        }
    }
}
