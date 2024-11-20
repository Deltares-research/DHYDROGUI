using System.Linq;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class BathymetryFileWriter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BathymetryFileWriter));

        public static void Write(string filePath, WaterFlowFMModelDefinition modelDefinition)
        {
            var bedLevelTypeProperty = modelDefinition.Properties.FirstOrDefault(p =>
                p.PropertyDefinition != null &&
                p.PropertyDefinition.MduPropertyName.ToLower() == KnownProperties.BedlevType);

            if (bedLevelTypeProperty == null)
            {
                Log.WarnFormat("Cannot determine Bed level location, z-values will not be exported");
                return;
            }

            var location = (BedLevelLocation)bedLevelTypeProperty.Value;
            var values = modelDefinition.Bathymetry.Components[0].GetValues<double>().ToArray();
            using (var ugridFile = new UGridFile(filePath))
            {
                ugridFile.WriteZValues(location, values);
            }
        }
    }
}
