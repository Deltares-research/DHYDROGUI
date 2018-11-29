using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.FileReaders.SpatialData
{
    public class SpatialFileDataReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        public SpatialFileDataReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        public INetworkCoverage ReadSpatialFileData(string filePath, IList<IChannel> channelsList)
        {
            var errorMessages = new List<string>();
            IList<DelftIniCategory> categories = new List<DelftIniCategory>();
            try
            {
                categories = DelftIniFileParser.ReadFile(filePath);
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }

            var spatialData = SpatialFileDataConverter.Convert(categories, channelsList, errorMessages);

            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke("While reading the spatial data from the file, an error occured", errorMessages);

            return spatialData;
        }
    }
}
