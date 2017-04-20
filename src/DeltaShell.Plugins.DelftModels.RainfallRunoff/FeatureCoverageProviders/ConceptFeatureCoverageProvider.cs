using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FeatureCoverageProviders
{
    public abstract class ConceptFeatureCoverageProvider<T> : IFeatureCoverageProvider where T : CatchmentModelData
    {
        protected RainfallRunoffModel Model;
        protected abstract string ConceptName { get; }
        protected abstract IList<Expression<Func<double>>> Attributes { get; set; }

        private IEnumerable<T> ApplicableModelData
        {
            get { return Model.GetAllModelData().OfType<T>(); }
        }
        
        public IEnumerable<string> FeatureCoverageNames
        {
            get
            {
                int count = ApplicableModelData.Count();
                return count > 0 ? GetAttributeDescriptions() : new string[] {};
            }
        }

        public IFeatureCoverage GetFeatureCoverageByName(string name)
        {
            Expression<Func<double>> attribute =
                Attributes.FirstOrDefault(a => GetFeatureCoverageNameForAttribute(a) == name);
            if (attribute != null)
            {
                return BuildFeatureCoverageForAttribute(attribute);
            }
            return null;
        }

        private string GetFeatureCoverageNameForAttribute(Expression<Func<double>> attribute)
        {
            return ConceptName + ": " + TypeUtils.GetMemberDescription(attribute);
        }

        private IEnumerable<string> GetAttributeDescriptions()
        {
            return Attributes.Select(GetFeatureCoverageNameForAttribute);
        }

        private IFeatureCoverage BuildFeatureCoverageForAttribute(Expression<Func<double>> attribute)
        {
            var featureCoverage = new FeatureCoverage();
            featureCoverage.Name = GetFeatureCoverageNameForAttribute(attribute);
            featureCoverage.Arguments.Add(new Variable<IFeature>());
            featureCoverage.Components.Add(new Variable<double>());
            foreach (T area in ApplicableModelData)
            {
                featureCoverage.Features.Add(area.Catchment);
                featureCoverage[area.Catchment] = GetValueForAttribute(area, attribute);
            }
            return featureCoverage;
        }
        
        protected virtual double GetValueForAttribute(T modelData, Expression<Func<double>> attribute)
        {
            return (double)TypeUtils.GetPropertyValue(modelData, TypeUtils.GetMemberName(attribute));
        }
    }
}