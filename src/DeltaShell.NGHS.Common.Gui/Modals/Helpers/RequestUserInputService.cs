using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.Modals.ViewModels;
using DeltaShell.NGHS.Common.Gui.Modals.Views;

namespace DeltaShell.NGHS.Common.Gui.Modals.Helpers
{
    /// <summary>
    /// <see cref="RequestUserInputService{TEnum}"/> implements the interface with which
    /// user input can be requested.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <remarks>
    /// <see cref="TEnum"/> is expected to be an enum.
    /// </remarks>
    /// <seealso cref="IRequestUserInputService{TEnum}" />
    public sealed class RequestUserInputService<TEnum> : IRequestUserInputService<TEnum>
        where TEnum : struct
    {
        public TEnum? RequestUserInput(string title, string text)
        {
            Ensure.NotNull(title, nameof(title));
            Ensure.NotNull(text, nameof(text));

            var modalData = new UserInputModalViewModel<TEnum>
            {
                Title = title,
                Text = text,
            };

            var modal = new UserInputModalView(modalData);
            modal.ShowDialog();

            return modalData.Result;
        }
    }
}