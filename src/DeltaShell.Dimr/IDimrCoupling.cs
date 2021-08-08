using DelftTools.Hydro;

namespace DeltaShell.Dimr
{
    /// <summary>
    /// Defines the dimr coupling behaviour for a <see cref="IDimrModel"/>.
    /// </summary>
    public interface IDimrCoupling
    {
        /// <summary>
        /// Whether or not the dimr coupling has been ended.
        /// </summary>
        bool HasEnded { get; }

        /// <summary>
        /// Gets the hydro object that corresponds to the given item string.
        /// </summary>
        /// <param name="itemString"> The item string.</param>
        /// <returns> The corresponding hydro object. </returns>
        IHydroObject GetLinkHydroObjectByItemString(string itemString);

        /// <summary>
        /// Prepares the instance before coupling.
        /// </summary>
        void Prepare();

        /// <summary>
        /// Ends the instance after coupling.
        /// </summary>
        void End();
    }
}