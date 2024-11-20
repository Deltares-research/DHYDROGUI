using System.Collections;
using DelftTools.Shell.Gui.Swf;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui
{
    public class CatchmentModelDataTreeFolder : TreeFolder
    {
        public CatchmentModelDataTreeFolder(object parent, IEnumerable childItems, string text,
                                             FolderImageType imageType) : base(parent, childItems, text, imageType)
        {
        }
    }
}