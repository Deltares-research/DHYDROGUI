using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    /// <summary>
    /// Defines the dimr coupling behaviour for a <see cref="WaterFlowFMModel"/>.
    /// </summary>
    public class WaterFlowFmDimrCoupling : IDimrCoupling
    {
        private readonly IHydroNetwork network;
        private Dictionary<string, ILateralSource> lateralSourcesByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaterFlowFmDimrCoupling"/> class.
        /// </summary>
        /// <param name="network"> The hydro network of the model. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="network"/> is <c>null</c>.
        /// </exception>
        public WaterFlowFmDimrCoupling(IHydroNetwork network)
        {
            Ensure.NotNull(network, nameof(network));

            this.network = network;
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
                throw new ArgumentException(string.Format("{0} should contain a category, feature name and a parameter name.",
                                                          itemString));
            }

            string category = stringParts[0];
            string featureName = stringParts[1];

            IHydroObject hydroObject = GetNetworkHydroObject(category, featureName);

            if (hydroObject == null)
            {
                throw new ArgumentException(string.Format("feature {0} in {1} cannot be found in the FM model.",
                                                          featureName, itemString));
            }

            return new List<IHydroObject> {hydroObject};
        }

        /// <inheritdoc/>
        public void Prepare()
        {
            lateralSourcesByName = network.LateralSources.ToDictionary(l => l.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <inheritdoc/>
        public void End()
        {
            HasEnded = true;
        }

        private IHydroObject GetNetworkHydroObject(string category, string featureName)
        {
            switch (category)
            {
                case "laterals":
                    lateralSourcesByName.TryGetValue(featureName, out ILateralSource lateralSource);
                    return lateralSource;
                default:
                    return null;
            }
        }
    }
}