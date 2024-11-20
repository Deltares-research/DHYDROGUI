using System;
using System.Windows;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    /// <summary>
    /// Class to inquire information from the user.
    /// </summary>
    public class InquiryHelper : IInquiryHelper
    {
        /// <summary>
        /// Inquires the user about the <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The query the user should answer.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is <c>null</c>.</exception>
        /// <returns><c>true</c> if the user confirmed, <c>false</c> otherwise.</returns>
        public bool InquireContinuation(string query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            MessageBoxResult dialog = MessageBox.Show(
                query,
                Resources.RealTimeControlModelNodePresenter_WhenAlreadyAssigned_OutputLocation_GivesWarning,
                MessageBoxButton.OKCancel);
            return dialog == MessageBoxResult.OK;
        }
    }
}