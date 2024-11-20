using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    /// <summary>
    /// <see cref="IOrifice"/> extends the <see cref="IWeir"/> interface
    /// with the <see cref="MaxDischarge"/>.
    /// </summary>
    public interface IOrifice : IWeir
    {
        /// <summary>
        /// Gets or sets the maximum discharge.
        /// </summary>
        double MaxDischarge { get; set; }
    }
}