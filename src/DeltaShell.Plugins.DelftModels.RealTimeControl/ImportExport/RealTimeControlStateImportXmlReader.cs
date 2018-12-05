using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlStateImportXmlReader
    {
        public static void Read(string stateImportFilePath, IList<ControlGroup> controlGroups)
        {
            var dataConfigObject = (TreeVectorFileXML)DelftConfigXmlFileParser.Read(stateImportFilePath);
        }
    }
}
