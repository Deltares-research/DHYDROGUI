using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    public class WaterFlowModel1D30LegacyLoader : LegacyLoader
    {
        private readonly List<WaterFlowModel1D> modelsToUpgrade = new List<WaterFlowModel1D>();
        public override void OnAfterInitialize(object entity, IDbConnection dbConnection)
        {
            base.OnAfterInitialize(entity, dbConnection);
            modelsToUpgrade.Add((WaterFlowModel1D)entity);
        }

        public override void OnAfterProjectMigrated(Project project)
        {
            foreach (var flowModel in modelsToUpgrade)
            {
                var limtypParam = flowModel.ParameterSettings.FirstOrDefault(p => p.Name == "Limtyphu1D");
                if (limtypParam != null)
                {
                    switch (limtypParam.Value)
                    {
                        case "0":
                            limtypParam.Value = "1";
                            break;
                        case "21":
                            limtypParam.Value = "3";
                            break;
                        case "100":
                            limtypParam.Value = "2";
                            break;
                    }
                }

                var advecParam = flowModel.ParameterSettings.FirstOrDefault(p => p.Name == "Iadvec1D");
                if (advecParam != null)
                {
                    switch (advecParam.Value)
                    {
                        case "4":
                            advecParam.Value = "2";
                            break;
                    }
                }
            }
        }
    }
}