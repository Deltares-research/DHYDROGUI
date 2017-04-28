using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi
{
    public class WaterFlowModelParametersReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModelParametersReader));

        public IList<ModelApiParameter> ReadParameters(string filePath)
        {
            IList<ModelApiParameter> apiParameters = new List<ModelApiParameter>();
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("The file {0} doesn't exist", filePath));
            }
            var allLines = File.ReadAllText(filePath);

            var document = new XmlDocument();

            try
            {
                document.LoadXml(allLines);
                foreach (XmlNode childNode in document.DocumentElement.ChildNodes)
                {
                    if (childNode.NodeType != XmlNodeType.Element) continue;
                    if (childNode.Name != "ModelApiParameter") continue;
                    ModelApiParameter parameter = new ModelApiParameter();
                    foreach (XmlAttribute atr in childNode.Attributes)
                    {
                        if (atr.Name == "Id")
                        {
                            parameter.Name = atr.Value;
                            
                        }
                        if (atr.Name =="Category")
                        {
                            parameter.Category = (ParameterCategory)Enum.Parse(typeof(ParameterCategory), atr.Value);
                        }
                        if (atr.Name == "Description")
                        {
                            parameter.Description = atr.Value;
                        }
                        if (atr.Name == "Type")
                        {
                            parameter.Type = atr.Value;
                        }
                        if (atr.Name == "Value")
                        {
                            parameter.Value = atr.Value;
                        }
                        if (atr.Name == "Visible")
                        {
                            parameter.Visible = Convert.ToBoolean(atr.Value);  //(bool)Enum.Parse(typeof(bool), atr.Value);
                        }
                        
                        // Console.WriteLine(string.Format("Node : {1},  Atribute : '{0}', Value : {2}", atr.Name, childNode.Name, atr.Value));
                    }
                    apiParameters.Add(parameter);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
            return apiParameters;
        }
    }
}
