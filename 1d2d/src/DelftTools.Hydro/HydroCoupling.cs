using System.Collections.Generic;
using System.Collections.Immutable;
using Deltares.Infrastructure.API.Guards;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Base HydroCoupling class.
    /// </summary>
    public class HydroCoupling : IHydroCoupling
    {
        public virtual bool HasEnded { get; protected set; }
        
        /// <inheritdoc/>
        public virtual void Prepare() {}

        /// <inheritdoc/>
        public virtual void End()
        {
            HasEnded = true;
        }

        /// <inheritdoc/>
        public virtual IHydroLink CreateLink(IHydroObject source, IHydroObject target)
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(target, nameof(target));
            
            if (source.CanLinkTo(target))
            {
                return source.LinkTo(target);
            }

            return null;
        }

        /// <inheritdoc/>
        /// <returns>False</returns>
        public virtual bool CanLink(IHydroObject source)
        {
            return false;
        }

        /// <inheritdoc/>
        /// <returns>Empty immutable List</returns>
        public virtual IEnumerable<IHydroObject> GetLinkHydroObjectsByItemString(string itemString)
        {
           return ImmutableList<IHydroObject>.Empty;
        }
    }
}