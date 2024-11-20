using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Validation;

namespace DelftTools.Hydro
{
    public interface IModelMerge : IModel
    {
        /// <summary>
        /// containers of models that need to be merged before you can merge this model
        /// </summary>
        IEnumerable<IModelMerge> DependentModels { get; }

        /// <summary>
        /// Validates if you can validate a merge into this model with another model
        /// </summary>
        /// <param name="sourceModel"> </param>
        /// <returns> A validation report </returns>
        ValidationReport ValidateMerge(IModelMerge sourceModel);

        /// <summary>
        /// Merges a model into this model
        /// </summary>
        /// <param name="sourceModel"> The model we want to merge into this model </param>
        /// <param name="mergedDependentModelsLookup"> A list of merged models listed in <see cref="DependentModels"/> </param>
        /// <returns> If the merge was successful </returns>
        bool Merge(IModelMerge sourceModel, IDictionary<IModelMerge, IModelMerge> mergedDependentModelsLookup);

        /// <summary>
        /// Checks if this model can be merged with the source model
        /// <param name="sourceModel"> </param>
        /// </summary>
        bool CanMerge(object sourceModel);
    }
}