using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekMeasurementStationsImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekMeasurementStationsImporter));

        private string displayName = "Measurement stations (observation points)";
        public override string DisplayName
        {
            get { return displayName; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

        protected override void PartialImport()
        {
            var listFiles = new List<string>();
            var sobekMeasurementLocations = new List<SobekMeasurementLocation>();

            foreach (var pathSplitted in SobekFileNames.SobekMeasurementStationsFileName.Split(new char[] { '|' }))
            {
                var fullPath = GetFilePath(pathSplitted);
                if (File.Exists(fullPath))
                {
                    listFiles.Add(fullPath);
                }
            }

            foreach (var path in listFiles)
            {
                sobekMeasurementLocations.AddRange(new SobekMeasurementLocationReader().Read(path));
            }

            var uniqueMeasurementLoacations = sobekMeasurementLocations.Distinct().ToList();
            var branches = HydroNetwork.Branches.ToDictionary(b => b.Name, b => b);
            var observationPoints = HydroNetwork.ObservationPoints.ToDictionary(o => o.Name, o => o);

            foreach (var measurementLocation in uniqueMeasurementLoacations)
            {
                if (!branches.ContainsKey(measurementLocation.BranchId))
                {
                    log.ErrorFormat("Could not import measurement location '{0} - {1}' because branch '{2}' doesn't exist.", measurementLocation.Id, measurementLocation.Name, measurementLocation.BranchId);
                    continue;
                }

                var offset = measurementLocation.Chainage;
                var branch = branches[measurementLocation.BranchId];

                if (offset > branch.Length)
                {
                    log.ErrorFormat("The chainage of lateral source '{0} - {1}' is out of the branch length. The chainage has been set from {2} to {3}.", measurementLocation.Id, measurementLocation.Name, offset, branch.Length);
                    offset = branch.Length;
                }

                var observationPoint = ObservationPoint.CreateDefault(branch);
                observationPoint.Name = measurementLocation.Id;
                observationPoint.LongName = measurementLocation.Name;
                observationPoint.Chainage = offset;
                observationPoint.Geometry = GeometryHelper.GetPointGeometry(branch, observationPoint.Chainage);
                AddOrReplaceObservationPoint(branch, observationPoint, observationPoints);
            }
        }

        private void AddOrReplaceObservationPoint(IBranch branch, ObservationPoint observationPoint, Dictionary<string, IObservationPoint> observationPoints)
        {
            if (observationPoints.ContainsKey(observationPoint.Name))
            {
                var targetObservationPoint = observationPoints[observationPoint.Name];
                targetObservationPoint.CopyFrom(observationPoint);
                targetObservationPoint.Chainage = observationPoint.Chainage;
                targetObservationPoint.Geometry = observationPoint.Geometry;
                if (targetObservationPoint.Branch != branch)
                {
                    targetObservationPoint.Branch.BranchFeatures.Remove(targetObservationPoint);
                    NetworkHelper.AddBranchFeatureToBranch(targetObservationPoint, branch, targetObservationPoint.Chainage);
                }
                return;
            }
            NetworkHelper.AddBranchFeatureToBranch(observationPoint, branch, observationPoint.Chainage);
        }
    }
}
