using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeltaShell.NGHS.Common.Gui.Modals.ViewModels
{
    /// <summary>
    /// <see cref="UserInputModalViewModelBase"/> defines an abstract view model
    /// for the <see cref="Views.UserInputModalView"/>.
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    public abstract class UserInputModalViewModelBase : INotifyPropertyChanged
    {
        private string text;
        private string title;
        private object internalResult = null;

        /// <summary>
        /// Gets the user input options.
        /// </summary>
        public abstract Array UserInputOptions { get; }

        /// <summary>
        /// Gets or sets the internal result.
        /// </summary>
        /// <remarks>
        /// This value is either <c>null</c> or one of the values of the <see cref="UserInputOptions"/>.
        /// </remarks>
        public object InternalResult
        {
            get => internalResult;
            set
            {
                if (Equals(internalResult, value))
                {
                    return;
                }

                internalResult = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the text displayed in the view.
        /// </summary>
        public string Text
        {
            get => text;
            set
            {
                if (Equals(Text, value))
                {
                    return;
                }

                text = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get => title;
            set
            {
                if (Equals(Title, value))
                {
                    return;
                }

                title = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}