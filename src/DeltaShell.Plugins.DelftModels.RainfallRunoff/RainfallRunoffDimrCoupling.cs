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
    /// Defines the dimr coupling behaviour for a <see cref="RainfallRunoffModel"/>.
    /// </summary>
    public class RainfallRunoffDimrCoupling : HydroCoupling
    {
        private readonly Dictionary<string, SobekRRLink[]> lateralToLinkableObjects;
        private readonly Dictionary<string, string> linkableObjectToRunoffBoundary = new Dictionary<string, string>();
        private readonly IDrainageBasin basin;

        private IDictionary<string, IHydroObject> linkableObjectsByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RainfallRunoffDimrCoupling"/> class.
        /// </summary>
        /// <param name="basin"> The drainage basin of the model. </param>
        /// <param name="lateralToLinkableObjects"> A lookup containing the lateral to catchment/wwtp information. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="basin"/> or <see cref="lateralToLinkableObjects"/> is <c>null</c>.
        /// </exception>
        public RainfallRunoffDimrCoupling(IDrainageBasin basin, Dictionary<string, SobekRRLink[]> lateralToLinkableObjects)
        {
            Ensure.NotNull(basin, nameof(basin));
            Ensure.NotNull(lateralToLinkableObjects, nameof(lateralToLinkableObjects));

            this.basin = basin;
            this.lateralToLinkableObjects = lateralToLinkableObjects;
        }

        /// <inheritdoc/>
        public override void Prepare()
        {
            linkableObjectsByName = basin.AllCatchments.Cast<IHydroObject>()
                                         .Concat(basin.WasteWaterTreatmentPlants)
                                         .ToDictionary(c => c.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets the hydro object item string.
        /// </summary>
        /// <param name="itemString"> The item string. </param>
        /// <returns> The matching data item. </returns>
        /// <remarks>
        /// <paramref name="itemString"/> cannot be null.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when
        /// - <paramref name="itemString"/> does not contain 3 elements
        /// - category in <paramref name="itemString"/> is unknown
        /// - feature in <paramref name="itemString"/> is unknown
        /// </exception>
        public override IEnumerable<IHydroObject> GetLinkHydroObjectsByItemString(string itemString)
        {
            string[] stringParts = itemString.Split('/');

            if (stringParts.Length != 3)
            {
                throw new ArgumentException($"{itemString} should contain a category, feature name and a parameter name.");
            }

            string category = stringParts[0];
            string featureName = stringParts[1];

            IList<IHydroObject> hydroObject = GetBasinHydroObjects(category, featureName);

            if (hydroObject == null)
            {
                throw new ArgumentException($"feature {featureName} in {itemString} cannot be found in the Rainfall Runoff model.");
            }

            return hydroObject;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// In some cases during the creating of links an existing link needs to be removed before the correct link can be made,
        /// this will be done within the CreateLink.<br/>
        /// During importing the RR will import a model-link to a lateral as a link to a RR boundary.<br/>
        /// The Runoff boundary will have the same name as the lateral.<br/>
        /// This Runoff boundary should not exist after all models are imported and linked.<br/>
        /// </remarks>
        public override IHydroLink CreateLink(IHydroObject source, IHydroObject target)
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(target, nameof(target));

            RemoveReplaceableLinks(source, target);

            if (source.CanLinkTo(target))
            {
                return source.LinkTo(target);
            }

            return null;
        }

        /// <inheritdoc/>
        public override bool CanLink(IHydroObject source)
        {
            return source is Catchment ||
                   source is WasteWaterTreatmentPlant;
        }

        /// <summary>
        /// Method to remove replaceable catchment/WWTP links,
        /// e.g. links between a catchment/WWTP and a RR boundary which should be a model link to a lateral.
        /// </summary>
        /// <param name="source">Source to link from.</param>
        /// <param name="target">Target to link to.</param>
        /// <remarks>
        /// During importing the RR will import a model-link to a lateral as a link to a RR boundary.<br/>
        /// This Runoff boundary should not exist after all models are imported and linked.<br/>
        /// Thus the Runoff boundary is removed before the link between the catchment and the lateral is created.<br/>
        /// </remarks>
        private void RemoveReplaceableLinks(IHydroObject source, IHydroObject target)
        {
            if (lateralToLinkableObjects.Count == 0 && linkableObjectToRunoffBoundary.Count == 0)
            {
                return;
            }

            if (!linkableObjectsByName.TryGetValue(source.Name, out IHydroObject linkableObject))
            {
                return;
            }

            if (lateralToLinkableObjects.ContainsKey(target.Name))
            {
                RemoveRunoffBoundaries(linkableObject, target.Name);
            }
            else if (linkableObjectToRunoffBoundary.TryGetValue(linkableObject.Name, out string runoffBoundaryName))
            {
                RemoveRunoffBoundaries(linkableObject, runoffBoundaryName);
            }
        }

        private static void RemoveRunoffBoundaries(IHydroObject linkableObject, string boundaryName)
        {
            var drainageBasin = (IDrainageBasin)linkableObject.Region;
            drainageBasin.Boundaries.RemoveAllWhere(boundary => boundary.Name.EqualsCaseInsensitive(boundaryName));
        }

        private IList<IHydroObject> GetBasinHydroObjects(string category, string featureName)
        {
            var basinHydroObjects = new List<IHydroObject>();
            switch (category)
            {
                case "catchments":
                    if (linkableObjectsByName.TryGetValue(featureName, out IHydroObject linkableObject))
                    {
                        basinHydroObjects.Add(linkableObject);
                    }
                    else
                    {
                        basinHydroObjects.AddRange(GetObjectsLinkedToRunoffBoundaries(featureName));
                    }

                    break;
            }

            return basinHydroObjects;
        }

        private IEnumerable<IHydroObject> GetObjectsLinkedToRunoffBoundaries(string featureName)
        {
            if (!lateralToLinkableObjects.TryGetValue(featureName, out SobekRRLink[] runoffBoundaryLinks))
            {
                yield break;
            }

            foreach (SobekRRLink link in runoffBoundaryLinks)
            {
                if (linkableObjectsByName.TryGetValue(link.NodeFromId, out IHydroObject linkableObject))
                {
                    linkableObjectToRunoffBoundary.Add(linkableObject.Name, featureName);
                    yield return linkableObject;
                }
            }
        }
    }
}