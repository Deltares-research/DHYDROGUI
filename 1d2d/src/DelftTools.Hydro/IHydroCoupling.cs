using System;
using System.Collections.Generic;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Interface for Coupling <see cref="IHydroModel"/>.
    /// </summary>
    public interface IHydroCoupling
    {
        /// <summary>
        /// Whether or not the HydroModel coupling has been ended.
        /// </summary>
        bool HasEnded { get; }
        
        /// <summary>
        /// Prepares the instance before coupling.
        /// </summary>
        void Prepare();

        /// <summary>
        /// Ends the instance after coupling.
        /// </summary>
        void End();

        /// <summary>
        /// Create a link between source and target.
        /// </summary>
        /// <param name="source">Source to link from.</param>
        /// <param name="target">Target to link to.</param>
        /// <returns>
        /// <see cref="HydroLink"/> of the created link between <paramref name="source"/> and <paramref name="target"/>.<br/>
        /// If no link can be created <c>null</c> is returned.
        /// </returns>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        IHydroLink CreateLink(IHydroObject source, IHydroObject target);

        /// <summary>
        /// Determines if the source is a linkable <see cref="IHydroObject"/> for <see cref="IHydroCoupling"/>.
        /// </summary>
        /// <param name="source">The <see cref="IHydroObject"/> to link from.</param>
        /// <returns>True if it can link for the <see cref="IHydroCoupling"/>, else false.</returns>
        bool CanLink(IHydroObject source);
        
        /// <summary>
        /// Gets the hydro object that corresponds to the given item string.
        /// </summary>
        /// <param name="itemString"> The item string.</param>
        /// <returns> The corresponding hydro object. </returns>
        IEnumerable<IHydroObject> GetLinkHydroObjectsByItemString(string itemString);
    }
}