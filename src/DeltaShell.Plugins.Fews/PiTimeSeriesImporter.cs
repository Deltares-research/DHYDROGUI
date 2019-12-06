using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core;
using DelftTools.Units;
using Deltares.IO.FewsPI;
using DeltaShell.Plugins.Fews.Properties;
using log4net;
using nl.wldelft.fews.pi;
using TimeSeries = DelftTools.Functions.TimeSeries;

namespace DeltaShell.Plugins.Fews
{
    public class PiTimeSeriesImporter : IFileImporter
    {
        #region private members and data

        private static readonly ILog Log = LogManager.GetLogger(typeof (PiTimeSeriesImporter));

        #endregion

        public PiTimeSeriesImporter()
        {
            Name = "FEWS-PI Time Series";
        }

        public void Execute()
        {
            TimeSeries = GetTimeSeriesFromFile(FilePath).ToList();
        }

        public virtual IList<TimeSeries> TimeSeries { get; private set; }

        public TimeSeries GetTimeSeries(string locationId, string parameterId)
        {
            var name = locationId + "_" + parameterId;
            return TimeSeries.FirstOrDefault(ts => ts.Name == name);
        }

        public bool OpenViewAfterImport { get { return false; } }

        public IEnumerable<TimeSeries> SelectedTimeSeries { get; set; }

        protected IEnumerable<TimeSeries> GetTimeSeriesFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Log.ErrorFormat("File {0} does not exists.", path);
                yield break;
            }

            var piTimeSeriesReader = new PiTimeSeriesReader(path);
            var timeSeriesArrays = piTimeSeriesReader.read();

            for (int i = 0; i < timeSeriesArrays.size(); i++)
            {
                var timeSeriesArray = timeSeriesArrays.get(i);
                var name = timeSeriesArray.getHeader().getLocationId() + "_" + timeSeriesArray.getHeader().getParameterId();
                var timeSeries = new TimeSeries { Name = name };
                timeSeries.Components.Add(new Variable<double>(name)
                    {
                        Unit = new Unit(timeSeriesArray.getHeader().getUnit(), timeSeriesArray.getHeader().getUnit()),
                        NoDataValue = Double.NaN
                    });

                timeSeries.Attributes.Add("Location", timeSeriesArray.getHeader().getLocationId() ?? "");
                timeSeries.Attributes.Add("Parameter", timeSeriesArray.getHeader().getParameterId() ?? "");
                timeSeries.Attributes.Add("Station", timeSeriesArray.getHeader().getLocationName() ?? "");                
                var geometry = timeSeriesArray.getHeader().getGeometry();
                if (geometry != null)
                {
                    timeSeries.Attributes.Add("X", geometry.getX(0).ToString());
                    timeSeries.Attributes.Add("Y", geometry.getY(0).ToString());
                }

                for (int j = 0; j < timeSeriesArray.size(); j++)
                {
                    double seriesValue = timeSeriesArray.getValue(j);
                    timeSeries[Java2DotNetHelper.DotNetDateTimeFromJavaMillies(timeSeriesArray.getTime(j))] = seriesValue;
                }
                yield return timeSeries;
            }
        }

        public string FilePath { get; set; }

        # region IFileImporter members

        public string Name { get; private set; }
        public string Description { get { return Name; } }

        public string Category
        {
            get { return "Data"; }
        }

        public Bitmap Image
        {
            get { return Resources.TimeSeries; }
        }

        public virtual IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(IList<TimeSeries>); }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public virtual bool CanImportOnRootLevel
        {
            get { return true; }
        }

        public string FileFilter
        {
            get { return "FEWS-PI XML Files (*.xml)|*.xml"; }
        }

        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public virtual object ImportItem(string path, object target = null)
        {
            if (SelectedTimeSeries != null && SelectedTimeSeries.Count() != 0)
            {
                return SelectedTimeSeries;
            }
            return GetTimeSeriesFromFile(path);
        }

        # endregion
    }
}
