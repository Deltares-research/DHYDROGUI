using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override string Regex
        {
            get { return @"(?<left>^.*)&&(?<right>.*$)"; } // Note: When also implementing ||, we require precedency rules as well!
        }

        public override bool CanHandleExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression)) return false;

            var canHandleExpression = RegularExpression.GetMatches(Regex, expression);
            if (canHandleExpression.Count == 0) return false;

            var leftPart = canHandleExpression[0].Groups["left"].Value.Trim();
            var rightPart = canHandleExpression[0].Groups["right"].Value.Trim();

            var leftMatches = dependencies.Where(d => d.CanHandleExpression(leftPart));
            var count = leftMatches.Count();
            if (count == 0) return false;
            if (count > 1) throw new NotImplementedException("This should not happen.");

            var rightMatches = dependencies.Where(d => d.CanHandleExpression(rightPart));
            count = rightMatches.Count();
            if (count == 0) return false;
            if (count > 1) throw new NotImplementedException("This should not happen.");

            return true; // Both subexpressions are valid
        }

        protected internal override string OnValidate(ModelProperty evaluatedProperty, IEnumerable<ModelProperty> allProperties, string dependencyExpression)
        {
            var canHandleExpression = RegularExpression.GetMatches(Regex, dependencyExpression);

            var leftPart = canHandleExpression[0].Groups["left"].Value.Trim();
            var leftMatch = dependencies.First(d => d.CanHandleExpression(leftPart));

            var error = leftMatch.OnValidate(evaluatedProperty, allProperties, leftPart);
            if (!string.IsNullOrEmpty(error)) return error;

            var rightPart = canHandleExpression[0].Groups["right"].Value.Trim();
            var rightMatch = dependencies.First(d => d.CanHandleExpression(rightPart));
            return rightMatch.OnValidate(evaluatedProperty, allProperties, rightPart);
        }

        protected internal override Func<IEnumerable<ModelProperty>, bool> OnCompile(ModelProperty evaluatedProperty, IEnumerable<ModelProperty> allProperties, string dependencyExpression)
        {
            return properties =>
                {
                    var canHandleExpression = RegularExpression.GetMatches(Regex, dependencyExpression);

                    var leftPart = canHandleExpression[0].Groups["left"].Value.Trim();
                    var leftMatch = dependencies.First(d => d.CanHandleExpression(leftPart));
                    var leftIsEnabled = leftMatch.OnCompile(evaluatedProperty, properties, leftPart);

                    var rightPart = canHandleExpression[0].Groups["right"].Value.Trim();
                    var rightMatch = dependencies.First(d => d.CanHandleExpression(rightPart));
                    var rightIsEnabled = rightMatch.OnCompile(evaluatedProperty, properties, rightPart);

                    return leftIsEnabled(properties) && rightIsEnabled(properties);
                };
        }
    }
}