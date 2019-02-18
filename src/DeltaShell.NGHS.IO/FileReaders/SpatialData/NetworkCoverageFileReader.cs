using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.FileReaders.SpatialData
{
    public class NetworkCoverageFileReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;
        private readonly Func<string, IList<DelftIniCategory>> parseFunc;
        private readonly Func<IList<DelftIniCategory>, IList<IChannel>, IList<string>, INetworkCoverage> convertFunc;

        public NetworkCoverageFileReader(Action<string, IList<string>> createAndAddErrorReport) : this(DelftIniFileParser.ReadFile,
            SpatialDataConverter.Convert,
            createAndAddErrorReport)
        { }

        /// <summary>
        /// Construct a new NetworkCoverageFileReader with the specified functions.
        /// </summary>
        /// <param name="parseFunc"></param>
        /// <param name="convertFunc"></param>
        /// <param name="createAndAddErrorReport"></param>
        /// /// <exception cref="ArgumentException"> parseFunc == null || ConvertFunc == null</exception>
        public NetworkCoverageFileReader(Func<string, IList<DelftIniCategory>> parseFunc,
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
        /// Reads the spatial data ini file and returns the info as a <see cref="INetworkCoverage"/>.
        /// The invoke functions are used as substitutes for DelftIniFileParser.ReadFile & SpatialDataConverter. Convert to make testing faster.
        /// </summary>
        /// <param name="filePath">The file path to the spatial data file.</param>
        /// <param name="channels">The channels from the model that are used to set up the <see cref="INetworkCoverage"/></param>
        /// <returns>A function of <see cref="INetworkLocation"/> to <see cref="double"/></returns>
        public INetworkCoverage ReadSpatialFileData(string filePath, IList<IChannel> channels)
        {
            var errorMessages = new List<string>();
            INetworkCoverage spatialData = null;
            try
            {
                var categories = parseFunc.Invoke(filePath);

                ValidateFileContent(categories, filePath, errorMessages);
                spatialData = convertFunc.Invoke(categories, channels, errorMessages);
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }

            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke("While reading the spatial data from the file, an error occured", errorMessages);

            return spatialData;
        }

        private static void ValidateFileContent(IEnumerable<DelftIniCategory> categories, string filePath, ICollection<string> errorMessages)
        {
            if (categories.Any(c => c.Name == SpatialDataRegion.ContentIniHeader)) return;

            var warningMessage = string.Format(Resources.NetworkCoverageFileReader_ReadSpatialFileData_Spatial_data_file_at_location___0___does_not_contain_a___1___tab_,
                filePath, SpatialDataRegion.ContentIniHeader);
            errorMessages.Add(warningMessage);
        }
    }
}
