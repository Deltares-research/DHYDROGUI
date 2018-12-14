using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public class RealTimeControlModelImporter : IDimrModelFileImporter
    {
        [ExcludeFromCodeCoverage]
        public bool CanImportOn(object targetObject)
        {
            return false;
        }

        public object ImportItem(string path, object target = null)
        {
            return RealTimeControlModelXmlReader.Read(path);
        }

        public string Name { get { return "RTC-Tools xml files"; } }

        public string Category { get { return "Xml files"; } }

        [ExcludeFromCodeCoverage]
        public Bitmap Image { get { return Properties.Resources.brick_add; } }

        public IEnumerable<Type> SupportedItemTypes { get { yield return typeof(HydroModel.HydroModel); } }
        
        public bool CanImportOnRootLevel { get { return false; } }

        public string FileFilter { get { return "xml files|*.xml"; } }

        [ExcludeFromCodeCoverage]
        public string TargetDataDirectory { get; set; }

        [ExcludeFromCodeCoverage]
        public bool ShouldCancel { get; set; }

        [ExcludeFromCodeCoverage]
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get { return false; } }
        public string MasterFileExtension
        {
            get { return "json"; }
        }
        public IEnumerable<string> SubFolders
        {
            get { yield return "rtc"; }
        }
    }
}
