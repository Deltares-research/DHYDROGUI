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
        public static void AddMissingBranchData(this WaterFlowFMModel fmModel, IEnumerable<IBranch> branches)
        {
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