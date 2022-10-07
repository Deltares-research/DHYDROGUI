using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    /// <summary>
    /// Validator for the meteo evaporation.
    /// </summary>
    public static class RainfallRunoffMeteoEvaporationValidator
    {
        private const int singleDay = 1;
        private const int daysInLeapYear = 366;
        private const int defaultLeapYear = 4;
        private const string dateFormat = "MMMM dd";
        private static readonly CultureInfo cultureFormat = CultureInfo.InvariantCulture;
        private static readonly string expectedCorrectStartDate = new DateTime(defaultLeapYear, 1, 1).ToString(dateFormat, cultureFormat);
        private static readonly string expectedCorrectEndDate = new DateTime(defaultLeapYear, 12, 31).ToString(dateFormat, cultureFormat);

        private static ValidationIssue CreateSingleIssue(IEvaporationMeteoData meteoData, string msg) => 
            new ValidationIssue(meteoData, ValidationSeverity.Error, msg);
        
        /// <summary>
        /// Validation of the meteo evaporation.
        /// </summary>
        /// <param name="meteoData">Meteo evaporation data to validate.</param>
        /// <returns>Collection of <see cref="ValidationIssue"/>.</returns>
        /// <exception cref="ArgumentNullException"> Thrown when <paramref name="meteoData"/> is null.</exception>
        public static IEnumerable<ValidationIssue> ValidateEvaporationMeteoData(IEvaporationMeteoData meteoData)
        {
            Ensure.NotNull(meteoData, nameof(meteoData));
            
            IList<DateTime> meteoDataDates = GetDateListFromEvaporationMeteoData(meteoData.Data);

            if (!meteoDataDates.Any())
            {
                yield break;
            }
            
            if (!ValidateTimeStepIsOneDay(meteoDataDates, out DateTime incorrectDate))
            {
                string msg = string.Format(Resources.RainfallRunoffMeteoEvaporationValidator_ValidateEvaporationMeteoData_Time_steps_should_be_done_by_day__the_following_date_seems_incorrect__0_, incorrectDate);
                yield return CreateSingleIssue(meteoData, msg);
            }

            if (meteoData.SelectedMeteoDataSource == MeteoDataSource.UserDefined)
            {
                yield break;
            }

            if (!ValidateDaysInFileAreLeapYearAmount(meteoDataDates, out int actualAmountOfDays))
            {
                string msg = string.Format(Resources.RainfallRunoffMeteoEvaporationValidator_ValidateEvaporationMeteoData_The_amount_of_days_was_not_as_the_expected_366_days__it_was___0__days, actualAmountOfDays);
                yield return CreateSingleIssue(meteoData, msg);
            }
            
            if (!ValidateStartDateOfLeapYear(meteoDataDates, out DateTime incorrectStartDate))
            {
                string receivedIncorrectDate = incorrectStartDate.ToString(dateFormat, cultureFormat);
                string msg = string.Format(Resources.RainfallRunoffMeteoEvaporationValidator_ValidateEvaporationMeteoData_The_start_date_is_incorrect____0___is_expected__the_actual_start_date_is____1__, 
                                           expectedCorrectStartDate, 
                                           receivedIncorrectDate);
                yield return CreateSingleIssue(meteoData, msg);
            }
            
            if (!ValidateEndDateOfLeapYear(meteoDataDates, out DateTime incorrectEndDate))
            {
                string receivedIncorrectDate = incorrectEndDate.ToString(dateFormat, cultureFormat);
                string msg = string.Format(Resources.RainfallRunoffMeteoEvaporationValidator_ValidateEvaporationMeteoData_The_end_date_is_incorrect____0___is_expected__the_actual_end_date_is____1__, 
                                           expectedCorrectEndDate, 
                                           receivedIncorrectDate);
                yield return CreateSingleIssue(meteoData, msg);
            }
        }

        private static bool ValidateStartDateOfLeapYear(IList<DateTime> meteoDataDates, out DateTime dateTime)
        {
            DateTime firstDate = meteoDataDates.First();
            dateTime = firstDate;
            return firstDate.Day == 1 && firstDate.Month == 1;
        }
        
        private static bool ValidateEndDateOfLeapYear(IList<DateTime> meteoDataDates, out DateTime dateTime)
        {
            DateTime endDate = meteoDataDates.Last();
            dateTime = endDate;
            return endDate.Day == 31 && endDate.Month == 12;
        }

        private static IList<DateTime> GetDateListFromEvaporationMeteoData(IFunction meteoDataData)
        {
            IMultiDimensionalArray time = meteoDataData.Arguments.First().Values;
            return time.Cast<DateTime>().ToList();
        }

        private static bool ValidateDaysInFileAreLeapYearAmount(IList<DateTime> meteoDataDates, out int i)
        {
            i = meteoDataDates.Count;
            return meteoDataDates.Count == daysInLeapYear;
        }

        private static bool ValidateTimeStepIsOneDay(IList<DateTime> meteoDataDates, out DateTime date)
        {
            DateTime previousDate = meteoDataDates.First();

            foreach (DateTime currentDate in meteoDataDates.Skip(1))
            {
                if (currentDate != previousDate.AddDays(singleDay))
                {
                    date = currentDate;
                    return false;
                }

                previousDate = currentDate;
            }

            date = default;
            return true;
        }
    }
}