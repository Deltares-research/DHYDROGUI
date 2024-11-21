using System;
using Deltares.Infrastructure.API.Guards;
using DHYDRO.Common.Properties;
using FluentValidation;

namespace DHYDRO.Common.IO.Validation
{
    internal static class RuleBuilderOptionsExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> WithMissingValueMessage<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            string propertyName,
            Func<T, int> lineNumber)
        {
            Ensure.NotNull(rule, nameof(rule));
            Ensure.NotNullOrWhiteSpace(propertyName, nameof(propertyName));
            Ensure.NotNull(lineNumber, nameof(lineNumber));

            return rule.WithMessage(t => MissingValueMessage(propertyName, lineNumber(t)));
        }

        private static string MissingValueMessage(string propertyName, int lineNumber)
        {
            return FormatMessage(string.Format(Resources.Property_0_must_be_provided, propertyName), lineNumber);
        }

        public static IRuleBuilderOptions<T, TProperty> WithMessage<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            Func<T, string> messageProvider,
            Func<T, int> lineProvider)
        {
            Ensure.NotNull(rule, nameof(rule));
            Ensure.NotNull(messageProvider, nameof(messageProvider));
            Ensure.NotNull(lineProvider, nameof(lineProvider));

            return rule.WithMessage(x => FormatMessage(messageProvider(x), lineProvider(x)));
        }

        public static IRuleBuilderOptions<T, TProperty> WithMessage<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule,
            Func<T, TProperty, string> messageProvider,
            Func<T, int> lineProvider)
        {
            Ensure.NotNull(rule, nameof(rule));
            Ensure.NotNull(messageProvider, nameof(messageProvider));
            Ensure.NotNull(lineProvider, nameof(lineProvider));

            return rule.WithMessage((x, p) => FormatMessage(messageProvider(x, p), lineProvider(x)));
        }

        private static string FormatMessage(string message, int lineNumber)
        {
            return string.Format(Resources._0_Line_1_, message, lineNumber);
        }
    }
}