using System;
using System.Collections.Generic;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class ImportedNetFileXyzExporter: GridPointsExporter
    {
        public override IEnumerable<Type> SourceTypes()
        {
            yield return typeof (ImportedFMNetFile);
        }
        
        protected override bool CheckObject(object item)
        {
            return item is ImportedFMNetFile;
        }
        
        protected override IEnumerable<IPointValue> GetPointValues(object item)
        {
            return base.GetPointValues(((ImportedFMNetFile) item).Grid);
        }
    }
}
