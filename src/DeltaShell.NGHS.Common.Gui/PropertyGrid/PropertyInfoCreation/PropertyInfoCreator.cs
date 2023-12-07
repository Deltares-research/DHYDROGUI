using DelftTools.Shell.Gui;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation
{
    /// <summary>
    /// Class for creating <see cref="PropertyInfo"/> objects for the properties window.
    /// </summary>
    public sealed class PropertyInfoCreator
    {
        private readonly GuiContainer guiContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyInfoCreator"/>.
        /// </summary>
        /// <param name="guiContainer"> The GUI container which provides an <see cref="DelftTools.Shell.Gui.IGui"/> instance. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="guiContainer"/> is <c>null</c>.
        /// </exception>
        public PropertyInfoCreator(GuiContainer guiContainer)
        {
            Ensure.NotNull(guiContainer, nameof(guiContainer));
            this.guiContainer = guiContainer;
        }

        /// <summary>
        /// Creates a new <see cref="PropertyInfo"/> object for the specified creation context.
        /// </summary>
        /// <param name="creationContext"> Creation context that is specific to the underlying data. </param>
        /// <typeparam name="T"> The type of data to create the view info for. </typeparam>
        /// <typeparam name="TProperties">The type of object properties.</typeparam>
        /// <returns>
        /// A newly constructed <see cref="PropertyInfo{T,TProperties}"/> object.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="creationContext"/> is <c>null</c>.
        /// </exception>
        public PropertyInfo Create<T, TProperties>(IPropertyInfoCreationContext<T, TProperties> creationContext) 
            where TProperties : ObjectProperties<T>
        {
            Ensure.NotNull(creationContext, nameof(creationContext));

            return new PropertyInfo<T, TProperties>
            {
                AfterCreate = properties => creationContext.CustomizeProperties(properties, guiContainer)
            };
        }
    }
}