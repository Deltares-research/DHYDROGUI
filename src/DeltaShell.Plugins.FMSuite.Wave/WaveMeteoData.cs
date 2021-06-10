using System.IO;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Common.Wind;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    /// <summary>
    /// The <see cref="WaveMeteoData"/> defines the wind input data of a <see cref="WaveModel"/>.
    /// </summary>
    [Entity]
    public class WaveMeteoData
    {
        /// <summary>
        /// Gets or sets the type of the files that are used.
        /// </summary>
        public WindDefinitionType FileType { get; set; } = WindDefinitionType.WindXY;

        /// <summary>
        /// Gets or sets the XY vector file path.
        /// </summary>
        public string XYVectorFilePath { get; set; }

        /// <summary>
        /// Gets the name of the XY vector file.
        /// </summary>
        public string XYVectorFileName => Path.GetFileName(XYVectorFilePath);

        /// <summary>
        /// Gets or sets the X component file path.
        /// </summary>
        public string XComponentFilePath { get; set; }

        /// <summary>
        /// Gets the name of the X component file.
        /// </summary>
        public string XComponentFileName => Path.GetFileName(XComponentFilePath);

        /// <summary>
        /// Gets or sets the Y component file path.
        /// </summary>
        public string YComponentFilePath { get; set; }

        /// <summary>
        /// Gets the name of the Y component file.
        /// </summary>
        public string YComponentFileName => Path.GetFileName(YComponentFilePath);

        /// <summary>
        /// Gets or sets a value indicating whether a spider web is used.
        /// </summary>
        public bool HasSpiderWeb { get; set; }

        /// <summary>
        /// Gets or sets the spider web file path.
        /// </summary>
        public string SpiderWebFilePath { get; set; }

        /// <summary>
        /// Gets the name of the spider web file.
        /// </summary>
        public string SpiderWebFileName => Path.GetFileName(SpiderWebFilePath);
    }
}