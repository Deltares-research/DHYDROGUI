using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deltares.IO.FewsPI;
using DeltaShell.Plugins.Fews.Assemblers;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using nl.wldelft.fews.pi;
using nl.wldelft.util.timeseries;

namespace DeltaShell.Plugins.Fews
{
    public class PiProfilesExporter // DISABLE FOR RELEASE OF SOBEK 3.0 :IFileExporter
    {
        readonly private static ILog log = LogManager.GetLogger(typeof(PiProfilesExporter));
        private string filePath;
        private readonly IList<NetworkCoverage> networkCoveragesToExport = new List<NetworkCoverage>();

        public string Name
        {
            get
            {
                return "FEWS-PI Longitudinal Profiles Exporter";
            }
        }

        public bool Export(object item, string path)
        {
            filePath = path;

            var networkCoverage = item as NetworkCoverage;
            if (networkCoverage != null)
            {
                AddInput(networkCoverage);
                return Execute();
            }

            log.ErrorFormat("Type {0} is not supported as export item for pi-profiles", item.GetType());

            return false;
        }

        public IEnumerable<Type> SourceTypes()
        {
           yield return typeof(NetworkCoverage);
           yield return typeof(FeatureCoverage);
        }

        public string FileFilter
        {
            get { return "FEWS-PI xml files (*.xml)|*.xml"; }
        }

        #region properties/methods based on pseudocode commandline request

        private bool Execute()
        {
            if (string.IsNullOrEmpty(filePath))
            {
                log.Error("File path to export has not been set.");
                return false;
            }

            if (networkCoveragesToExport.Count == 0)
            {
                log.Error("No items to export to pi-profiles.");
                return false;
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            IList<TimeSeriesArray> timeSeriesArrays = new List<TimeSeriesArray>();

            //write network coverages
            foreach (var networkCoverage in networkCoveragesToExport)
            {
                SetNetworkCoverageToProfilesComplexType(networkCoverage, timeSeriesArrays);
            }
            networkCoveragesToExport.Clear();

            PiProfilesWriter piProfilesWriter = new PiProfilesWriter(filePath);
            piProfilesWriter.write(new TimeSeriesArrays(timeSeriesArrays.ToArray()));
            piProfilesWriter.close();
            return true;
        }

        private void AddInput(NetworkCoverage networkCoverage)
        {
            networkCoveragesToExport.Add(networkCoverage);
        }

        #endregion

        #region private methods

        private static void SetNetworkCoverageToProfilesComplexType(NetworkCoverage networkCoverage, IList<TimeSeriesArray> timeSeriesArrays)
        {
            var branches = networkCoverage.Locations.AllValues.Select(l => l.Branch).Distinct().ToList();
            var timeSteps = networkCoverage.Time.AllValues.ToArray();

            DateTime startDate = timeSteps.FirstOrDefault();
            DateTime endDate = timeSteps.LastOrDefault();
            
            for (int i = 0; i < branches.Count(); i++)
            {
                IList<INetworkLocation> locationsOfBranch = networkCoverage.GetLocationsForBranch(branches[i]);

                DefaultTimeSeriesHeader timeSeriesHeader = new DefaultTimeSeriesHeader();
                TimeStep timeStep = SimpleEquidistantTimeStep.getInstance(Java2DotNetHelper.TicksToMillis(endDate.Ticks-startDate.Ticks));
                TimeSeriesArray timeSeriesArray = new TimeSeriesArray(TimeSeriesArray.Type.COVERAGE, 
                    timeSeriesHeader, timeStep);

                ProfilesComplexTypeAssembler.SetTimeEventValues(timeSeriesArray, networkCoverage, null, locationsOfBranch);
                timeSeriesArrays.Add(timeSeriesArray);
            }
        }

        #endregion
    }
}
