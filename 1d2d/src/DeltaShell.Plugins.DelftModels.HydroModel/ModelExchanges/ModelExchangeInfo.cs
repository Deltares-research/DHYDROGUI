using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ModelExchanges
{
    [DataContract]
    public class ModelExchangeInfo : IHydroModelExchangeInfo
    {
        public ModelExchangeInfo(IModel from, IModel to)
        {
            SourceModelName = GetModelIdentifier(from);
            TargetModelName = GetModelIdentifier(to);

            Exchanges = new List<ModelExchange>();
        }

        public static string GetModelIdentifier(IModel model)
        {
            return model.Name;
        }

        [DataMember]
        public string SourceModelName { get; set; }

        [DataMember]
        public string TargetModelName { get; set; }
        
        [DataMember]
        public IList<ModelExchange> Exchanges { get; set; }

        public bool HasExchanges
        {
            get { return Exchanges.Any(); }
        }
    }
}