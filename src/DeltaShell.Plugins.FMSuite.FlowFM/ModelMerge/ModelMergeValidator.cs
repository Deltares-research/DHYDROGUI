using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelMerge
{
    /// <summary>
    /// Class for validating whether a <see cref="WaterFlowFMModel"/> can be imported
    /// in another <see cref="WaterFlowFMModel"/>.
    /// </summary>
    public class ModelMergeValidator
    {
        private HashSet<string> existingNetworkItemsLookup;

        public ModelMergeValidator()
        {
            existingNetworkItemsLookup = new HashSet<string>();
            DuplicateNames = new HashSet<string>();
        }
        
        /// <summary>
        /// Gets the collection of duplicate names found during the validation.
        /// </summary>
        public HashSet<string> DuplicateNames { get; }
        
        /// <summary>
        /// Validates whether the <paramref name="newModel"/> can be safely merged into
        /// the <paramref name="existingModel"/>.
        /// </summary>
        /// <param name="existingModel">The existing <see cref="WaterFlowFMModel"/>.</param>
        /// <param name="newModel">The new <see cref="WaterFlowFMModel"/> to be merged into the existing model.</param>
        /// <returns><c>True</c> if the models can be safely merged. <c>False</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public bool Validate(WaterFlowFMModel existingModel, WaterFlowFMModel newModel)
        {
            Ensure.NotNull(existingModel, nameof(existingModel));
            Ensure.NotNull(newModel, nameof(newModel));

            existingNetworkItemsLookup = CreateExistingItemsLookup(existingModel.Network);
            ValidateNamesOfNewNetwork(newModel.Network);

            if (DuplicateNames.Any())
            {
                return false;
            }
            
            return true;
        }
        
        private static HashSet<string> CreateExistingItemsLookup(IHydroNetwork originalNetwork)
        {
            var lookup = new HashSet<string>();

            originalNetwork.Branches.ForEach(b => lookup.Add(b.Name));
            originalNetwork.Nodes.ForEach(n => lookup.Add(n.Name));

            originalNetwork.Bridges.ForEach(b => lookup.Add(b.Name));
            originalNetwork.Compartments.ForEach(c => lookup.Add(c.Name));
            originalNetwork.CompositeBranchStructures.ForEach(cbs => lookup.Add(cbs.Name));
            originalNetwork.CrossSectionSectionTypes.ForEach(csst => lookup.Add(csst.Name));
            originalNetwork.CrossSections.ForEach(cs => lookup.Add(cs.Name));
            originalNetwork.Culverts.ForEach(c => lookup.Add(c.Name));
            originalNetwork.Gates.ForEach(g => lookup.Add(g.Name));
            originalNetwork.LateralSources.ForEach(ls => lookup.Add(ls.Name));
            originalNetwork.Links.ForEach(l => lookup.Add(l.Name));
            originalNetwork.ObservationPoints.ForEach(o => lookup.Add(o.Name));
            originalNetwork.Orifices.ForEach(o => lookup.Add(o.Name));
            originalNetwork.OutletCompartments.ForEach(oc => lookup.Add(oc.Name));
            originalNetwork.Pipes.ForEach(p => lookup.Add(p.Name));
            originalNetwork.Pumps.ForEach(p => lookup.Add(p.Name));
            originalNetwork.Retentions.ForEach(r => lookup.Add(r.Name));
            originalNetwork.Routes.ForEach(r => lookup.Add(r.Name));
            originalNetwork.SewerConnections.ForEach(sc => lookup.Add(sc.Name));
            originalNetwork.SharedCrossSectionDefinitions.ForEach(scsd => lookup.Add(scsd.Name));
            originalNetwork.Weirs.ForEach(w => lookup.Add(w.Name));
            
            return lookup;
        }

        private void ValidateNamesOfNewNetwork(IHydroNetwork newModelNetwork)
        {
            newModelNetwork.Branches.ForEach(b => CheckIfItemIsUnique(b.Name, "branch"));
            newModelNetwork.Nodes.ForEach(n => CheckIfItemIsUnique(n.Name, "node"));

            newModelNetwork.Bridges.ForEach(b => CheckIfItemIsUnique(b.Name, "bridge"));
            newModelNetwork.Compartments.ForEach(c => CheckIfItemIsUnique(c.Name, "compartment"));
            newModelNetwork.CompositeBranchStructures.ForEach(cbs => CheckIfItemIsUnique(cbs.Name, "composite structure"));
            newModelNetwork.CrossSections.ForEach(cs => CheckIfItemIsUnique(cs.Name, "cross-section"));
            newModelNetwork.Culverts.ForEach(c => CheckIfItemIsUnique(c.Name, "culvert"));
            newModelNetwork.Gates.ForEach(g => CheckIfItemIsUnique(g.Name, "gate"));
            newModelNetwork.LateralSources.ForEach(ls => CheckIfItemIsUnique(ls.Name, "lateral source"));
            newModelNetwork.Links.ForEach(l => CheckIfItemIsUnique(l.Name, "link"));
            newModelNetwork.ObservationPoints.ForEach(o => CheckIfItemIsUnique(o.Name, "observation point"));
            newModelNetwork.Pumps.ForEach(p => CheckIfItemIsUnique(p.Name, "pump"));
            newModelNetwork.Retentions.ForEach(r => CheckIfItemIsUnique(r.Name, "retention"));
            newModelNetwork.Routes.ForEach(r => CheckIfItemIsUnique(r.Name, "route"));
            newModelNetwork.SharedCrossSectionDefinitions.ForEach(scsd => CheckIfItemIsUnique(scsd.Name, "shared cross-section definition"));
            newModelNetwork.Weirs.ForEach(w => CheckIfItemIsUnique(w.Name, "weir"));
        }

        private void CheckIfItemIsUnique(string name, string type)
        {
            if (existingNetworkItemsLookup.Contains(name))
            {
                DuplicateNames.Add($"{name} ({type})");
            }
        }
    }
}