using System.Collections.Generic;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi
{
    public class ModelApiParameters
    {
        private static string ParametersXmlPath
        {
            get
            {
                var assemblyDirectory = Path.GetDirectoryName(typeof(ModelApiParameters).Assembly.CodeBase.Substring(8));
                return assemblyDirectory + @"\parameters.xml";
            }
        }

        // reads parameters.xml located next to modelapi.dll
        public static IList<ModelApiParameter> ReadParametersFromXml()
        {
            return ReadParametersFromXml(ParametersXmlPath);
        }

        public static IList<ModelApiParameter> ReadParametersFromXml(string path)
        {
            if (File.Exists(path))
            {
                var parametersReader = new WaterFlowModelParametersReader();
                return parametersReader.ReadParameters(path);
            }
            throw new FileNotFoundException(string.Format("Could not find parameters.xml at {0}", path));
        }
    }
}