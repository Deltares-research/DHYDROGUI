using System;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.IO.Helpers
{
    /// <summary>
    /// Represents a small data transfer object containing a quantity with its corresponding unit.
    /// </summary>
    public class QuantityUnitPair
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityUnitPair"/> class.
        /// </summary>
        /// <param name="quantity"> The quantity. </param>
        /// <param name="unit"> The unit. </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="quantity"/> or <paramref name="unit"/> is <c>null</c> or white space.
        /// </exception>
        public QuantityUnitPair(string quantity, string unit)
        {
            Ensure.NotNullOrWhiteSpace(quantity, nameof(quantity));
            Ensure.NotNullOrWhiteSpace(unit, nameof(unit));

            Quantity = quantity;
            Unit = unit;
        }

        /// <summary>
        /// The quantity.
        /// </summary>
        public string Quantity { get; }

        /// <summary>
        /// The unit.
        /// </summary>
        public string Unit { get; }
    }
}