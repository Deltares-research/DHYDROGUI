using Ranorex;
using Ranorex.Core.Repository;
using Ranorex.Core.Testing;
using WinForms = System.Windows.Forms;

namespace DHYDRO.Code
{
    /// <summary>
    ///     Creates a Ranorex user code collection. A collection is used to publish user code methods to the user code library.
    /// </summary>
    [UserCodeCollection]
    public class UserCodeLibrary
    {
        // You can use the "Insert New User Code Method" functionality from the context menu,
        // to add a new method with the attribute [UserCodeMethod].


        /// <summary>
        ///     Clicks the provided <paramref name="repoItemInfo" />, if the item exists with 1000 milliseconds.
        /// </summary>
        /// <param name="repoItemInfo">
        ///     The repository item to click if it exists.
        /// </param>
        /// <param name="waitPeriodInMilliSeconds">
        ///     The period to wait for the item to exist.
        /// </param>
        [UserCodeMethod]
        public static void ClickIfExists(RepoItemInfo repoItemInfo, int waitPeriodInMilliSeconds)
        {
            if (!repoItemInfo.Exists(Duration.FromMilliseconds(waitPeriodInMilliSeconds)))
            {
                return;
            }

            Report.Log(
                ReportLevel.Info,
                "Mouse",
                $"(Optional Action)\r\nMouse Left Click item '{repoItemInfo.Name}' at Center.",
                repoItemInfo);

            repoItemInfo.FindAdapter<Unknown>().Click();
            Delay.Duration(500, false);
        }
    }
}