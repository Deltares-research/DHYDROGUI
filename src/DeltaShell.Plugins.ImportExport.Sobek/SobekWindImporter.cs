using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekWindImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekWindImporter));

        public string Name
        {
            get { return "SOBEK Wind Data"; }
        }
        public string Description { get { return Name; } }
        public string Category { get; private set; }
        
        public Bitmap Image { get; private set; }
        
        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield break; }
            //          get { yield return typeof(WindFunction); }
        }

        public bool OpenViewAfterImport { get { return false; } }
        
        public bool CanImportOn(object targetObject)
        {
            return true;
        }
        
        public bool CanImportOnRootLevel
        {
            get { return false; }
        }
        
        public string FileFilter
        {
            get { return "SOBEK Wind Files (*.wnd)|*.wnd"; }
        }
        
        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }
        
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        
        public object ImportItem(string path, object target)
        {
            var targetWind = target;
            // var targetWind = (WindFunction) target;
            //
            // var winds = new SobekWindReader().Read(path);
            // // only uniform wind (constant in space) is supported.
            // var globalWind = winds.FirstOrDefault(w => w.IsGlobal);
            //
            // if (globalWind == null)
            // {
            //     Log.WarnFormat("No global wind data found in {0}", path);
            //     return targetWind;
            // }
            //
            // if (globalWind.IsConstantVelocity != globalWind.IsConstantDirection)
            // {
            //     Log.Error("Wind velocity and direction should be both of same type (constant/time series)");
            //     return targetWind;
            // }
            //
            // if (globalWind.IsConstantVelocity)
            // {
            //     targetWind.Velocity.DefaultValue = globalWind.ConstantVelocity;
            //     targetWind.Direction.DefaultValue = globalWind.ConstantDirection;
            // }
            // else
            // {
            //     FunctionHelper.CopyValuesFrom(globalWind.Wind, targetWind);
            // }
        
            return targetWind;
        }
    }
}