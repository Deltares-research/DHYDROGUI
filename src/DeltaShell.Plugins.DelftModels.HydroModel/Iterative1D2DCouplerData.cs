using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public class Iterative1D2DCouplerData : Unique<long>, IHydroModelWorkFlowData
    {
        private IEnumerable<IDataItem> dataItems;

        public Iterative1D2DCouplerData()
        {
            // set defaults
            MaxError = 1e-4;
            MaxIteration = 3;
            refresh1D2DLinks = true; 
        }

        public string HydroModelWorkFlowId { get { return ""; } }

        internal Iterative1D2DCoupler Coupler { get; set; }

        public IEnumerable<IDataItem> OutputDataItems
        {
            get 
            {
                if (Coupler == null || (Coupler != null && Coupler.LinkCoverages == null))
                {
                    return Enumerable.Empty<IDataItem>();
                }

                return dataItems ??
                       (dataItems = Coupler.LinkCoverages.
                                Select(c => new DataItem(c, DataItemRole.Output, c.Name + "Tag") { Owner = Coupler.HydroModel })
                               .Cast<IDataItem>()
                               .ToList());
            }
            set
            {
                dataItems = value;
            }
        }

        public double MaxError { get; set; }

        public int MaxIteration { get; set; }

        private bool refresh1D2DLinks; 
        public bool Refresh1D2DLinks
        {
            get { return refresh1D2DLinks; }
            set
            {
                if (refresh1D2DLinks != value)
                {
                    refresh1D2DLinks = value;
                    if (RefreshMappingAction != null)
                    {
                        // Either clear or create the mappings after the setting has been changed. 
                        RefreshMappingAction(false); 
                    }
                }
            }
        }

        public Action<bool> RefreshMappingAction { get; set; }

        private bool debug;
        public bool Debug
        {
            get { return debug; }
            set
            {
                debug = value;
                if (ForceDebugLoggingAction != null)
                {
                    ForceDebugLoggingAction(debug);
                }
            }
        }

        public Action<bool> ForceDebugLoggingAction { get; set; }
    }
}