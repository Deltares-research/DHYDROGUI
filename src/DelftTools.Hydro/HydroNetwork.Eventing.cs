using System;
using System.Collections.Specialized;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.Hydro
{
    public partial class HydroNetwork
    {
        [EditAction]
        private void RoutesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var route = e.GetRemovedOrAddedItem() as Route;

            if (route == null)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    route.Network = this;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    route.Network = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}