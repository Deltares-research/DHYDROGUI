using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;

namespace DHYDRO.Common.IO.ExtForce
{
    /// <summary>
    /// Represents the data contained in an external forcings file (*.ext).
    /// This file is referenced by the MDU file through the <c>ExtForceFile</c> property.
    /// </summary>
    public class ExtForceFileData
    {
        private readonly List<ExtForceData> forcings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtForceFileData"/> class.
        /// </summary>
        public ExtForceFileData()
            : this(Enumerable.Empty<ExtForceData>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtForceFileData"/> class.
        /// </summary>
        /// <param name="forcings">The collection of <see cref="ExtForceData"/> instances.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="forcings"/> is <c>null</c>.</exception>
        public ExtForceFileData(IEnumerable<ExtForceData> forcings)
        {
            Ensure.NotNull(forcings, nameof(forcings));
            this.forcings = forcings.ToList();
        }

        /// <summary>
        /// Gets the collection of external forcings.
        /// </summary>
        public IEnumerable<ExtForceData> Forcings
            => forcings.AsEnumerable();

        /// <summary>
        /// Adds an external forcing to the collection.
        /// </summary>
        /// <param name="forcing">The external forcing data to add.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="forcing"/> is <c>null</c>.</exception>
        public void AddForcing(ExtForceData forcing)
        {
            Ensure.NotNull(forcing, nameof(forcing));
            forcings.Add(forcing);
        }

        /// <summary>
        /// Adds multiple external forcings to the collection.
        /// </summary>
        /// <param name="forcingsToAdd">The external forcings data to add.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="forcingsToAdd"/> is <c>null</c>.</exception>
        public void AddMultipleForcings(IEnumerable<ExtForceData> forcingsToAdd)
        {
            Ensure.NotNull(forcingsToAdd, nameof(forcingsToAdd));
            forcings.AddRange(forcingsToAdd);
        }
    }
}