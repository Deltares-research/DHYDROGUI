namespace DeltaShell.NGHS.Common.Gui.Modals.Helpers
{
    /// <summary>
    /// <see cref="IRequestUserInputService{TEnum}"/> defines the interface with which
    /// user input can be requested.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <remarks>
    /// <see cref="TEnum"/> is expected to be an enum.
    /// </remarks>
    public interface IRequestUserInputService<TEnum> where TEnum : struct
    {
        /// <summary>
        /// Requests a user choice from the specified <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="title">The title of the modal to be opened.</param>
        /// <param name="text">The text displayed in the pop up.</param>
        /// <returns>
        /// The choice of <typeparamref name="TEnum"/> made by the user.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="title"/> or <paramref name="text"/>
        /// are <c>null</c>.
        /// </exception>
        TEnum? RequestUserInput(string title, string text);
    }
}