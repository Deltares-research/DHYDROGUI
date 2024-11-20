using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ModelExchanges
{
    public static class HydroModelExchangeInfoJsonFileExtensions
    {
        public static void WriteToJson<T>(this IEnumerable<T> exchanges, string filePath) where T : IHydroModelExchangeInfo
        {
            if (!exchanges.Any(e => e.HasExchanges)) return;

            File.WriteAllText(filePath, JsonConvert.SerializeObject(exchanges, Formatting.Indented));
        }

        public static void ReadFromJson<T>(this ICollection<T> exchanges, string filePath, bool replace = true) where T : IHydroModelExchangeInfo
        {
            if (replace)
            {
                exchanges.Clear();
            }

            if (!File.Exists(filePath)) return;

            var jsonReader = new DataContractJsonSerializer(typeof(IList<T>));
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                var infos = (IList<T>) jsonReader.ReadObject(stream);
                foreach (var info in infos)
                {
                    exchanges.Add(info);
                }
            }
        }
    }
}