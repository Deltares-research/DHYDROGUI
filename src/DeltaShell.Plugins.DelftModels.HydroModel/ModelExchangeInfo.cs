using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    /// <summary>
    /// Class that represents a collection of model exchanges between two <see cref="ICoupledModel"/>.
    /// </summary>
    [DataContract]
    public class ModelExchangeInfo
    {
        private readonly ICoupledModel sourceModel;
        private readonly ICoupledModel targetModel;

        /// <summary>
        /// Creates a new instance of <see cref="ModelExchangeInfo"/>.
        /// </summary>
        /// <param name="sourceModel">The source model.</param>
        /// <param name="targetModel">The target model.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public ModelExchangeInfo(ICoupledModel sourceModel, ICoupledModel targetModel)
        {
            Ensure.NotNull(sourceModel, nameof(sourceModel));
            Ensure.NotNull(targetModel, nameof(targetModel));

            this.sourceModel = sourceModel;
            this.targetModel = targetModel;

            SourceModelName = sourceModel.Name;
            TargetModelName = targetModel.Name;

            Exchanges = new List<ModelExchange>();
        }

        /// <summary>
        /// Gets the source model name.
        /// </summary>
        /// <remarks>The setter is required and should only be used for the deserialization.</remarks>
        [DataMember]
        public string SourceModelName { get; private set; }

        /// <summary>
        /// Gets the target model name.
        /// </summary>
        /// <remarks>The setter is required and should only be used for the deserialization.</remarks>
        [DataMember]
        public string TargetModelName { get; private set; }

        /// <summary>
        /// Gets the collection of <see cref="ModelExchange"/>.
        /// </summary>
        /// <remarks>The setter is required and should only be used for the deserialization.</remarks>
        [DataMember]
        public IList<ModelExchange> Exchanges { get; private set; }

        /// <summary>
        /// Adds a new model exchange between the given <see cref="IDataItem"/> to the collection of model exchanges.
        /// </summary>
        /// <param name="sourceDataItem">The source data item.</param>
        /// <param name="targetDataItem">The target data item.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public void AddExchange(IDataItem sourceDataItem, IDataItem targetDataItem)
        {
            Ensure.NotNull(sourceDataItem, nameof(sourceDataItem));
            Ensure.NotNull(targetDataItem, nameof(targetDataItem));

            string sourceName = GetExchangeIdentifier(sourceDataItem, sourceModel);
            string targetName = GetExchangeIdentifier(targetDataItem, targetModel);

            var exchange = new ModelExchange(sourceName, targetName);

            Exchanges.Add(exchange);
        }

        /// <summary>
        /// Create an exchange identifier consisting of the type and name of the dataItem
        /// and the parameter name if it has a <see cref="ParameterValueConverter"/>.
        /// </summary>
        /// <example>observations/ObservationPoint1/water_level</example>
        /// <returns>The identifier that represents the data item and its parameter name.</returns>
        private static string GetExchangeIdentifier(IDataItem dataItem, ICoupledModel model)
        {
            if (IsSobek3Legacy(dataItem.Name))
            {
                return GetSobek3LegacyIdentifier(dataItem);
            }

            return model.GetExchangeIdentifier(dataItem);
        }

        private static bool IsSobek3Legacy(string dataItemName)
        {
            return string.IsNullOrEmpty(dataItemName) || dataItemName == "0";
        }

        private static string GetSobek3LegacyIdentifier(IDataItem dataItem)
        {
            string result = dataItem.Name;

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

            return result;
        }
    }

    /// <summary>
    /// DTO representing a model exchange between (the names of) two <see cref="IDataItem"/>.
    /// </summary>
    [DataContract]
    public class ModelExchange
    {
        /// <summary>
        /// Creates a new instance of <see cref="ModelExchange"/>.
        /// </summary>
        /// <param name="sourceName">The name of the source data item for this exchange.</param>
        /// <param name="targetName">The name of the target data item for this exchange.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public ModelExchange(string sourceName, string targetName)
        {
            Ensure.NotNull(sourceName, nameof(sourceName));
            Ensure.NotNull(targetName, nameof(targetName));

            SourceName = sourceName;
            TargetName = targetName;
        }

        /// <summary>
        /// Gets the name of the source for the model exchange.
        /// </summary>
        [DataMember]
        public string SourceName { get; private set; }

        /// <summary>
        /// Gets the name of the target for the model exchange.
        /// </summary>
        [DataMember]
        public string TargetName { get; private set; }
    }
}