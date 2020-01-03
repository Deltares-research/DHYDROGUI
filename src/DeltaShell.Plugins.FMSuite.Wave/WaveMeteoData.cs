using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using System.IO;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    /// <summary>
    /// Wave meteo data
    /// </summary>
    [Entity]
    public class WaveMeteoData
    {
        public WaveMeteoData()
        {
            FileType = WindDefinitionType.WindXY;
        }

        /// <summary>
        /// Gets or sets the type of the files that are used.
        /// </summary>
        /// <value>
        /// The type of the file.
        /// </value>
        public WindDefinitionType FileType { get; set; }

        /// <summary>
        /// Gets or sets the XY vector file path.
        /// </summary>
        /// <value>
        /// The XY vector file path.
        /// </value>
        public string XYVectorFilePath { get; set; }

        /// <summary>
        /// Gets the name of the XY vector file.
        /// </summary>
        /// <value>
        /// The name of the XY vector file.
        /// </value>
        public string XYVectorFileName => Path.GetFileName(XYVectorFilePath);

        /// <summary>
        /// Gets or sets the X component file path.
        /// </summary>
        /// <value>
        /// The X component file path.
        /// </value>
        public string XComponentFilePath { get; set; }

        /// <summary>
        /// Gets the name of the X component file.
        /// </summary>
        /// <value>
        /// The name of the X component file.
        /// </value>
        public string XComponentFileName => Path.GetFileName(XComponentFilePath);

        /// <summary>
        /// Gets or sets the Y component file path.
        /// </summary>
        /// <value>
        /// The T component file path.
        /// </value>
        public string YComponentFilePath { get; set; }

        /// <summary>
        /// Gets the name of the Y component file.
        /// </summary>
        /// <value>
        /// The name of the Y component file.
        /// </value>
        public string YComponentFileName => Path.GetFileName(YComponentFilePath);

        /// <summary>
        /// Gets or sets a value indicating whether a spider web is used.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance uses a spider web; otherwise, <c>false</c>.
        /// </value>
        public bool HasSpiderWeb { get; set; }

        /// <summary>
        /// Gets or sets the spider web file path.
        /// </summary>
        /// <value>
        /// The spider web file path.
        /// </value>
        public string SpiderWebFilePath { get; set; }

        /// <summary>
        /// Gets the name of the spider web file.
        /// </summary>
        /// <value>
        /// The name of the spider web file.
        /// </value>
        public string SpiderWebFileName => Path.GetFileName(SpiderWebFilePath);
    }
}