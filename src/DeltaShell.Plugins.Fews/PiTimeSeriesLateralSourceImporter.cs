using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using NetTopologySuite.Extensions.Features.Generic;

namespace DeltaShell.Plugins.Fews
{
    public class PiTimeSeriesLateralSourceImporter : PiTimeSeriesImporter
    {
        public override bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public override IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof (FeatureData<IFunction, LateralSource>); }
        }

        public override object ImportItem(string path, object target)
        {
            var nodeData = (FeatureData<IFunction, LateralSource>) target;
            if (SelectedTimeSeries != null && SelectedTimeSeries.Count() != 0)
            {
                nodeData.Data = SelectedTimeSeries.FirstOrDefault();
            }
            else
            {
                var timeSeries = GetTimeSeriesFromFile(path);
                nodeData.Data = timeSeries.FirstOrDefault();
            }

            return target;
        }
    }
}
