using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xceed.Wpf.Toolkit;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Custom time span so we can display the letter 'd' after days instead of a point.
    /// </summary>
    /// <seealso cref="Xceed.Wpf.Toolkit.TimeSpanUpDown"/>
    public class WpfCustomTimeSpan : TimeSpanUpDown
    {
        /// <summary>
        /// Converts the a <see cref="string"/> value into a <see cref="TimeSpan"/> object.
        /// </summary>
        /// <param name="text">The <see cref="string"/> value to convert.</param>
        /// <returns>A <see cref="TimeSpan"/> object that represents the <paramref name="text"/> argument.</returns>
        /// <remarks>
        /// <param name="text"/>
        /// is expected to be in the form "dd'd' hh:mm:ss.fff" or "dd hh:mm:ss.fff". If
        /// not, it will return a default <see cref="TimeSpan"/> object.
        /// </remarks>
        protected override TimeSpan? ConvertTextToValue(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Value;
            }

            //dd'd' hh:mm:ss.fff
            // Separate the above fields
            string[] dhhmmssfff = text.Split(':');
            if (dhhmmssfff.Length != 3)
            {
                return new TimeSpan(0);
            }

            // Get seconds and milliseconds
            string[] ssfff = dhhmmssfff[2].Split('.');
            int ss = ConvertToValidDigit(ssfff.First());
            int fff = ssfff.Length == 2
                          ? ConvertToValidDigit(ssfff.Last())
                          : 0;

            //Get minutes
            int mm = ConvertToValidDigit(dhhmmssfff[1]);

            string[] dhh = GetDaysAndHours(dhhmmssfff);
            int hh = GetHours(dhh);
            int d = GetDays(dhh);

            //Return new timespan
            return new TimeSpan(d, hh, mm, ss, fff);
        }

        /// <summary>
        /// Converts the inner <see cref="TimeSpan"/> value into a string representation of the form "dd'd' hh:mm:ss.fff".
        /// </summary>
        protected override string ConvertValueToText()
        {
            if (!Value.HasValue)
            {
                return string.Empty;
            }

            return string.Format(@"{0:%d\d\ hh\:mm\:ss\.fff}", Value.Value);
        }

        private static string[] GetDaysAndHours(string[] dhhmmssfff)
        {
            string[] dhh = dhhmmssfff[0].Split('d');
            dhh = dhh.Length == 2 ? dhh : dhh.Last().Split(' ');
            return dhh;
        }

        private static int GetDays(string[] dhh)
        {
            string daysStringValue = dhh.First().Split(' ').Last();
            return dhh.Length == 2
                       ? ConvertToValidDigit(daysStringValue)
                       : 0;
        }

        private static int GetHours(IEnumerable<string> dhh)
        {
            string hoursStringValue = dhh.Last().Split(' ').Last();
            return ConvertToValidDigit(hoursStringValue);
        }

        private static int ConvertToValidDigit(string textInput)
        {
            var digitsOnly = new Regex(@"[^\d]");
            return Convert.ToInt32(digitsOnly.Replace(textInput, ""));
        }
    }
}