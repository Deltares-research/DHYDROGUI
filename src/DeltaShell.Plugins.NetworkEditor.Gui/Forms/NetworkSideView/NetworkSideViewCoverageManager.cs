using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    /// <summary>
    /// This class listens to a collection of items (typically Project.RootFolder) and whenever a coverage is added or
    /// removed
    /// it calls delegate functions to keep a listener up to date. It also supports initial coverages to be defined.
    /// </summary>
    public class NetworkSideViewCoverageManager : IDisposable
    {
        private readonly Dictionary<IVariable, ICoverage> coverageQueue = new Dictionary<IVariable, ICoverage>();
        public OnRouteRemovedDelegate OnRouteRemoved { get; set; }
        public OnCoverageAddedRemovedDelegate OnCoverageAddedToProject { get; set; }
        public OnCoverageAddedRemovedDelegate OnCoverageRemovedFromProject { get; set; }
        private readonly INotifyCollectionChange projectItems;
        private IEnumerable<ICoverage> initialCoverages;
        private readonly Route route;

        public delegate void OnRouteRemovedDelegate();

        public delegate void OnCoverageAddedRemovedDelegate(ICoverage coverage);

        public NetworkSideViewCoverageManager(Route route, INotifyCollectionChange projectItems, IEnumerable<ICoverage> initialCoverages)
        {
            this.route = route;
            this.initialCoverages = initialCoverages;
            this.projectItems = projectItems;

            if (projectItems != null)
            {
                projectItems.CollectionChanged += ProjectItemsCollectionChanged;
            }
        }

        /// <summary>
        /// Triggers the initial coverages to be set through the delegate OnCoverageAddedToProject. Should be called after
        /// registering the delegate.
        /// </summary>
        public void RequestInitialCoverages()
        {
            if (initialCoverages == null)
            {
                return;
            }

            foreach (ICoverage coverage in initialCoverages.Where(coverage => coverage != null))
            {
                AddCoverage(coverage);
            }

            initialCoverages = null;
        }

        public void Dispose()
        {
            OnRouteRemoved = null;
            OnCoverageAddedToProject = null;
            OnCoverageRemovedFromProject = null;

            if (projectItems != null)
            {
                projectItems.CollectionChanged -= ProjectItemsCollectionChanged;
            }

            if (coverageQueue != null)
            {
                foreach (IVariable timeKey in coverageQueue.Keys)
                {
                    timeKey.ValuesChanged -= TimeValuesChanged;
                }

                coverageQueue.Clear();
            }
        }

        private void ModelItemRemoved(object item)
        {
            if (item == route.Network)
            {
                OnRouteRemoved?.Invoke();
            }
        }

        private void ProjectItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //start updating our stuff
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            if (removedOrAddedItem is Route)
            {
                var routeItem = removedOrAddedItem as Route;
                var routeItemNetwork = routeItem.Network as IHydroNetwork;
                if (Equals(route, routeItem) && routeItemNetwork != null && Equals(routeItemNetwork.Routes, sender))
                {
                    OnRouteRemoved?.Invoke();
                }
            }
            else if (removedOrAddedItem is IModel && e.Action == NotifyCollectionChangedAction.Remove)
            {
                (removedOrAddedItem as IModel).GetAllItemsRecursive().ForEach(ModelItemRemoved);
            }
            else if (removedOrAddedItem is IDataItem)
            {
                object value = ((IDataItem) removedOrAddedItem).Value;

                if (!(value is ICoverage))
                {
                    return;
                }

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        AddCoverage((ICoverage) value);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        OnCoverageRemovedFromProject?.Invoke((ICoverage) value);

                        break;
                    case NotifyCollectionChangedAction.Replace:
                        break;
                }
            }
        }

        private void AddCoverage(ICoverage coverage)
        {
            if (coverage is Route)
            {
                return; //filter routes out
            }

            if (coverage.IsTimeDependent && coverage.Time.Values.Count == 0)
            {
                //Coverage is time dependent, but has no time values yet: eg, model is still running / needs to run first.
                //Until we got time values, we keep the coverages in a queue so we don't give the side view coverages 
                //without valid values.

                //we may receive a notification for the same coverage multiple times in case of nested models (eg, flow in rtc)
                if (coverageQueue.ContainsKey(coverage.Time))
                {
                    return;
                }

                coverage.Time.ValuesChanged += TimeValuesChanged;
                coverageQueue.Add(coverage.Time, coverage);
            }
            else
            {
                OnCoverageAddedToProject?.Invoke(coverage);
            }
        }

        private void TimeValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            var time = (IVariable) sender;
            ICoverage coverage = coverageQueue[time];
            if (coverage.Time.Values.Count > 0)
            {
                OnCoverageAddedToProject?.Invoke(coverage);

                coverageQueue.Remove(time);
                coverage.Time.ValuesChanged -= TimeValuesChanged;
            }
        }
    }
}