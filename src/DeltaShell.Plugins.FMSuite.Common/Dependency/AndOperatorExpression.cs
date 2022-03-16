using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.Dependency
{
    public class AndOperatorExpression : DependencyExpressionBase
    {
        private readonly ICollection<DependencyExpressionBase> dependencies;

        public AndOperatorExpression(ICollection<DependencyExpressionBase> dependencies)
        {
            this.dependencies = dependencies;
        }

        public override bool CanHandleExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return false;
            }

            MatchCollection canHandleExpression = RegularExpression.GetMatches(Regex, expression);
            if (canHandleExpression.Count == 0)
            {
                return false;
            }

            string leftPart = canHandleExpression[0].Groups["left"].Value.Trim();
            string rightPart = canHandleExpression[0].Groups["right"].Value.Trim();

            IEnumerable<DependencyExpressionBase>
                leftMatches = dependencies.Where(d => d.CanHandleExpression(leftPart));
            int count = leftMatches.Count();
            if (count == 0)
            {
                return false;
            }

            if (count > 1)
            {
                throw new NotImplementedException("This should not happen.");
            }

            IEnumerable<DependencyExpressionBase> rightMatches =
                dependencies.Where(d => d.CanHandleExpression(rightPart));
            count = rightMatches.Count();
            if (count == 0)
            {
                return false;
            }

            if (count > 1)
            {
                throw new NotImplementedException("This should not happen.");
            }

            return true; // Both subexpressions are valid
        }

        protected internal override string OnValidate(ModelProperty evaluatedProperty,
                                                      IEnumerable<ModelProperty> allProperties,
                                                      string dependencyExpression)
        {
            MatchCollection canHandleExpression = RegularExpression.GetMatches(Regex, dependencyExpression);

            string leftPart = canHandleExpression[0].Groups["left"].Value.Trim();
            DependencyExpressionBase leftMatch = dependencies.First(d => d.CanHandleExpression(leftPart));

            string error = leftMatch.OnValidate(evaluatedProperty, allProperties, leftPart);
            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            string rightPart = canHandleExpression[0].Groups["right"].Value.Trim();
            DependencyExpressionBase rightMatch = dependencies.First(d => d.CanHandleExpression(rightPart));
            return rightMatch.OnValidate(evaluatedProperty, allProperties, rightPart);
        }

        protected internal override Func<IEnumerable<ModelProperty>, bool> OnCompile(
            ModelProperty evaluatedProperty, string dependencyExpression)
        {
            return properties =>
            {
                MatchCollection canHandleExpression = RegularExpression.GetMatches(Regex, dependencyExpression);

                string leftPart = canHandleExpression[0].Groups["left"].Value.Trim();
                DependencyExpressionBase leftMatch = dependencies.First(d => d.CanHandleExpression(leftPart));
                Func<IEnumerable<ModelProperty>, bool> leftIsEnabled =
                    leftMatch.OnCompile(evaluatedProperty, leftPart);

                string rightPart = canHandleExpression[0].Groups["right"].Value.Trim();
                DependencyExpressionBase rightMatch = dependencies.First(d => d.CanHandleExpression(rightPart));
                Func<IEnumerable<ModelProperty>, bool> rightIsEnabled =
                    rightMatch.OnCompile(evaluatedProperty, rightPart);

                return leftIsEnabled(properties) && rightIsEnabled(properties);
            };
        }

        protected override string Regex => @"(?<left>^.*)&&(?<right>.*$)";
    }
}