using System.Windows;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.Modals.ViewModels;

namespace DeltaShell.NGHS.Common.Gui.Modals.Views
{
    /// <summary>
    /// Interaction logic for UserFeedbackWindow.xaml
    /// </summary>
    public partial class UserInputModalView : Window
    {
        private readonly UserInputModalViewModelBase viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInputModalView"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="viewModel"/> is <c>null</c>.
        /// </exception>
        public UserInputModalView(UserInputModalViewModelBase viewModel)
        {
            Ensure.NotNull(viewModel, nameof(viewModel));
            this.viewModel = viewModel;
            DataContext = this.viewModel;

            InitializeComponent();
            ButtonCommand = new RelayCommand(UpdateAndClose);
        }

        /// <summary>
        /// Gets the button command assigned to the buttons generated from the corresponding viewmodel.
        /// </summary>
        public ICommand ButtonCommand { get; }

        private void UpdateAndClose(object arg)
        {
            viewModel.InternalResult = arg;
            Close();
        }
    }
}