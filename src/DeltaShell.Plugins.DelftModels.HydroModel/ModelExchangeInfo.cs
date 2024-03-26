using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    [DataContract]
    public class ModelExchangeInfo
    {
        public ModelExchangeInfo(IModel from, IModel to)
        {
            SourceModelName = GetModelIdentifier(from);
            TargetModelName = GetModelIdentifier(to);

            Exchanges = new List<ModelExchange>();
        }

        [DataMember]
        public string SourceModelName { get; set; }

        [DataMember]
        public string TargetModelName { get; set; }

        [DataMember]
        public IList<ModelExchange> Exchanges { get; set; }

        public static string GetModelIdentifier(IModel model)
        {
            return model.Name;
        }
    }

    [DataContract]
    public class ModelExchange
    {
        public ModelExchange(IDataItem source, IDataItem target)
        {
            SourceName = GetExchangeIdentifier(source);
            TargetName = GetExchangeIdentifier(target);
        }

        [DataMember]
        public string SourceName { get; set; }

        [DataMember]
        public string TargetName { get; set; }

        /// <summary>
        /// Create an exchange identifier consisting of the name of the dataItem
        /// and the parameter name if it has a <see cref="ParameterValueConverter"/>.
        /// </summary>
        /// <example>ObservationPoint1.CrestLevel</example>
        /// <returns>The identifier that represents the data item and its parameter name.</returns>
        public static string GetExchangeIdentifier(IDataItem dataItem)
        {
            string result = dataItem.Name;

            string parameterName = dataItem.GetParameterName();

            if (!string.IsNullOrEmpty(parameterName))
            {
                result += "." + parameterName;
            }

            if (string.IsNullOrEmpty(result) || result == "0") //sobek legacy?
            {
                IDataItem parent = dataItem.Parent;
                DataItemRole role = dataItem.Role;

                if (parent == null)
                {
                    return result;
                }

                string id = null;
                if (role.HasFlag(DataItemRole.Input))
                {
                    int index =
                        parent.Children.Where(c => c.Role.HasFlag(DataItemRole.Input))
                              .ToList()
                              .IndexOf(dataItem);
                    id = "input" + index;
                }

                if (role.HasFlag(DataItemRole.Output))
                {
                    int index =
                        parent.Children.Where(c => c.Role.HasFlag(DataItemRole.Output))
                              .ToList()
                              .IndexOf(dataItem);
                    id = "output" + index;
                }

                if (id != null)
                {
                    string parentName = parent.Name;
                    result = string.IsNullOrEmpty(parentName) ? id : string.Join(".", parentName, id);
                }
            }

            return result;
        }
    }
}