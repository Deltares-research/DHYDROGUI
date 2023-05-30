using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers
{
    /// <summary>
    /// Converter that converts the data access object <see cref="BcBlockData"/> to the domain object <see cref="RainfallRunoffBoundaryData"/>.
    /// <see cref="RainfallRunoffBoundaryData"/>.
    /// </summary>
    public class RRBoundaryConditionsConverter
    {
        private readonly RRBoundaryConditionsDataParserProvider parserProvider;

        /// <summary>
        /// Constructor for converter for rainfall runoff boundary conditions from <see cref="BcBlockData"/> to
        /// <see cref="RainfallRunoffBoundaryData"/>.
        /// </summary>
        /// <param name="parserProvider">Parser provider, which provides the parser for the <see cref="BcBlockData"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// When <paramref name="parserProvider"/> is <c>null</c>.
        /// </exception>
        public RRBoundaryConditionsConverter(RRBoundaryConditionsDataParserProvider parserProvider)
        {
            Ensure.NotNull(parserProvider, nameof(parserProvider));
            this.parserProvider = parserProvider;
        }

        /// <summary>
        /// Converter to convert <see cref="BcBlockData"/> to <see cref="RainfallRunoffBoundaryData"/>.
        /// </summary>
        /// <param name="bcBlockDatas">Data to convert.</param>
        /// <returns>
        /// A dictionary with the boundary name as the key and the related <see cref="RainfallRunoffBoundaryData"/> as
        /// value.
        /// </returns>
        /// <exception cref="ArgumentNullException">When <paramref name="bcBlockDatas"/> is <c>null</c>.</exception>
        /// <remarks>
        /// When the <paramref name="bcBlockDatas"/> contains multiple boundaries with the same name, the data will be
        /// overwritten with the last occurrence.
        /// </remarks>
        public IReadOnlyDictionary<string, RainfallRunoffBoundaryData> Convert(IEnumerable<BcBlockData> bcBlockDatas)
        {
            Ensure.NotNull(bcBlockDatas, nameof(bcBlockDatas));
            return bcBlockDatas.ToDictionary(b => b.SupportPoint, ConvertToRunOffBoundary);
        }

        private RainfallRunoffBoundaryData ConvertToRunOffBoundary(BcBlockData bcBlockData)
        {
            IRRBoundaryConditionsDataParser parser = parserProvider.GetParser(bcBlockData);
            return parser.Parse(bcBlockData);
        }
    }
}