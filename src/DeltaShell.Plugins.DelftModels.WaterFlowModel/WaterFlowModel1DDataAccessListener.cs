using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Units;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    public class WaterFlowModel1DDataAccessListener : DataAccessListenerBase
    {
        private bool firstNetwork = true;
        private bool firstFlowModel = true;

        public override object Clone()
        {
            return new WaterFlowModel1DDataAccessListener {ProjectRepository = ProjectRepository};
        }

        public override void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            var waterFlowModel1D = entity as WaterFlowModel1D;
            if (waterFlowModel1D != null)
            {
                // Check if name is still valid (for backwards compatibility)
                var name = waterFlowModel1D.DispersionFormulationTypeParameter.Value;
                if (Enum.GetNames(typeof(DispersionFormulationType)).All(n => n != name))
                {
                    waterFlowModel1D.DispersionFormulationType = DispersionFormulationType.Constant;
                }
            }
                
            base.OnPostLoad(entity, state, propertyNames);
        }

        public override void OnPreLoad(object entity, object[] loadedState, string[] propertyNames)
        {
            // nhibernate performance optimizations:
            if (entity is Project)
            {
                firstNetwork = true;
                firstFlowModel = true;
            }
            else if (firstFlowModel && entity is WaterFlowModel1D)
            {
                ProjectRepository.PreLoad<Parameter>(fp => fp.Value);

                ProjectRepository.PreLoad<WaterFlowModel1DLateralSourceData>(lsd => lsd.Feature);
                ProjectRepository.PreLoad<WaterFlowModel1DLateralSourceData>(lsd => lsd.SeriesDataItem);
                ProjectRepository.PreLoad<WaterFlowModel1DLateralSourceData>(lsd => lsd.FlowConstantDataItem);

                ProjectRepository.PreLoad<WaterFlowModel1DBoundaryNodeData>(bnd => bnd.Feature);
                ProjectRepository.PreLoad<WaterFlowModel1DBoundaryNodeData>(bnd => bnd.SeriesDataItem);
                ProjectRepository.PreLoad<WaterFlowModel1DBoundaryNodeData>(bnd => bnd.FlowConstantDataItem);

                firstFlowModel = false;
            }
            else if (firstNetwork && entity is HydroNetwork)
            {
                ProjectRepository.PreLoad<HydroNode>(n => n.Links);
                ProjectRepository.PreLoad<LateralSource>(n => n.Links);
                ProjectRepository.PreLoad<ICompositeBranchStructure>(cbs => cbs.Structures);
                firstNetwork = false;
            }
        }
    }
}