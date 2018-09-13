using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.NetCDF;
using GeoAPI.Extensions.Coverages;

namespace Sobek.IntegrationTests
{
    public static class ModelTestHelper
    {
        public static void ReplaceStoreForOutputCoverages(IModel model)
        {
            foreach (var pi in model.GetAllItemsRecursive().OfType<DataItem>()
                                    .Where(di => (di.Value is INetworkCoverage || di.Value is IFeatureCoverage) &&
                                                 (di.Role & DataItemRole.Output) == DataItemRole.Output))
            {
                var store = new NetCdfFunctionStore();
                string tempFileName = Path.GetTempFileName();
                store.CreateNew(tempFileName);
                store.Functions.Add((IFunction) pi.Value);
            }
        }

        public static HydroModel GetHydroModelForSobek()
        {
            var hydroModel = new HydroModelBuilder().BuildModel(ModelGroup.All);
            hydroModel.Activities.RemoveAllWhere(a => !(a is WaterFlowModel1D ||
                                                        a is RainfallRunoffModel ||
                                                        a is RealTimeControlModel));
            return hydroModel;
        }

        public static void RefreshCrossSectionDefinitionSectionWidths(IHydroNetwork network)
        {
            EnsureAllCrossSectionDefinitionsHaveAtLeastOneSection(network);

            // fix for added validation (cross section definition sections total width should not be less than total cross section width
            network.CrossSections.Select(cs => cs.Definition)
                .OfType<CrossSectionDefinition>()
                .Union
                (
                    network.CrossSections.Select(cs => cs.Definition)
                        .OfType<CrossSectionDefinitionProxy>()
                        .Select(csdp => csdp.InnerDefinition)
                        .OfType<CrossSectionDefinition>()
                )
                .ForEach(csd => csd.RefreshSectionsWidths());
        }

        private static void EnsureAllCrossSectionDefinitionsHaveAtLeastOneSection(IHydroNetwork network)
        {
            var crossSectionDefinitionsWithoutSections = network.CrossSections.Select(cs => cs.Definition)
                .Union(network.SharedCrossSectionDefinitions)
                .Where(csd => !csd.Sections.Any())
                .ToList();

            if (!crossSectionDefinitionsWithoutSections.Any()) return;

            var mainSection = network.CrossSectionSectionTypes.FirstOrDefault(csst => csst.Name.Equals(RoughnessDataSet.MainSectionTypeName));
            if (mainSection == null)
            {
                mainSection = new CrossSectionSectionType { Name = RoughnessDataSet.MainSectionTypeName };
                network.CrossSectionSectionTypes.Add(mainSection);
            }

            foreach (var definition in crossSectionDefinitionsWithoutSections)
            {
                definition.Sections.Add(new CrossSectionSection { SectionType = mainSection });
            }
        }
    }
}