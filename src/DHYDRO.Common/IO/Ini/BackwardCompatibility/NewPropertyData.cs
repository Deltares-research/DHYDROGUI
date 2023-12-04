using DHYDRO.Common.Guards;

namespace DHYDRO.Common.IO.Ini.BackwardCompatibility
{
    /// <summary>
    /// Class that holds the data required to update legacy properties.
    /// </summary>
    public class NewPropertyData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewPropertyData"/> class.
        /// </summary>
        /// <param name="key">The new key for a property.</param>
        /// <param name="converter">The converter used to update the property value.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public NewPropertyData(string key, IPropertyUpdater converter)
        {
            Ensure.NotNull(key, nameof(key));
            Ensure.NotNull(converter, nameof(converter));

            Key = key;
            Updater = converter;
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public string Key { get; }
        
        /// <summary>
        /// Gets the updater.
        /// </summary>
        public IPropertyUpdater Updater { get; }
    }
}