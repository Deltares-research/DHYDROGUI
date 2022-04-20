using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows
{
    public sealed class ConceptDataRowProvider<TConceptData, TRow> : IDataRowProvider
        where TRow : RainfallRunoffDataRow<TConceptData>, new()
        where TConceptData : CatchmentModelData 
    {
        private readonly ICatchmentModelDataSynchronizer synchronizer;

        public ConceptDataRowProvider(RainfallRunoffModel model, string name, ICatchmentModelDataSynchronizer customSynchronizer = null)
        {
            Model = model;
            Name = name;
            Filter = Array.Empty<Catchment>();

            synchronizer = customSynchronizer ?? new CatchmentModelDataSynchronizer<TConceptData>(model);
            synchronizer.OnAreaAddedOrModified = OnAreaChanged;
            synchronizer.OnAreaRemoved = OnAreaChanged;
        }

        public IEnumerable<Catchment> Filter { get; set; }

        public RainfallRunoffModel Model { get; private set; }
        public string Name { get; private set; }

        public IEnumerable<IDataRow> Rows
        {
            get
            {
                if (Model == null)
                {
                    yield break;
                }
                foreach(var modelData in Model.GetAllModelData().OfType<TConceptData>())
                {
                    if (!MatchesCatchmentSubSelection(modelData)) 
                        continue;

                    var row = new TRow();
                    row.Initialize(modelData);
                    yield return row;
                }
            }
        }
        
        public event EventHandler RefreshRequired;

        public void ClearFilter()
        {
            if (Filter != null)
            {
                Filter = new Catchment[0];
            }
        }

        public bool HasFilter()
        {
            return Filter.Any();
        }

        public void Disconnect()
        {
            synchronizer.Disconnect();
        }
        
        private void OnAreaChanged(CatchmentModelData area)
        {
            if (MatchesCatchmentSubSelection(area))
            {
                OnRefreshRequired();
            }
        }

        private void OnRefreshRequired()
        {
            if (RefreshRequired != null)
            {
                RefreshRequired(this, EventArgs.Empty);
            }
        }

        private bool MatchesCatchmentSubSelection(CatchmentModelData catchmentModelData)
        {
            return !HasFilter() || Filter.Contains(catchmentModelData.Catchment);
        }
    }
}