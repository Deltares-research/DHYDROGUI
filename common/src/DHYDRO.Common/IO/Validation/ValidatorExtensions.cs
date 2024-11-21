using Deltares.Infrastructure.API.Logging;
using FluentValidation;
using FluentValidation.Results;

namespace DHYDRO.Common.IO.Validation
{
    internal static class ValidatorExtensions
    {
        public static bool IsValidWithLogging<T>(this IValidator<T> validator, T instance, ILogHandler logHandler)
        {
            ValidationResult validationResult = validator.Validate(instance);
            if (validationResult.IsValid)
            {
                return true;
            }

            foreach (ValidationFailure validationFailure in validationResult.Errors)
            {
                logHandler.ReportError(validationFailure.ErrorMessage);
            }

            return false;
        }
    }
}