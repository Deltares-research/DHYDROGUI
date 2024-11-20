using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    public class TestModelWithHydroNetwork : ModelBase, IModel
    {
        public TestModelWithHydroNetwork()
        {
            //name is essential here
            WaterLevel = new NetworkCoverage("water level",false);
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            /*foreach (var item in HydroNetwork.GetAllItemsRecursive())
            {
                yield return item;
            }*/
            yield return HydroNetwork;
            yield return WaterLevel;//should return it to be found by sideviewcommand
            
        }

        protected override void OnInitialize()
        {
        }

        protected override void OnExecute()
        {
            Status = ActivityStatus.Done;
        }

        public bool OutputOutOfSync
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        
        public void UpdateLink(object data)
        {
            throw new NotImplementedException();
        }


        public IDataItem GetDataItemByValue(object value)
        {
            throw new NotImplementedException();
        }

        public bool SuspendClearOutputOnInputChange { get; set; }

        public HydroNetwork HydroNetwork { get; set; }

        public INetworkCoverage WaterLevel { get; set; }
    }
}