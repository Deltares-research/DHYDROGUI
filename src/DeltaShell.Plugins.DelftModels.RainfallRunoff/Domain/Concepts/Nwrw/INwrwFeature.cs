using System;
using System.Collections.Concurrent;
using DelftTools.Hydro;
using DelftTools.Utils;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwImporterHelper
    {
        public ConcurrentDictionary<string, NwrwData> CurrentNwrwCatchmentModelDataByNodeOrBranchId { get; set; } = new ConcurrentDictionary<string, NwrwData>(StringComparer.InvariantCultureIgnoreCase);

    }

    public interface INwrwFeature : INameable
    {
        IGeometry Geometry { get; set; }
        void AddNwrwCatchmentModelDataToModel(IHydroModel model, NwrwImporterHelper helper);
        void InitializeNwrwCatchmentModelData(NwrwData nwrwData);
    }
}