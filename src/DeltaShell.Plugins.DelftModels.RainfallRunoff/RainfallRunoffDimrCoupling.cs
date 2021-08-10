using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Dimr;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    /// <summary>
    /// Defines the dimr coupling behaviour for a <see cref="RainfallRunoffModel"/>.
    /// </summary>
    public class RainfallRunoffDimrCoupling : IDimrCoupling
    {
        private readonly Dictionary<string, SobekRRLink[]> lateralToCatchmentLookup;
        private readonly IDrainageBasin basin;

        private IDictionary<string, Catchment> catchmentsByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RainfallRunoffDimrCoupling"/> class.
        /// </summary>
        /// <param name="basin"> The drainage basin of the model. </param>
        /// <param name="lateralToCatchmentLookup"> A lookup containing the lateral to catchment information. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="basin"/> or <see cref="lateralToCatchmentLookup"/> is <c>null</c>.
        /// </exception>
        public RainfallRunoffDimrCoupling(IDrainageBasin basin, Dictionary<string, SobekRRLink[]> lateralToCatchmentLookup)
        {
            Ensure.NotNull(basin, nameof(basin));
            Ensure.NotNull(lateralToCatchmentLookup, nameof(lateralToCatchmentLookup));

            this.basin = basin;
            this.lateralToCatchmentLookup = lateralToCatchmentLookup;
        }

        /// <inheritdoc/>
        public bool HasEnded { get; private set; } = false;

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
        public IList<IHydroObject> GetLinkHydroObjectsByItemString(string itemString)
        {
            string[] stringParts = itemString.Split('/');

            if (stringParts.Length != 3)
            {
                throw new ArgumentException($"{itemString} should contain a category, feature name and a parameter name.");
            }

            string category = stringParts[0];
            string featureName = stringParts[1].Replace("_boundary", string.Empty);

            IList<IHydroObject> hydroObject = GetBasinHydroObjects(category, featureName);

            if (hydroObject == null)
            {
                throw new ArgumentException($"feature {featureName} in {itemString} cannot be found in the Rainfall Runoff model.");
            }

            return hydroObject;
        }

        /// <inheritdoc/>
        public void Prepare()
        {
            catchmentsByName = basin.AllCatchments.ToDictionary(c => c.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <inheritdoc/>
        public void End()
        {
            HasEnded = true;
        }

        private IList<IHydroObject> GetBasinHydroObjects(string category, string featureName)
        {
            var basinHydroObjects = new List<IHydroObject>();
            switch (category)
            {
                case "catchments":
                    Catchment catchment = GetCatchmentByName(featureName);
                    if (catchment != null)
                    {
                        basinHydroObjects.Add(catchment);
                    }
                    else
                    {
                        if (lateralToCatchmentLookup.TryGetValue(featureName, out var catchmentLinks))
                        {
                            foreach (SobekRRLink catchmentLink in catchmentLinks)
                            {
                                catchment = GetCatchmentByName(catchmentLink.NodeFromId);
                                if (catchment != null)
                                {
                                    basinHydroObjects.Add(catchment);
                                }
                            }
                        }
                    }
                    break;
            }

            return basinHydroObjects;
        }

        private Catchment GetCatchmentByName(string name)
        {
            catchmentsByName.TryGetValue(name, out Catchment catchment);
            return catchment;
        }
    }
}