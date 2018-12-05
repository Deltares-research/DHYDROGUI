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
        private readonly Func<string, IList<DelftIniCategory>> parseFunc;
        private readonly Func<IList<DelftIniCategory>, IList<IChannel>, IList<string>, INetworkCoverage> convertFunc;

        public SpatialFileDataReader(Action<string, IList<string>> createAndAddErrorReport) : this(DelftIniFileParser.ReadFile,
            SpatialFileDataConverter.Convert,
            createAndAddErrorReport)
        { }

        public SpatialFileDataReader(Func<string, IList<DelftIniCategory>> parseFunc,
            Func<IList<DelftIniCategory>, IList<IChannel>, IList<string>, INetworkCoverage> convertFunc,
            Action<string, IList<string>> createAndAddErrorReport)
        {
            if (parseFunc != null) this.parseFunc = parseFunc;
            else throw new ArgumentException("Parser cannot be null.");

            if (convertFunc != null) this.convertFunc = convertFunc;
            else throw new ArgumentException("Converter cannot be null.");

            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        public INetworkCoverage ReadSpatialFileData(string filePath, IList<IChannel> channelsList)
        {
            var errorMessages = new List<string>();
            IList<DelftIniCategory> categories = new List<DelftIniCategory>();
            try
            {
                categories = parseFunc.Invoke(filePath);
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }

            var spatialData = convertFunc.Invoke(categories, channelsList, errorMessages);

            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke("While reading the spatial data from the file, an error occured", errorMessages);

            return spatialData;
        }
    }
}
