using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties;
using GeoAPI.Extensions.Coverages;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.MapLayerProviders
{
    public class RainfallRunoffMapLayerProvider : IMapLayerProvider
    {
        /// <summary>
        /// Create a layer for the provided data.
        /// </summary>
        /// <param name="data"> Data to create a layer for. </param>
        /// <param name="parentData"> Parent data of the data. </param>
        /// <returns> Layer for the data. </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when <paramref name="data"/> is of a type that is not supported;
        /// when <see cref="CanCreateLayerFor"/> returns <c>false</c> for this type of data.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        public ILayer CreateLayer(object data, object parentData)
        {
            Ensure.NotNull(data, nameof(data));

            switch (data)
            {
                case RainfallRunoffModel rainfallRunoffModel:
                    return CreateLayer(rainfallRunoffModel);
                case OutputMapLayerData outputMapLayerData:
                    return CreateLayer(outputMapLayerData);
                case OutputCoverageGroupMapLayerData outputCoverageGroupMapLayerData:
                    return CreateLayer(outputCoverageGroupMapLayerData);
                case IFeatureCoverage featureCoverage:
                    return CreateLayer(featureCoverage);
                default:
                    throw new NotSupportedException($"Layer for type {data.GetType()} is not supported.");
            }
        }

        public bool CanCreateLayerFor(object data, object parentData)
        {
            return data is RainfallRunoffModel ||
                   data is OutputMapLayerData ||
                   data is OutputCoverageGroupMapLayerData ||
                   data is IFeatureCoverage && parentData is OutputCoverageGroupMapLayerData;
        }

        /// <summary>
        /// Child objects for <paramref name="data"/>.
        /// Objects will be used to create child layers for the group layer.
        /// </summary>
        /// <param name="data"> Group layer data. </param>
        /// <returns> Child objects for <paramref name="data"/>. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <c>null</c>.
        /// </exception>
        public IEnumerable<object> ChildLayerObjects(object data)
        {
            Ensure.NotNull(data, nameof(data));

            switch (data)
            {
                case RainfallRunoffModel rainfallRunoffModel:
                    return ChildLayerObjects(rainfallRunoffModel);
                case OutputMapLayerData outputMapLayerData:
                    return ChildLayerObjects(outputMapLayerData);
                case OutputCoverageGroupMapLayerData outputCoverageGroupMapLayerData:
                    return ChildLayerObjects(outputCoverageGroupMapLayerData);
                default:
                    throw new NotSupportedException($"Child layer objects for type {data.GetType()} is not supported.");
            }
        }

        public void AfterCreate(ILayer layer, object layerObject, object parentObject, IDictionary<ILayer, object> objectsLookup)
        {
            // no actions needed
        }

        private static ILayer CreateLayer(RainfallRunoffModel rainfallRunoffModel)
        {
            return new GroupLayer(rainfallRunoffModel.Name)
            {
                LayersReadOnly = true,
                NameIsReadOnly = true
            };
        }

        private static ILayer CreateLayer(OutputMapLayerData outputMapLayerData)
        {
            return new GroupLayer(outputMapLayerData.Name)
            {
                LayersReadOnly = true,
                NameIsReadOnly = true
            };
        }
        
        private static ILayer CreateLayer(OutputCoverageGroupMapLayerData outputCoverageGroupMapLayerData)
        {
            return new GroupLayer(outputCoverageGroupMapLayerData.Name)
            {
                LayersReadOnly = true,
                NameIsReadOnly = true
            };
        }

        private static ILayer CreateLayer(IFeatureCoverage featureCoverage)
        {
            var coverageLayer = (FeatureCoverageLayer)SharpMapLayerFactory.CreateMapLayerForCoverage(featureCoverage, null);
            coverageLayer.NameIsReadOnly = false;
            coverageLayer.Name = GetCoverageLayerName(featureCoverage);
            coverageLayer.NameIsReadOnly = true;
            coverageLayer.Visible = false;
            coverageLayer.AutoUpdateThemeOnDataSourceChanged = true;
            return coverageLayer;
        }

        private static IEnumerable<object> ChildLayerObjects(RainfallRunoffModel rainfallRunoffModel)
        {
            yield return rainfallRunoffModel.Basin;
            yield return new OutputMapLayerData(rainfallRunoffModel);
        }

        private static IEnumerable<object> ChildLayerObjects(OutputMapLayerData outputMapLayerData)
        {
            IEnumerable<ICoverage> coverages = outputMapLayerData.Model.OutputCoverages;
            Dictionary<string, IEnumerable<ICoverage>> coverageCategories = coverages.ToGroupedDictionary(GetCategory);

            yield return new OutputCoverageGroupMapLayerData(Resources.BoundariesLayer, GetCoverages(coverageCategories, "bnd"));
            yield return new OutputCoverageGroupMapLayerData(Resources.UnpavedLayer, GetCoverages(coverageCategories, "unp"));
            yield return new OutputCoverageGroupMapLayerData(Resources.PavedLayer, GetCoverages(coverageCategories, "p"));
            yield return new OutputCoverageGroupMapLayerData(Resources.GreenhouseLayer, GetCoverages(coverageCategories, "g"));
            yield return new OutputCoverageGroupMapLayerData(Resources.OpenWaterLayer, GetCoverages(coverageCategories, "ow"));
            yield return new OutputCoverageGroupMapLayerData(Resources.SacramentoLayer, GetCoverages(coverageCategories, "sac"));
            yield return new OutputCoverageGroupMapLayerData(Resources.HBVLayer, GetCoverages(coverageCategories, "hbv"));
            yield return new OutputCoverageGroupMapLayerData(Resources.WWTPLayer, GetCoverages(coverageCategories, "wwtp"));
            yield return new OutputCoverageGroupMapLayerData(Resources.BalancesLayer, GetCoverages(coverageCategories, "bn", "bm"));
            yield return new OutputCoverageGroupMapLayerData(Resources.LinksLayer, GetCoverages(coverageCategories, "lnk"));
            yield return new OutputCoverageGroupMapLayerData(Resources.NWRWLayer, GetCoverages(coverageCategories, "nwrw"));
        }
        
        private static IEnumerable<object> ChildLayerObjects(OutputCoverageGroupMapLayerData outputCoverageGroupMapLayerData) => 
            outputCoverageGroupMapLayerData.Coverages;

        private static IEnumerable<ICoverage> GetCoverages(IReadOnlyDictionary<string, IEnumerable<ICoverage>> coverageCategories, params string[] filters) =>
            filters.SelectMany(filter => GetFilteredCoverages(coverageCategories, filter));

        private static IEnumerable<ICoverage> GetFilteredCoverages(IReadOnlyDictionary<string, IEnumerable<ICoverage>> coverageCategories, string filter) =>
            coverageCategories.TryGetValue(filter, out IEnumerable<ICoverage> coverages)
                ? coverages
                : Enumerable.Empty<ICoverage>();

        private static string GetCategory(ICoverage coverage) => coverage.Name.LastStringBetween('(', ')');

        private static string GetCoverageLayerName(ICoverage coverage)
        {
            string category = GetCategory(coverage);
            if (category == string.Empty)
            {
                throw new ArgumentException($"Coverage layer cannot be created for coverage with name without a category. Coverage name: {coverage.Name}");
            }
            
            string withoutCategory = RemoveLast(coverage.Name, $"({category})");
            return withoutCategory.Trim();
        }

        private static string RemoveLast(string source, string substring)
        {
            int index = source.LastIndexOf(substring);
            return source.Remove(index, substring.Length);
        }
    }
}