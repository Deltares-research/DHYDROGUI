using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Common;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    [Entity]
    public class WaveMeteoData
    {
        public WaveMeteoData()
        {
            FileType = WindDefinitionType.WindXY;
            XYVectorFileName = "";
        }

        public WindDefinitionType FileType { get; set; }

        public string XYVectorFileName { get; set; }

        public string XComponentFileName { get; set; }
        public string YComponentFileName { get; set; }

        public bool HasSpiderWeb { get; set; }
        public string SpiderWebFileName { get; set; }
    }
}