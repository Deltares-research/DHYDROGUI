using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    /// <summary>
    /// Defines the loading coupling behaviour for a <see cref="RainfallRunoffModel"/>.
    /// </summary>
    public class RainfallRunoffHydroCoupling : HydroCoupling
    {
        private readonly Dictionary<string, SobekRRLink[]> lateralToCatchmentLookup;
        private IDictionary<string, Catchment> catchmentsByName;
        private readonly IDrainageBasin basin;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RainfallRunoffHydroCoupling"/> class.
        /// </summary>
        /// <param name="basin"> The drainage basin of the model. </param>
        /// <param name="lateralToCatchmentLookup"> A lookup containing the lateral to catchment information. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="basin"/> or <see cref="lateralToCatchmentLookup"/> is <c>null</c>.
        /// </exception>
        public RainfallRunoffHydroCoupling(IDrainageBasin basin, Dictionary<string, SobekRRLink[]> lateralToCatchmentLookup)
        {
            Ensure.NotNull(basin, nameof(basin));
            Ensure.NotNull(lateralToCatchmentLookup, nameof(lateralToCatchmentLookup));

            this.basin = basin;
            this.lateralToCatchmentLookup = lateralToCatchmentLookup;
        }

        /// <inheritdoc />
        public override void Prepare()
        {
            catchmentsByName = basin.AllCatchments.ToDictionary(c => c.Name, StringComparer.InvariantCultureIgnoreCase);
        }
        
        /// <inheritdoc/>
        /// <remarks>
        /// In some cases during the creating of links an existing link needs to be removed before the correct link can be made, this will be done within the CreateLink.<br/>
        /// During loading the RR will load a model-link to a lateral as a link to a RR boundary.<br/>
        /// The Runoff boundary will have the same name as the lateral.<br/>
        /// This Runoff boundary should not exist after all models are loaded and linked.<br/>
        /// </remarks>
        public override IHydroLink CreateLink(IHydroObject source, IHydroObject target)
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(target, nameof(target));
            
            RemoveReplaceableCatchmentLinks(source, target);

            if (source.CanLinkTo(target))
            {
                return source.LinkTo(target);
            }

            return null;
        }

        public override bool CanLink(IHydroObject source)
        {
            return source is Catchment;
        }

        /// <summary>
        /// Method to remove replaceable catchment links, e.g. links between a catchment and a RR boundary which should be a model link to a lateral.
        /// </summary>
        /// <param name="source">Source to link from.</param>
        /// <param name="target">Target to link to.</param>
        /// <remarks>
        /// During loading the RR will load a model-link to a lateral as a link to a RR boundary.<br/>
        /// The Runoff boundary will have the same name as the lateral.<br/>
        /// This Runoff boundary should not exist after all models are loaded and linked.<br/>
        /// Thus the Runoff boundary is removed before the link between the catchment and the lateral is created.<br/>
        /// </remarks>
        private void RemoveReplaceableCatchmentLinks(IHydroObject source, IHydroObject target)
        {
            if (lateralToCatchmentLookup.Count == 0)
            {
                return;
            }

            Catchment catchment = GetCatchmentByName(source.Name);
            if (catchment == null)
            {
                return;
            }

            if (lateralToCatchmentLookup.ContainsKey(target.Name))
            {
                catchment.Basin.Boundaries.RemoveAllWhere(boundary => boundary.Name.EqualsCaseInsensitive(target.Name));
            }
        }
        
        private Catchment GetCatchmentByName(string name)
        {
            catchmentsByName.TryGetValue(name, out Catchment catchment);
            return catchment;
        }
    }
}