using DelftTools.Shell.Gui;

namespace DeltaShell.NGHS.Common.Gui.PropertyGrid.PropertyInfoCreation
{
    /// <summary>
    /// Provides the creation context for a <see cref="PropertyInfoCreator"/>.
    /// The creation context is dependent of the type that needs to be represented in the table view.
    /// </summary>
    /// <typeparam name="T"> The type of feature. </typeparam>
    /// <typeparam name="TProperties"> The type of object properties. </typeparam>
    public interface IPropertyInfoCreationContext<in T, in TProperties>
        where TProperties : IObjectProperties<T>
    {
        /// <summary>
        /// Customize the properties object.
        /// </summary>
        /// <param name="properties"> The properties object. </param>
        /// <param name="guiContainer"> The container that holds the running <see cref="IGui"/> instance. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="properties"/> or <paramref name="guiContainer"/> is
        /// <c>null</c>.
        /// </exception>
        void CustomizeProperties(TProperties properties, GuiContainer guiContainer);
    }
}