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
        }

        public IEnumerable<IDataItem> GetChildDataItems(IFeature location)
        {
            yield break;
        }
    }
}