using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter
{
    /// <inheritdoc/>
    public class ManifestRetriever : IManifestRetriever
    {
        private const string fixedManifest = "Fixed";
        private readonly string fixedLocation;
        private readonly Assembly assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestRetriever"/> class.
        /// </summary>
        public ManifestRetriever()
        {
            string @namespace = GetType().Namespace;
            assembly = GetType().Assembly;

            fixedLocation = $"{@namespace}.{fixedManifest}.";

            FixedResources = GetResourcesNames(fixedLocation).ToArray();
        }

        /// <inheritdoc/>
        public IEnumerable<string> FixedResources { get; }

        /// <inheritdoc/>
        public Stream GetFixedStream(string fileName) =>
            assembly.GetManifestResourceStream($"{fixedLocation}{fileName}");

        private IEnumerable<string> GetResourcesNames(string location)
        {
            IEnumerable<string> resources = assembly.GetManifestResourceNames().Where(n => n.StartsWith(location));
            IEnumerable<string> resourcesNames = resources.Select(n => n.Replace(location, string.Empty));

            return resourcesNames;
        }
    }
}