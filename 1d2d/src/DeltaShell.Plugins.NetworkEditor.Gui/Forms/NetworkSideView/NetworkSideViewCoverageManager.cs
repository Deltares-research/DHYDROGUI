using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    /// <summary>
    /// This class listenens to a collection of items (typically Project.RootFolder) and whenever a coverage is added or removed
    /// it calls delegate functions to keep a listener up to date. It also supports initial coverages to be defined.
    /// </summary>
    public class NetworkSideViewCoverageManager : IDisposable
    {
        private INotifyCollectionChange projectItems;
        private IEnumerable<ICoverage> initialCoverages;
        private readonly Route route;
        public OnRouteRemovedDelegate OnRouteRemoved;
        public OnCoverageAddedRemovedDelegate OnCoverageAddedToProject;
        public OnCoverageAddedRemovedDelegate OnCoverageRemovedFromProject;

        private readonly Dictionary<IVariable, ICoverage> coverageQueue = new Dictionary<IVariable, ICoverage>();

        public NetworkSideViewCoverageManager(Route route, INotifyCollectionChange projectItems, IEnumerable<ICoverage> initialCoverages)
        {
            this.route = route;
            this.initialCoverages = initialCoverages;
            this.projectItems = projectItems;

            if (projectItems != null)
            {
                projectItems.CollectionChanged += ProjectItemsCollectionChanged;
                ((INotifyPropertyChanged)projectItems).PropertyChanged += SynchronizeCoverages;
            }
        }

        public delegate void OnRouteRemovedDelegate();

        public delegate void OnCoverageAddedRemovedDelegate(ICoverage coverage);

        /// <summary>
        /// Triggers the initial coverages to be set through the delegate OnCoverageAddedToProject. Should be called after registering the delegate.
        /// </summary>
        public void RequestInitialCoverages()
        {
            if (initialCoverages == null)
                return;

            foreach (var coverage in initialCoverages.Where(coverage => coverage != null))
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
                ((INotifyPropertyChanged)projectItems).PropertyChanged -= SynchronizeCoverages;
            }

            if (coverageQueue != null)
            {
                foreach(IVariable timeKey in coverageQueue.Keys)
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
            var item = e.GetRemovedOrAddedItem();
            switch (item)
            {
                case Route routeItem:
                {
                    if (Equals(route, routeItem) && routeItem.Network is IHydroNetwork routeItemNetwork && Equals(routeItemNetwork.Routes, sender))
                    {
                        OnRouteRemoved?.Invoke();
                    }

                    break;
                }
                case IModel model when e.Action == NotifyCollectionChangedAction.Remove:
                    model.GetAllItemsRecursive().ForEach(ModelItemRemoved);
                    break;
                case IDataItem dataItem:
                {
                    var value = dataItem.Value;

                    if (!(value is ICoverage coverage))
                        return;

                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            AddCoverage(coverage);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            OnCoverageRemovedFromProject?.Invoke(coverage);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            break;
                    }

                    break;
                }
            }
        }

        private void SynchronizeCoverages(object sender, PropertyChangedEventArgs e)
        {
            if (!RequireSynchronizationOfCoverages(sender, e))
            {
                return;
            }
            
            IEnumerable<ICoverage> coverages = GetAllCoveragesFromModel(sender as IModel);

            AddOrRemoveCoverages(sender as IEditableObject, coverages);
        }
        
        private static bool RequireSynchronizationOfCoverages(object sender, PropertyChangedEventArgs e)
        {
            return sender is IModel 
                   && e.PropertyName == nameof(IEditableObject.IsEditing) 
                   && !(sender is ICompositeActivity);
        }
        
        private static IEnumerable<ICoverage> GetAllCoveragesFromModel(IItemContainer model)
        {
            return model.GetAllItemsRecursive().OfType<ICoverage>().Distinct();
        }

        private void AddOrRemoveCoverages(IEditableObject model, IEnumerable<ICoverage> coverages)
        {
            if (!model.IsEditing && CurrentActionIsReconnectingOutputFiles(model.CurrentEditAction))
            {
                foreach (ICoverage coverage in coverages)
                {
                    OnCoverageAddedToProject?.Invoke(coverage);
                }
            } 
            else if (model.IsEditing && CurrentActionIsDisconnectingOutputFiles(model.CurrentEditAction))
            {
                foreach (ICoverage coverage in coverages)
                {
                    OnCoverageRemovedFromProject?.Invoke(coverage);
                }
            }
        }

        private static bool CurrentActionIsReconnectingOutputFiles(IEditAction modelCurrentEditAction)
        {
            return modelCurrentEditAction.Name.Equals(
                DelftTools.Hydro.Properties.Resources.Reconnect_output_files_edit_action);
        }
        
        private static bool CurrentActionIsDisconnectingOutputFiles(IEditAction modelCurrentEditAction)
        {
            return modelCurrentEditAction.Name.Equals(
                DelftTools.Hydro.Properties.Resources.Disconnect_output_files_edit_action);
        }

        private void AddCoverage(ICoverage coverage)
        {
            if (coverage is Route)
                return; //filter routes out

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
                if (OnCoverageAddedToProject != null)
                {
                    OnCoverageAddedToProject(coverage);
                }
            }
        }

        private void TimeValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            var time = (IVariable)sender;
            var coverage = coverageQueue[time];
            if (coverage.Time.Values.Count > 0)
            {
                OnCoverageAddedToProject?.Invoke(coverage);

                coverageQueue.Remove(time);
                coverage.Time.ValuesChanged -= TimeValuesChanged;
            }
        }
    }
}
