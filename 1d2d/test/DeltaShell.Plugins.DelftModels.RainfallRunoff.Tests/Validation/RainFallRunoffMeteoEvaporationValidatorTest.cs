using System;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Validation
{
    [TestFixture]
    public class RainFallRunoffMeteoEvaporationValidatorTest
    {
        private EvaporationMeteoData evaporationMeteoData;
        private IVariable datesAndTimes;
        private const string dateFormat = "MMMM dd";
        private const int defaultLeapYear = 4;
        private readonly CultureInfo cultureFormat = CultureInfo.InvariantCulture;

        [SetUp]
        public void SetUp()
        {
            evaporationMeteoData = new EvaporationMeteoData();
            evaporationMeteoData.Data.Arguments.Clear();
            datesAndTimes = Substitute.For<IVariable>();
        }
        
        [Test]
        public void WhenValidateEvaporationMeteoData_MeteoDataIsNull_ShouldThrowArgumentNull()
        {
            // Arrange & Call
            void Action() => RainfallRunoffMeteoEvaporationValidator.ValidateEvaporationMeteoData(null).ToList();
            
            // Assert
            Assert.That(Action, Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(MeteoDataSource.UserDefined)]
        [TestCase(MeteoDataSource.GuidelineSewerSystems)]
        [TestCase(MeteoDataSource.LongTermAverage)]
        public void WhenEvaporationMeteo_DatesContainsMultipleEntriesForSameDay_TimeStepMessageExpected(MeteoDataSource source)
        {
            //Arrange
            evaporationMeteoData.SelectedMeteoDataSource = source;

            IMultiDimensionalArray<DateTime> listOfDates = new MultiDimensionalArray<DateTime>();
            listOfDates.Add(new DateTime(defaultLeapYear, 1, 1, 1,0,0));
            listOfDates.Add(new DateTime(defaultLeapYear, 1, 1, 2,0,0));
            
            datesAndTimes.Values = listOfDates;
            evaporationMeteoData.Data.Arguments = new EventedList<IVariable> {datesAndTimes};

            //Act
            var report = RainfallRunoffMeteoEvaporationValidator.ValidateEvaporationMeteoData(evaporationMeteoData).ToList();

            string expectedMessage = string.Format(Resources.RainfallRunoffMeteoEvaporationValidator_ValidateEvaporationMeteoData_Time_steps_should_be_done_by_day__the_following_date_seems_incorrect__0_,
                                                   listOfDates[1]);
            //Assert
            Assert.That(report.First().Message, Is.EqualTo(expectedMessage));
        }
        
        [Test]
        [TestCase(1,3)]
        [TestCase(2,1)]
        public void WhenEvaporationMeteoUserDefined_DatesContainsNonConsecutiveDays_TimeStepMessageExpected(int firstDay, int secondDay)
        {
            //Arrange
            evaporationMeteoData.SelectedMeteoDataSource = MeteoDataSource.UserDefined;

            IMultiDimensionalArray<DateTime> listOfDates = new MultiDimensionalArray<DateTime>();
            listOfDates.Add(new DateTime(defaultLeapYear, 1, firstDay));
            listOfDates.Add(new DateTime(defaultLeapYear, 1, secondDay));
            datesAndTimes.Values = listOfDates;
            evaporationMeteoData.Data.Arguments = new EventedList<IVariable> {datesAndTimes};

            //Act
            var report = RainfallRunoffMeteoEvaporationValidator.ValidateEvaporationMeteoData(evaporationMeteoData).ToList();

            string expectedMessage = string.Format(Resources.RainfallRunoffMeteoEvaporationValidator_ValidateEvaporationMeteoData_Time_steps_should_be_done_by_day__the_following_date_seems_incorrect__0_,
                                                   listOfDates[1]);
            //Assert
            Assert.That(report.First().Message, Is.EqualTo(expectedMessage));
        }
        
        [Test]
        [TestCase(MeteoDataSource.GuidelineSewerSystems)]
        [TestCase(MeteoDataSource.LongTermAverage)]
        public void WhenEvaporationMeteoNotUserDefined_AndDatesContainLeapYearAmount_NoMessageExpected(MeteoDataSource source)
        {
            //Arrange
            evaporationMeteoData.SelectedMeteoDataSource = source;
            datesAndTimes.Values = GetDefaultLeapYear();
            evaporationMeteoData.Data.Arguments = new EventedList<IVariable> {datesAndTimes};

            //Act
            var report = RainfallRunoffMeteoEvaporationValidator.ValidateEvaporationMeteoData(evaporationMeteoData).ToList();

            //Assert
            Assert.That(report.Count, Is.EqualTo(0));
        }
        
        [Test]
        [TestCase(MeteoDataSource.GuidelineSewerSystems, 400)]
        [TestCase(MeteoDataSource.LongTermAverage, 400)]
        [TestCase(MeteoDataSource.GuidelineSewerSystems, 200)]
        [TestCase(MeteoDataSource.LongTermAverage, 200)]
        public void WhenEvaporationMeteoNotUserDefined_AndDatesContainNotLeapYearAmount_AmountOfDaysIncorrectMessageExpected(MeteoDataSource source, int amountOfDays)
        {
            //Arrange
            evaporationMeteoData.SelectedMeteoDataSource = source;
            datesAndTimes.Values = GetMdaOfDateTimes(amountOfDays);
            evaporationMeteoData.Data.Arguments = new EventedList<IVariable> {datesAndTimes};

            //Act
            var report = RainfallRunoffMeteoEvaporationValidator.ValidateEvaporationMeteoData(evaporationMeteoData).ToList();

            string expectedMessage = string.Format(Resources.RainfallRunoffMeteoEvaporationValidator_ValidateEvaporationMeteoData_The_amount_of_days_was_not_as_the_expected_366_days__it_was___0__days, 
                                                   amountOfDays);
            //Assert
            Assert.That(report.First().Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        [TestCase(MeteoDataSource.GuidelineSewerSystems)]
        [TestCase(MeteoDataSource.LongTermAverage)]
        public void WhenEvaporationMeteoNotUserDefined_AndDatesContainLeapYearAmount_ButStartAndEndDayAreIncorrect_IncorrectStartAndEndDateMessagesExpected(MeteoDataSource source)
        {
            //Arrange
            evaporationMeteoData.SelectedMeteoDataSource = source;
            datesAndTimes.Values = GetMdaOfDateTimes(366, 2);
            evaporationMeteoData.Data.Arguments = new EventedList<IVariable> {datesAndTimes};
            
            DateTime expectedCorrectStartDateTime = new DateTime(defaultLeapYear, 1, 1);
            DateTime expectedIncorrectStartDateTime = new DateTime(defaultLeapYear, 1, 2);
            
            string expectedStartDateMessage = GetExpectedDateMessage(expectedCorrectStartDateTime,
                                                                     expectedIncorrectStartDateTime,
                                                                     Resources.RainfallRunoffMeteoEvaporationValidator_ValidateEvaporationMeteoData_The_start_date_is_incorrect____0___is_expected__the_actual_start_date_is____1__);

            DateTime expectedCorrectEndDateTime = new DateTime(defaultLeapYear, 12, 31);
            DateTime expectedIncorrectEndDateTime = new DateTime(defaultLeapYear+1, 1, 1);


            string expectedEndDateMessage = GetExpectedDateMessage(expectedCorrectEndDateTime,
                                                                   expectedIncorrectEndDateTime,
                                                                   Resources.RainfallRunoffMeteoEvaporationValidator_ValidateEvaporationMeteoData_The_end_date_is_incorrect____0___is_expected__the_actual_end_date_is____1__);
            //Act
            var report = RainfallRunoffMeteoEvaporationValidator.ValidateEvaporationMeteoData(evaporationMeteoData).ToList();

            //Assert
            Assert.That(report, Has.Count.EqualTo(2));
            Assert.That(report.First().Message, Is.EqualTo(expectedStartDateMessage));
            Assert.That(report.Last().Message, Is.EqualTo(expectedEndDateMessage));
        }

        [Test]
        public void WhenEvaporationMeteoUserDefined_AndEmptyDataReceived_NoMessagesExpected()
        {
            //Arrange
            evaporationMeteoData.SelectedMeteoDataSource = MeteoDataSource.UserDefined;
            datesAndTimes.Values.Count.Returns(0);
            evaporationMeteoData.Data.Arguments = new EventedList<IVariable> {datesAndTimes};
            
            //Act
            var report = RainfallRunoffMeteoEvaporationValidator.ValidateEvaporationMeteoData(evaporationMeteoData).ToList();

            //Assert
            Assert.That(report, Has.Count.EqualTo(0));
        }

        private string GetExpectedDateMessage(DateTime expectedDate, DateTime receivedDate, string resourceString)
        {
            return string.Format(resourceString, expectedDate.ToString(dateFormat, cultureFormat), receivedDate.ToString(dateFormat, cultureFormat));
        }
        
        private static IMultiDimensionalArray<DateTime> GetMdaOfDateTimes(int maxAmountOfDays, int startDay = 1)
        {
            IMultiDimensionalArray<DateTime> listOfDates = new MultiDimensionalArray<DateTime>();

            for (int i = 0; i < maxAmountOfDays; i++)
            {
                listOfDates.Add(new DateTime(defaultLeapYear, 1, startDay));
            }

            for (int i = 1; i < maxAmountOfDays; i++)
            {
                listOfDates[i] = listOfDates[i].AddDays(i);
            }

            return listOfDates;
        }

        private static IMultiDimensionalArray<DateTime> GetDefaultLeapYear()
        {
            return GetMdaOfDateTimes(366);
        }
    }
}