using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Utils.Collections.Generic;
using log4net;
using SharpMap.Api;
using SharpMap.Data.Providers;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// HydroNetworkFromGisImporter is a composite importer for all types of NetworkFeatureFromGisImporters
    /// </summary>
    public class HydroRegionFromGisImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroRegionFromGisImporter));
        private readonly IList<IFileBasedFeatureProvider> fileBasedFeatureProviders;
        private readonly Dictionary<string, Type> availableFeatureFromGisImporters = new Dictionary<string, Type>();
        private int snappingPrecision = 5;
        private IHydroRegion hydroRegion;

        public HydroRegionFromGisImporter()
        {
            fileBasedFeatureProviders = new List<IFileBasedFeatureProvider> {new ShapeFile(), new OgrFeatureProvider()};
            FeatureFromGisImporters = new EventedList<FeatureFromGisImporterBase>();
        }

        public IList<IFileBasedFeatureProvider> FileBasedFeatureProviders
        {
            get { return fileBasedFeatureProviders; }
        }

        public Dictionary<string,Type> AvailableFeatureFromGisImporters
        {
            get { return availableFeatureFromGisImporters; }
        }

        public EventedList<FeatureFromGisImporterBase> FeatureFromGisImporters { get; set; }
        public bool OpenViewAfterImport { get { return false; } }
        public IHydroRegion HydroRegion
        {
            get { return hydroRegion; }
            set
            {
                hydroRegion = value;
                InitAvailableFeatureImporters();
            }
        }

        public int SnappingPrecision
        {
            get { return snappingPrecision; }
            set { snappingPrecision = value; }
        }

        #region IFileImporter Members

        public virtual string Name
        {
            get { return "Model features from GIS"; }
        }
        public string Description { get { return Name; } }
        public string Category
        {
            get { return "Data"; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.HydroRegion; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof (IHydroNetwork);
                yield return typeof (HydroRegion);
                yield return typeof (IDrainageBasin);
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
            get { return ""; }
        }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public virtual object ImportItem(string path, object target = null)
        {
            var targetHydroRegion = target as IHydroRegion;

            HydroRegion = targetHydroRegion ?? new HydroRegion
                {
                    Name = "Imported Hydro Region",
                    SubRegions =
                        {
                            new HydroNetwork {Name = "Imported Network"},
                            new DrainageBasin {Name = "Imported Basin"}
                        }
                };

            RunAllFeatureFromGisImporters();
            FeatureFromGisImporters.Clear();
            return HydroRegion;
        }

        #endregion

        #region private
        
        private void InitAvailableFeatureImporters()
        {
            availableFeatureFromGisImporters.Clear();
            if (HydroRegion is HydroNetwork || HydroRegion is HydroRegion)
            {
                availableFeatureFromGisImporters.Add("Channels", typeof (ChannelFromGisImporter));
                availableFeatureFromGisImporters.Add("Cross Sections ZW", typeof (CrossSectionZWFromGisImporter));
                availableFeatureFromGisImporters.Add("Cross Sections Y'Z", typeof (CrossSectionYZFromGisImporter));
                availableFeatureFromGisImporters.Add("Cross Sections XYZ", typeof (CrossSectionXYZFromGisImporter));
                availableFeatureFromGisImporters.Add("Bridge", typeof (BridgeRectangularFromGisImporter));
                availableFeatureFromGisImporters.Add("Bridge tabulated (ZW)", typeof (BridgeZwFromGisImporter));
                availableFeatureFromGisImporters.Add("Bridge tabulated (YZ)", typeof (BridgeYzFromGisImporter));
                availableFeatureFromGisImporters.Add("Culvert", typeof (CulvertFromGisImporter));
                availableFeatureFromGisImporters.Add("Lateral sources", typeof (LateralSourceFromGisImporter));
                availableFeatureFromGisImporters.Add("Observation points", typeof (ObservationPointFromGisImporter));
                availableFeatureFromGisImporters.Add("Pump", typeof (PumpFromGisImporter));
                availableFeatureFromGisImporters.Add("Weir (simple weir)", typeof (SimpleWeirFromGisImporter));
            }
            if (HydroRegion is IDrainageBasin || HydroRegion is HydroRegion)
            {
                availableFeatureFromGisImporters.Add("Catchments", typeof (CatchmentFromGisImporter));
            }
        }

        private void RunAllFeatureFromGisImporters()
        {
            //run channels import first
            var channelImporters = FeatureFromGisImporters.OfType<ChannelFromGisImporter>().ToList();
            foreach (var importer in channelImporters)
            {
                RunFeatureFromGisImporter(importer);
            }
            //then the other importers
            var featureImporters = FeatureFromGisImporters.Where(importer => !(importer is ChannelFromGisImporter)).ToList();
            foreach (var importer in featureImporters)
            {
                RunFeatureFromGisImporter(importer);
            }
        }

        private void RunFeatureFromGisImporter(FeatureFromGisImporterBase importer)
        {
            try
            {
                importer.HydroRegion = HydroRegion;
                importer.SnappingTolerance = SnappingPrecision;
                importer.ImportItem(null);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("{0} failed to import from {1}. Exception: {2}.", importer.Name, importer.FeatureFromGisImporterSettings.Path, e.Message);
            }
        }

        #endregion
    }
}
