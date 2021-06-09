using System;

namespace DeltaShell.NGHS.Common.Gui.Modals.ViewModels
{
    /// <summary>
    /// <see cref="UserInputModalViewModel{TEnum}"/> implements the <see cref="UserInputModalViewModelBase"/>
    /// as a generic class specifying the <typeparamref name="TEnum"/> used to define the options.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <seealso cref="UserInputModalViewModelBase" />
    public sealed class UserInputModalViewModel<TEnum> : UserInputModalViewModelBase
        where TEnum : struct
    {
        public override Array UserInputOptions => Enum.GetValues(typeof(TEnum));

        /// <summary>
        /// Gets the <see cref="UserInputModalViewModelBase.InternalResult"/> as
        /// the actual <typeparamref name="TEnum"/>.
        /// </summary>
        public TEnum? Result => (TEnum?)InternalResult;
    }
}