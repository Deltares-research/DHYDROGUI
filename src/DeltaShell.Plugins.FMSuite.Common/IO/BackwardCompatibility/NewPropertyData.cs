using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility
{
    /// <summary>
    /// Class that holds the data required to update legacy properties.
    /// </summary>
    public class NewPropertyData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewPropertyData"/> class.
        /// </summary>
        /// <param name="name">The new name for a property.</param>
        /// <param name="converter">The converter used to update the property value.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public NewPropertyData(string name, IPropertyUpdater converter)
        {
            Ensure.NotNull(name, nameof(name));
            Ensure.NotNull(converter, nameof(converter));

            Name = name;
            Updater = converter;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the updater.
        /// </summary>
        public IPropertyUpdater Updater { get; }
    }
}