using System.Collections.Generic;
using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.WaveOutputData
{
    /// <summary>
    /// <see cref="CollectPropertyValueBehaviour"/> implements the
    /// <see cref="IDelftIniPropertyBehaviour"/> that stores the value relative
    /// to the provided relative directory associated with the provided property
    /// name in the provided hash set.
    /// </summary>
    /// <seealso cref="IDelftIniPropertyBehaviour"/>
    public class CollectPropertyValueBehaviour : IDelftIniPropertyBehaviour
    {
        private readonly string propertyName;
        private readonly HashSet<string> hashSet;
        private readonly string relativeDirectory;

        /// <summary>
        /// Creates a new <see cref="CollectPropertyValueBehaviour"/>.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="hashSet">The hash set.</param>
        /// <param name="relativeDirectory">The relative directory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public CollectPropertyValueBehaviour(string propertyName, 
                                             HashSet<string> hashSet, 
                                             string relativeDirectory)
        {
            Ensure.NotNull(propertyName, nameof(propertyName));
            Ensure.NotNull(hashSet, nameof(hashSet));
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));

            this.hashSet = hashSet;
            this.propertyName = propertyName;
            this.relativeDirectory = relativeDirectory;
        }

        public void Invoke(DelftIniProperty property, ILogHandler logHandler)
        {
            Ensure.NotNull(property, nameof(property));

            if (property.Name.Equals(propertyName))
            {
                hashSet.Add(Path.Combine(relativeDirectory, property.Value));
            }
        }
    }
}