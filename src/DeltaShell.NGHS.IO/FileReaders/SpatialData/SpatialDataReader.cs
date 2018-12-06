using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.FileReaders.SpatialData
{
    public class SpatialDataReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;
        private readonly Func<string, IList<DelftIniCategory>> parseFunc;
        private readonly Func<IList<DelftIniCategory>, IList<IChannel>, IList<string>, INetworkCoverage> convertFunc;

        public SpatialDataReader(Action<string, IList<string>> createAndAddErrorReport) : this(DelftIniFileParser.ReadFile,
            SpatialDataConverter.Convert,
            createAndAddErrorReport)
        { }
        /// <summary>
        /// Construct a new SpatialDataReader with the specified functions.
        /// </summary>
        /// <param name="parseFunc"></param>
        /// <param name="convertFunc"></param>
        /// <param name="createAndAddErrorReport"></param>
        /// /// <exception cref="ArgumentException"> parseFunc == null || ConvertFunc == null</exception>
        public SpatialDataReader(Func<string, IList<DelftIniCategory>> parseFunc,
            Func<IList<DelftIniCategory>, IList<IChannel>, IList<string>, INetworkCoverage> convertFunc,
            Action<string, IList<string>> createAndAddErrorReport)
        {
            if (parseFunc != null) this.parseFunc = parseFunc;
            else throw new ArgumentException("Parser cannot be null.");

            if (convertFunc != null) this.convertFunc = convertFunc;
            else throw new ArgumentException("Converter cannot be null.");

            this.createAndAddErrorReport = createAndAddErrorReport;
        }

        /// <summary>
        /// Reads the spatial data ini file and returns the info as a Networkcoverage.
        /// The invoke functions are used as substitutes for DelftIniFileParser.ReadFile & SpatialDataConverter.Convert to make testing faster.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="channelsList"></param>
        /// <returns></returns>
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
