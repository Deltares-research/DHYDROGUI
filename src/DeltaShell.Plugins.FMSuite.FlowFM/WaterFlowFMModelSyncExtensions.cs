using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public static class WaterFlowFMModelSyncExtensions
    {
        public static void AddMissingNodeData(this WaterFlowFMModel fmModel, IEnumerable<INode> nodes)
        {
            if (fmModel == null)
                return;

            foreach (var node in nodes)
            {
                var bc = Helper1D.CreateDefaultBoundaryCondition(node, fmModel.UseSalinity, fmModel.UseTemperature);
                bc.SetBoundaryConditionDataForOutlet();
                fmModel.BoundaryConditions1D.Add(bc);
            }
        }

        public static void AddMissingBranchData(this WaterFlowFMModel fmModel, IEnumerable<IBranch> branches)
        {
            if (fmModel == null)
                return;

            var pointsToAdd = new List<NetworkLocation>();
            var lateralSourcesToAdd = new List<LateralSource>();

            foreach (IBranch branch in branches)
            {
                switch (branch)
                {
                    case IChannel channel:
                    {
                        lateralSourcesToAdd.AddRange(channel.BranchSources);
                        
                        fmModel.ChannelFrictionDefinitions.Add(new ChannelFrictionDefinition(channel));
                        fmModel.ChannelInitialConditionDefinitions.Add(new ChannelInitialConditionDefinition(channel));
                        continue;
                    }
                    case ISewerConnection sewerConnection:
                    {
                        pointsToAdd.Add(new NetworkLocation(sewerConnection, 0.0));
                        
                        if (sewerConnection.Length > 0)
                        {
                            pointsToAdd.Add(new NetworkLocation(sewerConnection, sewerConnection.Length));
                        }

                        continue;
                    }
                }
            }

            fmModel.NetworkDiscretization.AddNetworkLocationsIfNotAlreadyCreated(pointsToAdd);
            fmModel.AddMissingLateralSourceData(lateralSourcesToAdd);
        }

        public static void AddMissingLateralSourceData(this WaterFlowFMModel fmModel, IEnumerable<LateralSource> lateralSources)
        {
            if (fmModel == null)
                return;

            var lateralDataLookup = fmModel.LateralSourcesData.ToDictionary(d => d.Feature);
            foreach (var lateralSource in lateralSources)
            {
                if (lateralDataLookup.ContainsKey(lateralSource))
                    continue;

                var model1DLateralSourceData = new Model1DLateralSourceData
                {
                    Feature = lateralSource,
                    UseSalt = fmModel.UseSalinity,
                    UseTemperature = fmModel.UseTemperature
                };
                if (lateralSource.Branch is IPipe pipe)
                {
                    model1DLateralSourceData.Compartment = pipe.SourceCompartmentName != null && pipe.SourceCompartmentName.Equals(lateralSource.Name)
                                                               ? pipe.SourceCompartment
                                                               : pipe.TargetCompartment;
                }

                fmModel.LateralSourcesData.Add(model1DLateralSourceData);
            }
        }
    }
}