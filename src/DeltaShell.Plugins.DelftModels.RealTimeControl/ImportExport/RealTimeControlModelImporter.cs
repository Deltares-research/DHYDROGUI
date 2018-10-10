using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public class RealTimeControlModelImporter:IFileImporter
    {
        private static ILog Log = LogManager.GetLogger(typeof(RealTimeControlModelImporter));

        public bool CanImportOn(object targetObject)
        {
            /*var hydroModel = targetObject as HydroModel.HydroModel;
            return hydroModel == null || !hydroModel.Activities.Any(a => a.GetType().Implements(typeof(RealTimeControlModel)));*/
            return false;
        }

        public object ImportItem(string path, object target = null)
        {
            return CanImportOn(target) ? RealTimeControlModelReader.Read(path) : null;
        }

        public string Name { get { return "RTC-Tools xml files"; }  }
        public string Category { get { return "Xml files"; } }
        public Bitmap Image { get { return Properties.Resources.brick_add; } }
        //public IEnumerable<Type> SupportedItemTypes { get { yield return typeof(HydroModel.HydroModel); } }
        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield break; }
        }

        public bool CanImportOnRootLevel { get { return false; } }
        public string FileFilter { get { return "xml files|*.xml"; } }
        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get {return false;} }
    }

    public class RealTimeControlModelReader
    {
        public static RealTimeControlModel Read(string path)
        {
            return null;
        }
    }
}
