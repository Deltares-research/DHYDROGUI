using System.Linq;
using System.Runtime.Serialization;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ModelExchanges
{
    [DataContract]
    public class ModelExchange
    {
        public ModelExchange(IDataItem source, IDataItem target)
        {
            SourceName = GetExchangeIdentifier(source);
            TargetName = GetExchangeIdentifier(target);
        }

        /// <summary>
        /// Create an exchange identifier consisting of the name of the dataItem
        /// and the parameter name if it has a <see cref="ParameterValueConverter"/>.
        /// </summary>
        /// <example>ObservationPoint1.CrestLevel</example>
        /// <returns>The identifier that represents the data item and its parameter name.</returns>
        public static string GetExchangeIdentifier(IDataItem dataItem)
        {
            var result = dataItem.Name;

            var parameterName = dataItem.GetParameterName();
            
            if (!string.IsNullOrEmpty(parameterName))
            {
                result += "." + parameterName;
            }

            if (string.IsNullOrEmpty(result) || result == "0") //sobek legacy?
            {
                var parent = dataItem.Parent;
                var role = dataItem.Role;

                if (parent == null) return result;
                
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
                    var parentName = parent.Name;
                    result = string.IsNullOrEmpty(parentName) ? id : string.Join(".", parentName, id);
                }
            }

            return result;
        }

        [DataMember]
        public string SourceName { get; set; }

        [DataMember]
        public string TargetName { get; set; }
    }
}