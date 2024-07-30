using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.Dependency
{
    /// <summary>
    /// This class compiles <see cref="ModelPropertyDefinition.EnabledDependencies"/> and is
    /// responsible for updating all dependency related methods.
    /// </summary>
    public static class Dependencies
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (Dependencies));

        private static readonly ICollection<DependencyExpressionBase> SupportedDependencyExpressions;

        static Dependencies()
        {
            SupportedDependencyExpressions = new List<DependencyExpressionBase>
                {
                    new BooleanIsTrueDependencyExpression(),
                    new ValueGreaterOrLesserThanDependencyExpression(),
                    new ValueEqualsDependencyExpression()
                };
            SupportedDependencyExpressions.Add(new AndOperatorExpression(SupportedDependencyExpressions));
        }

        public static void CompileEnabledDependencies(IEnumerable<ModelProperty> allProperties)
        {
            foreach (var modelProperty in allProperties)
            {
                // Nothing specified, use default:
                if (string.IsNullOrEmpty(modelProperty.PropertyDefinition.EnabledDependencies))
                {
                    modelProperty.PropertyDefinition.IsEnabled = null;
                    continue;
                }

                var property = modelProperty; // Deal with 'foreach variable in closure'
                var matchingDependencies =
                    SupportedDependencyExpressions.Where(
                        sde => sde.CanHandleExpression(property.PropertyDefinition.EnabledDependencies)).ToArray();
                if (matchingDependencies.Length > 0)
                {
                    // Get all compiled expressions:
                    var compiledExpressions = new List<Func<IEnumerable<ModelProperty>, bool>>();
                    foreach (var dependencyExpression in matchingDependencies)
                    {
                        try
                        {
                            compiledExpressions.Add(dependencyExpression.CompileExpression(property, allProperties, property.PropertyDefinition.EnabledDependencies));
                        }
                        catch (ArgumentException e)
                        {
                            modelProperty.PropertyDefinition.IsEnabled = null;
                            Log.ErrorFormat("Cannot read dependencies for property '{0}'; Reason: {1}",
                                            modelProperty.PropertyDefinition.FilePropertyKey, e.Message);
                        }
                    }
                    // AND composition of all compiled expressions:
                    modelProperty.PropertyDefinition.IsEnabled =
                        properties => compiledExpressions.All(isEnabled => isEnabled(properties));
                }
                else
                {
                    modelProperty.PropertyDefinition.IsEnabled = null;
                }
            }
        }

        public static void CompileVisibleDependencies(IEnumerable<ModelProperty> allProperties)
        {
            foreach (var modelProperty in allProperties)
            {
                // Nothing specified, use default:
                if (string.IsNullOrEmpty(modelProperty.PropertyDefinition.VisibleDependencies))
                {
                    modelProperty.PropertyDefinition.IsVisible = null;
                    continue;
                }

                var property = modelProperty; // Deal with 'foreach variable in closure'
                var matchingDependencies =
                    SupportedDependencyExpressions.Where(
                        sde => sde.CanHandleExpression(property.PropertyDefinition.VisibleDependencies)).ToArray();
                if (matchingDependencies.Length > 0)
                {
                    // Get all compiled expressions:
                    var compiledExpressions = new List<Func<IEnumerable<ModelProperty>, bool>>();
                    foreach (var dependencyExpression in matchingDependencies)
                    {
                        try
                        {
                            compiledExpressions.Add(dependencyExpression.CompileExpression(property, allProperties, property.PropertyDefinition.VisibleDependencies));
                        }
                        catch (ArgumentException e)
                        {
                            modelProperty.PropertyDefinition.IsVisible = null;
                            Log.ErrorFormat("Cannot read dependencies for property '{0}'; Reason: {1}",
                                            modelProperty.PropertyDefinition.FilePropertyKey, e.Message);
                        }
                    }
                    // AND composition of all compiled expressions:
                    modelProperty.PropertyDefinition.IsVisible =
                        properties => compiledExpressions.All(isVisible => isVisible(properties));
                }
                else
                {
                    modelProperty.PropertyDefinition.IsVisible = null;
                }
            }
        }

        public static void CompileDefaultValueIndexerDependencies(IEnumerable<ModelProperty> allProperties)
        {
            IEnumerable<ModelProperty> modelProperties = allProperties as ModelProperty[] ?? allProperties.ToArray();
            foreach (var modelProperty in modelProperties)
            {
                // Nothing specified, use default:
                if (string.IsNullOrEmpty(modelProperty.PropertyDefinition.DefaultsIndexer))
                {
                    continue;
                }

                var property = modelProperty;
                var matchingDependency = modelProperties.SingleOrDefault(p => p.PropertyDefinition.FilePropertyKey.Equals(property.PropertyDefinition.DefaultsIndexer));
                    
                if (matchingDependency != null)
                {
                    matchingDependency.LinkedModelProperty = property;
                    property.SetValueFromString(property.PropertyDefinition.DefaultValueAsStringArray[(int)matchingDependency.Value]);
                }
            }
        }
    }
}