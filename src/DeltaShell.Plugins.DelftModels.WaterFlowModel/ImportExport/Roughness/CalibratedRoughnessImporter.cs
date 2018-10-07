using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Roughness;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public class CalibratedRoughnessImporter : IFileImporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CalibratedRoughnessImporter));

        #region Implementation of IFileImporter

        public string Name
        {
            get { return "Calibrated Roughness File"; }
        }

        public string Category { get; private set; }
        
        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IList<RoughnessSection>);
            }
        }

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
            get { return "Ini Files (*.ini)|*.ini"; }
        }
        
        public bool OpenViewAfterImport { get { return false; } }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target)
        {
            var roughnessSections = target as IList<RoughnessSection>;
            if (roughnessSections == null) return null;
            var firstRoughnessSection = roughnessSections.FirstOrDefault();
            if (firstRoughnessSection == null) return null;
            var network = firstRoughnessSection.Network;
            
            try
            {
                RoughnessDataFileReader.ReadFile(path, network, roughnessSections, isCalibratedRoughness: true);
            }
            catch (FileReadingException exception)
            {
                log.Error(exception);
            }
            

            return target;
        }        
        #endregion
    }
}