namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    /// <summary>
    /// Specifies the interface for classes that can be used to inquire information from
    /// the user.
    /// </summary>
    public interface IInquiryHelper
    {
        /// <summary>
        /// Gets the confirmation of a user.
        /// </summary>
        /// <param name="query">The query to which the user needs to answer.</param>
        /// <returns><c>true</c> if the user confirmed, <c>false</c> otherwise.</returns>
        bool InquireContinuation(string query);
    }
}