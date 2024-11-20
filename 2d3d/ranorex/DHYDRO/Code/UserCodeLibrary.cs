using System.IO;
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
        private static readonly DHYDRORepository repo = DHYDRORepository.Instance;

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
        
        /// <summary>
        ///     Sets and opens the specified path in select file dialog if the dialog exists.
        /// </summary>
        /// <param name="path">
        ///     The file path to set if the dialog exists.
        /// </param>
        /// <param name="waitPeriodInMilliSeconds">
        ///     The period to wait for the item to exist.
        /// </param>
        [UserCodeMethod]
        public static void SelectFileIfDialogExists(string path, int waitPeriodInMilliSeconds)
        {
            if (!repo.DialogSelectFile.SelfInfo.Exists(Duration.FromMilliseconds(waitPeriodInMilliSeconds)))
            {
                return;
            }

            if (!repo.DialogSelectFile.FileType.SelectedItemText.Contains(Path.GetExtension(path)))
            {
                return;
            }
            
            Report.Log(ReportLevel.Info, "Set value", $"Setting attribute Text to '{path}' on item 'DialogSelectFile.FieldFilePath'.", repo.DialogSelectFile.FieldFilePathInfo, new RecordItemIndex(1));
            repo.DialogSelectFile.FieldFilePath.Element.SetAttributeValue("Text", path);

            Report.Log(ReportLevel.Info, "Mouse", "Mouse Left Click item 'DialogSelectFile.ButtonOpen' at Center.", repo.DialogSelectFile.ButtonOpenInfo, new RecordItemIndex(2));
            repo.DialogSelectFile.ButtonOpen.Click();
        }
    }
}