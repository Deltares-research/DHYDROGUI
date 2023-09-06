using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public class MultipleDataEditorListeningToModelNwrwDryWeatherFlowDefinitions : MultipleDataEditor
    {
        private IEventedList<NwrwDryWeatherFlowDefinition> nwrwDryWeatherFlowDefinitions;
        
        public override object Data
        {
            get 
            {
                return base.Data; 
            }
            set
            {
                if (base.Data != null && nwrwDryWeatherFlowDefinitions != null)
                {
                    nwrwDryWeatherFlowDefinitions.CollectionChanged -= OnDefinitionsOnCollectionChanged;
                    ((INotifyPropertyChanged)nwrwDryWeatherFlowDefinitions).PropertyChanged -= OnDefinitionsOnPropertyChanged;
                }

                base.Data = value;

                if (base.Data != null)
                {
                    var dataRowProviders = value as IEnumerable<IDataRowProvider>;
                    var rrModel = dataRowProviders?.FirstOrDefault()?.Model;
                    if (rrModel != null)
                    {
                        nwrwDryWeatherFlowDefinitions = rrModel.NwrwDryWeatherFlowDefinitions;
                        nwrwDryWeatherFlowDefinitions.CollectionChanged += OnDefinitionsOnCollectionChanged;
                        ((INotifyPropertyChanged) nwrwDryWeatherFlowDefinitions).PropertyChanged += OnDefinitionsOnPropertyChanged;
                    }
                }
            }
        }

        

        private void OnDefinitionsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var providers = Data as IEnumerable<IDataRowProvider>;
            var rows = providers.SelectMany(pr => pr.Rows).OfType<NwrwDataRow>().ToArray();

            var oldDefinitions = new HashSet<string>(rows.Select(r => r.FirstDryWeatherFlowId)
                                                                    .Concat(rows.Select(r => r.LastDryWeatherFlowId))); // Old definitions present in view
            var newDefinitions = new HashSet<string>(nwrwDryWeatherFlowDefinitions.Select(def => def.Name)); // Definitions present in RR Model
            oldDefinitions.ExceptWith(newDefinitions); 
            
            if (string.Equals(e.PropertyName, nameof(NwrwDefinition.Name)))
            {
                var nwrwDefinition = sender as NwrwDryWeatherFlowDefinition;
                if (nwrwDefinition != null)
                {
                    var newValue = nwrwDefinition.Name; // This is our new name
                    foreach (var nwrwDataRow in rows)
                    {
                        if (oldDefinitions.Contains(nwrwDataRow.FirstDryWeatherFlowId))
                        {
                            nwrwDataRow.FirstDryWeatherFlowId = newValue;
                        }

                        if (oldDefinitions.Contains(nwrwDataRow.LastDryWeatherFlowId))
                        {
                            nwrwDataRow.LastDryWeatherFlowId = newValue;
                        }
                    }
                }
            }
            ShowDataForSelectedTab();
        }

        private void OnDefinitionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                var itemsRemoved = new HashSet<string>(args.OldItems.Cast<NwrwDryWeatherFlowDefinition>()
                                                                            .Select(item => item.Name));

                var providers = Data as IEnumerable<IDataRowProvider>;
                if (providers == null) return;

                var rows = providers.SelectMany(pr => pr.Rows);
                foreach (var row in rows)
                {
                    var nwrwDataRow = row as NwrwDataRow;
                    if (nwrwDataRow != null && itemsRemoved.Contains(nwrwDataRow.FirstDryWeatherFlowId))
                    {
                        nwrwDataRow.FirstDryWeatherFlowId = NwrwDryWeatherFlowDefinition.DefaultDwaId;
                    }

                    if (nwrwDataRow != null && itemsRemoved.Contains(nwrwDataRow.LastDryWeatherFlowId))
                    {
                        nwrwDataRow.LastDryWeatherFlowId = NwrwDryWeatherFlowDefinition.DefaultDwaId;
                    }
                }
            }
            ShowDataForSelectedTab();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && nwrwDryWeatherFlowDefinitions != null)
            {
                nwrwDryWeatherFlowDefinitions.CollectionChanged -= OnDefinitionsOnCollectionChanged;
                ((INotifyPropertyChanged)nwrwDryWeatherFlowDefinitions).PropertyChanged -= OnDefinitionsOnPropertyChanged;
                nwrwDryWeatherFlowDefinitions = null;
            }

            base.Dispose(disposing);
        }
    }
}