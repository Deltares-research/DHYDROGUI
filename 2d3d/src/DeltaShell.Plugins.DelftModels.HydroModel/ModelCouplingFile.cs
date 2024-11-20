using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public class ModelCouplingFile
    {
        public string FilePath { get; set; }

        public void Write(string filePath, IEnumerable<ModelExchangeInfo> exchange)
        {
            if (!exchange.Any(e => e.Exchanges.Any()))
            {
                return;
            }

            string newtonResult = JsonConvert.SerializeObject(exchange, Formatting.Indented);
            Console.WriteLine(newtonResult);
            File.WriteAllText(filePath, newtonResult);
        }

        public IEnumerable<ModelExchangeInfo> Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                yield break;
            }

            var jsonReader = new DataContractJsonSerializer(typeof(IList<ModelExchangeInfo>));
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                var infos = (IList<ModelExchangeInfo>) jsonReader.ReadObject(stream);
                foreach (ModelExchangeInfo info in infos)
                {
                    yield return info;
                }
            }
        }
    }
}