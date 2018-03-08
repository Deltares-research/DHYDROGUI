using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate
{
    public class WaterQualityModel350LegacyLoader : LegacyLoader
    {
        public override void OnAfterProjectMigrated(Project project)
        {

            var waqModels = project.RootFolder.GetAllModelsRecursive().OfType<WaterQualityModel>();

            foreach (var waterQualityModel in waqModels)
            {
                var outputParametersDataItemSet = new DataItemSet(new EventedList<UnstructuredGridCellCoverage>(), WaterQualityModel.OutputParametersDataItemMetaData.Name, DataItemRole.Output, false, WaterQualityModel.OutputParametersDataItemMetaData.Tag, typeof(UnstructuredGridCellCoverage));
                waterQualityModel.DataItems.Insert(0, outputParametersDataItemSet);
             
                var substancesDataItemSet = new DataItemSet(new EventedList<UnstructuredGridCellCoverage>(), WaterQualityModel.OutputSubstancesDataItemMetaData.Name, DataItemRole.Output, false, WaterQualityModel.OutputSubstancesDataItemMetaData.Tag, typeof(UnstructuredGridCellCoverage));
                waterQualityModel.DataItems.Insert(0, substancesDataItemSet);

                var outputDataItems = waterQualityModel.DataItems.Where(di => di.Role.HasFlag(DataItemRole.Output)).ToList();

                foreach (var waterQualitySubstance in waterQualityModel.SubstanceProcessLibrary.Substances)
                {
                    var substanceDataItem = outputDataItems.FirstOrDefault(odi => odi.Tag == waterQualitySubstance.Name);
                    if (substanceDataItem == null) continue;
                    waterQualityModel.DataItems.Remove(substanceDataItem);
                    outputDataItems.Remove(substanceDataItem);
                    waterQualityModel.OutputSubstancesDataItemSet.DataItems.Add(substanceDataItem);
                }
                foreach (var outputParameter in waterQualityModel.SubstanceProcessLibrary.OutputParameters)
                {
                    var outputParameterDataItem = outputDataItems.FirstOrDefault(odi => odi.Tag == outputParameter.Name);
                    if (outputParameterDataItem == null) continue;
                    waterQualityModel.DataItems.Remove(outputParameterDataItem);
                    outputDataItems.Remove(outputParameterDataItem); 
                    waterQualityModel.OutputParametersDataItemSet.DataItems.Add(outputParameterDataItem);
                }
                
            }
        }
    }
}