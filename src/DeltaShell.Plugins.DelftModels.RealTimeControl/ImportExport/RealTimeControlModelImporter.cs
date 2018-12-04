using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.Reflection;
using DeltaShell.Dimr;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public class RealTimeControlModelImporter : IDimrModelFileImporter
    {
        private static ILog Log = LogManager.GetLogger(typeof(RealTimeControlModelImporter));

        public bool CanImportOn(object targetObject)
        {
            var hydroModel = targetObject as HydroModel.HydroModel;
            return hydroModel != null ||
                   hydroModel.Activities.Any(a => a.GetType().Implements(typeof(RealTimeControlModel)));
        }

        public object ImportItem(string path, object target = null)
        {
            return RealTimeControlModelXmlReader.Read(path);
        }

        public string Name { get { return "RTC-Tools xml files"; } }

        public string Category { get { return "Xml files"; } }

        public Bitmap Image { get { return Properties.Resources.brick_add; } }

        public IEnumerable<Type> SupportedItemTypes { get { yield return typeof(HydroModel.HydroModel); } }

        public bool CanImportOnRootLevel { get { return false; } }

        public string FileFilter { get { return "xml files|*.xml"; } }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

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
