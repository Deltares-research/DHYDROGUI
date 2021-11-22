using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;

namespace DelftTools.Hydro.Structures
{
    /// <summary>
    /// Provides extension methods for <see cref="IManhole"/>.
    /// </summary>
    public static class ManholeExtensions
    {
        /// <summary>
        /// Gets the internal sewer connections of the <paramref name="manhole"/>.
        /// </summary>
        /// <param name="manhole"> The manhole. </param>
        /// <returns>
        /// An <see cref="IEnumerable{ISewerConnection}"/> with the internal sewer connection.
        /// </returns>
        public static IEnumerable<ISewerConnection> InternalConnections(this IManhole manhole) =>
            manhole.IncomingBranches
                   .Concat(manhole.OutgoingBranches)
                   .OfType<ISewerConnection>()
                   .Where(b => b.Source == b.Target)
                   .Distinct();

        /// <summary>
        /// Gets the structures defined on the internal sewer connections of the <paramref name="manhole"/>.
        /// </summary>
        /// <param name="manhole"> The manhole. </param>
        /// <returns>
        /// An <see cref="IEnumerable{IStructure1D}"/> with the internal sewer connection structures.
        /// </returns>
        public static IEnumerable<IStructure1D> InternalStructures(this IManhole manhole) => 
            manhole.InternalConnections().SelectMany(s => s.GetStructuresFromBranchFeatures());

        /// <summary>
        /// Gets the incoming and outgoing pipes of the <paramref name="manhole"/>.
        /// </summary>
        /// <param name="manhole"> The manhole. </param>
        /// <returns>
        /// An <see cref="IEnumerable{IPipe}"/> with the incoming and outgoing pipes.
        /// </returns>
        public static IEnumerable<IPipe> Pipes(this IManhole manhole) => 
            manhole.IncomingBranches.Concat(manhole.OutgoingBranches).OfType<IPipe>();

        /// <summary>
        /// Gets the outlet compartments of the <paramref name="manhole"/>.
        /// </summary>
        /// <param name="manhole"> The manhole. </param>
        /// <returns>
        /// An <see cref="IEnumerable{OutletCompartment}"/> with the outlet compartments.
        /// </returns>
        public static IEnumerable<OutletCompartment> OutletCompartments(this IManhole manhole) => 
            manhole?.Compartments?.OfType<OutletCompartment>();

        /// <summary>
        /// Gets the incoming pipes of the <paramref name="manhole"/>.
        /// </summary>
        /// <param name="manhole"> The manhole. </param>
        /// <returns>
        /// An <see cref="IEnumerable{IPipe}"/> with the incoming pipes.
        /// </returns>
        public static IEnumerable<IPipe> IncomingPipes(this IManhole manhole) => manhole.IncomingBranches.OfType<IPipe>();

        /// <summary>
        /// Gets the outgoing pipes of the <paramref name="manhole"/>.
        /// </summary>
        /// <param name="manhole"> The manhole. </param>
        /// <returns>
        /// An <see cref="IEnumerable{IPipe}"/> with the outgoing pipes.
        /// </returns>
        public static IEnumerable<IPipe> OutgoingPipes(this IManhole manhole) => manhole.OutgoingBranches.OfType<IPipe>();

        // TODO: Move to a good location
        public static bool IndexInRange(this ICollection shapes, int index) => index >= 0 && index < shapes.Count;
    }
}