using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public class RainfallRunoffChildDataItemProvider
    {
        public RainfallRunoffChildDataItemProvider(RainfallRunoffModel model)
        {
            Model = model;
        }

        private RainfallRunoffModel Model { get; set; }

        public IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
        {
            yield break;

            //todo: at some point, when there are multiple models, only return 
            //todo: laterals/boundaries connected to 'our' schematization

            //we want exchange on connected laterals/boundaries (regardless of input/output)

            // throw new NotImplementedException();

//            var laterals = Model.Network.LateralSources.Where(l => l.Links.Count(link => Equals(link.To, l)) > 0).OfType<IFeature>();
//            var boundaries = Model.Network.HydroNodes.Where(hn => !hn.IsConnectedToMultipleBranches && hn.Links.Count(l => Equals(l.To, hn)) > 0).OfType<IFeature>();
//            return laterals.Concat(boundaries);
        }

        public IEnumerable<IDataItem> GetChildDataItems(IFeature location)
        {
            yield break;

            if (!(location is ILateralSource) && !(location is IHydroNode))
            {
                yield break;
            }

            throw new NotImplementedException("todo");
        }
    }
}