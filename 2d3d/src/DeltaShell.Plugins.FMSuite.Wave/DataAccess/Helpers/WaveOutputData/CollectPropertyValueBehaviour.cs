﻿using System.Collections.Generic;
using System.IO;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.WaveOutputData
{
    /// <summary>
    /// <see cref="CollectPropertyValueBehaviour"/> implements the
    /// <see cref="IIniPropertyBehaviour"/> that stores the value relative
    /// to the provided relative directory associated with the provided property
    /// name in the provided hash set.
    /// </summary>
    /// <seealso cref="IIniPropertyBehaviour"/>
    public class CollectPropertyValueBehaviour : IIniPropertyBehaviour
    {
        private readonly string propertyKey;
        private readonly HashSet<string> hashSet;
        private readonly string relativeDirectory;

        /// <summary>
        /// Creates a new <see cref="CollectPropertyValueBehaviour"/>.
        /// </summary>
        /// <param name="propertyKey">Name of the property.</param>
        /// <param name="hashSet">The hash set.</param>
        /// <param name="relativeDirectory">The relative directory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public CollectPropertyValueBehaviour(string propertyKey, 
                                             HashSet<string> hashSet, 
                                             string relativeDirectory)
        {
            Ensure.NotNull(propertyKey, nameof(propertyKey));
            Ensure.NotNull(hashSet, nameof(hashSet));
            Ensure.NotNull(relativeDirectory, nameof(relativeDirectory));

            this.hashSet = hashSet;
            this.propertyKey = propertyKey;
            this.relativeDirectory = relativeDirectory;
        }

        public void Invoke(IniProperty property, ILogHandler logHandler)
        {
            Ensure.NotNull(property, nameof(property));

            if (property.Key.Equals(propertyKey))
            {
                hashSet.Add(Path.Combine(relativeDirectory, property.Value));
            }
        }
    }
}