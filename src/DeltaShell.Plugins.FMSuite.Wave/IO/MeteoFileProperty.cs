using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// Container for a property defined in a meteo file.
    /// </summary>
    public class MeteoFileProperty
    {
        /// <summary>
        /// Creates a new instance of <see cref="MeteoFileProperty"/>.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any parameter is <c>null</c>.</exception>
        public MeteoFileProperty(string property, string value)
        {
            Ensure.NotNull(property, nameof(property));
            Ensure.NotNull(value, nameof(value));

            Property = property;
            Value = value;
        }

        public string Property { get; }
        public string Value { get; }
    }
}