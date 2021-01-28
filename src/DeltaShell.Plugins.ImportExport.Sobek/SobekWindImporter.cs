using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekWindImporter : IFileImporter
    {
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
            return targetWind;
        }
    }
}