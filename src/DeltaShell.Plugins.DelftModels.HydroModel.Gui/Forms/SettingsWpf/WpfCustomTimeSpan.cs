using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xceed.Wpf.Toolkit;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Custom time span so we can display the letter 'd' after days instead of a point.
    /// </summary>
    /// <seealso cref="TimeSpanUpDown" />
    public class WpfCustomTimeSpan : TimeSpanUpDown
    {
        private bool[] timeFrameIsChecked = new bool[5];
       
        /// <summary>
        /// Converts the a <see cref="string"/> value into a <see cref="TimeSpan"/> object.
        /// </summary>
        /// <param name="text">The <see cref="string"/> value to convert.</param>
        /// <returns>A <see cref="TimeSpan"/> object that represents the <paramref name="text"/> argument.</returns>
        /// <remarks><param name="text"/> is expected to be in the form "dd'd' hh:mm:ss.fff" or "dd hh:mm:ss.fff". If
        /// not, it will return a default <see cref="TimeSpan"/> object.</remarks>
        protected override TimeSpan? ConvertTextToValue(string text)
        {
            ReAssignCurrentSelectedTimeFrame();
            if (string.IsNullOrEmpty(text))
                return Value;
           
            //dd'd' hh:mm:ss.fff
            // Separate the above fields
            var dhhmmssfff = text.Split(' ',':','.');
            if (dhhmmssfff[0].Contains("d"))
            {
                var expValue = dhhmmssfff[0].Split('d');
                dhhmmssfff[0] = expValue[0];
            }

            if ( dhhmmssfff.Length !=5 )
                return new TimeSpan(0);

            var d = int.Parse(dhhmmssfff[0]);
            var hh = ConvertToValidDigit(dhhmmssfff[1]);
            var mm = ConvertToValidDigit(dhhmmssfff[2]);
            var ss = ConvertToValidDigit(dhhmmssfff[3]);
            var fff = ConvertToValidDigit(dhhmmssfff[4]);
            
            var currentTimeSpan = new TimeSpan(d, hh, mm, ss, fff);
            return currentTimeSpan;
        }

        private static int ConvertToValidDigit(string textInput)
        {
            var digitsOnly = new Regex(@"[^\d]");
            return Convert.ToInt32(digitsOnly.Replace(textInput, ""));
        }

        /// <summary>
        /// Converts the inner <see cref="TimeSpan"/> value into a string representation of the form "dd'd' hh:mm:ss.fff".
        /// </summary>
        protected override string ConvertValueToText()
        {
            if (!Value.HasValue)
                return string.Empty;

            return string.Format(@"{0:%d\d\ hh\:mm\:ss\.fff}", this.Value.Value);
        }

        protected override void OnTextChanged(string previousValue, string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue))
            {
                if (UpdateValueOnEnterKey)
                    return;
                Value = new TimeSpan?();
            }
            else
            {
                TimeSpan? nullable = this.Value;
                if (nullable.HasValue)
                {
                    nullable = Value;
                }

                if (!TimeSpan.TryParse(currentValue, out TimeSpan result))
                {
                    string[] strArray = currentValue.Split(' ',':', '.');
                    if (strArray.Count<string>() >= 2 && !strArray.Any(x => string.IsNullOrEmpty(x)))
                    {
                        var days = strArray[0].Split('d');
                        result = new TimeSpan(int.Parse(days[0]),
                                              int.Parse(strArray[1]),
                                              int.Parse(strArray[2]),
                                              (this.ShowSeconds ? int.Parse(strArray[2]) : 0),
                                              int.Parse(strArray.Last()));
                    }
                }

                currentValue = result.ToString();
                SyncTextAndValueProperties(true, currentValue);
            }
        }

        /// <summary>
        /// Event that fires when the value of an selected date time part of <see cref="TimeSpan"/> is increased".
        /// </summary>
        protected override void OnIncrement()
        {
            if (Value.HasValue)
            {
                ReAssignCurrentSelectedTimeFrame();
                UpdateTimeSpan(1);
            }
        }

        /// <summary>
        /// Event that fires when the value of an selected date time part of <see cref="TimeSpan"/> is decreased".
        /// </summary>
        protected override void OnDecrement()
        {
            if (Value.HasValue)
            {
                ReAssignCurrentSelectedTimeFrame();
                UpdateTimeSpan(-1);
            }
        }

        private void UpdateTimeSpan(int value)
        {
            if (Value != null)
            {
                var currentTimeSpan = (TimeSpan)Value;

                if (CurrentDateTimePart == DateTimePart.Day)
                {
                    currentTimeSpan = currentTimeSpan.Add(new TimeSpan(value, 0, 0, 0, 0));
                }
                if (CurrentDateTimePart == DateTimePart.Hour24)
                {
                    currentTimeSpan = currentTimeSpan.Add(new TimeSpan(0, value, 0, 0, 0));
                }
                if (CurrentDateTimePart == DateTimePart.Minute)
                {
                    currentTimeSpan = currentTimeSpan.Add(new TimeSpan(0, 0, value, 0, 0));
                }
                if (CurrentDateTimePart == DateTimePart.Second)
                {
                    currentTimeSpan = currentTimeSpan.Add(new TimeSpan(0, 0, 0, value, 0));
                }
                if (CurrentDateTimePart == DateTimePart.Millisecond)
                {
                    currentTimeSpan = currentTimeSpan.Add(new TimeSpan(0, 0, 0, 0, value));
                }

                DelimitTimeSpanValues(currentTimeSpan);
                Value = currentTimeSpan;
            }
        }

        private void ReAssignCurrentSelectedTimeFrame()
        {
            if (CurrentDateTimePart == DateTimePart.Hour24 && !timeFrameIsChecked[0])
            {
                timeFrameIsChecked[0] = true;
                CurrentDateTimePart = CurrentDateTimePart - 5;
                
                return;
            }
            if (CurrentDateTimePart == DateTimePart.Minute && !timeFrameIsChecked[1])
            {
                timeFrameIsChecked[1] = true;
                CurrentDateTimePart = CurrentDateTimePart - 1;
                return;
            }
            if (CurrentDateTimePart == DateTimePart.Second && !timeFrameIsChecked[2])
            {
                timeFrameIsChecked[2] = true;
                CurrentDateTimePart = CurrentDateTimePart - 6;
                return;
            }
            if (CurrentDateTimePart == DateTimePart.Millisecond && !timeFrameIsChecked[3])
            {
                timeFrameIsChecked[3] = true;
                CurrentDateTimePart = CurrentDateTimePart - 5;
                return;
            }
            if (CurrentDateTimePart == DateTimePart.Day && !timeFrameIsChecked[4])
            {
                timeFrameIsChecked[4] = true;
                CurrentDateTimePart = CurrentDateTimePart + 3;
            }
        }

        private void DelimitTimeSpanValues(TimeSpan currentTimeSpan)
        {
            if (IsLowerThan(currentTimeSpan, Minimum))
            {
                Value = Minimum;
                return;
            }

            if (IsGreaterThan(currentTimeSpan, Maximum))
            {
                Value = Maximum;
            }
        }
    }
}