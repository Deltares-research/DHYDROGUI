using System.IO;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class MduFileHelper
    {
        public static string GetSubfilePath(string mduFilePath, WaterFlowFMProperty property)
        {
            if (IsFileValued(property))
            {
                var fileName = property.GetValueAsString();
                if (!string.IsNullOrEmpty(fileName))
                {
                    return FileUtils.PathIsRelative(fileName)
                        ? Path.Combine(Path.GetDirectoryName(mduFilePath), fileName)
                        : fileName;
                }
            }
            return null;
        }

        public static bool IsFileValued(WaterFlowFMProperty property)
        {
            if (property.PropertyDefinition.IsDefinedInSchema)
                return property.PropertyDefinition.IsFile;
            return property.Value is string && property.PropertyDefinition.MduPropertyName.ToLower().EndsWith("file");
        }
    }
}
