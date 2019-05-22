using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Custom time span so we can display the letter 'd' after days instead of a point.
    /// </summary>
    /// <seealso cref="Xceed.Wpf.Toolkit.TimeSpanUpDown" />
    public class WpfCustomTimeSpan : Xceed.Wpf.Toolkit.TimeSpanUpDown
    {
        /// <summary>
        /// Converts the a <see cref="string"/> value into a <see cref="TimeSpan"/> object.
        /// </summary>
        /// <param name="text">The <see cref="string"/> value to convert.</param>
        /// <returns>A <see cref="TimeSpan"/> object that represents the <paramref name="text"/> argument.</returns>
        /// <remarks><param name="text"/> is expected to be in the form "dd'd' hh:mm:ss.fff" or "dd hh:mm:ss.fff". If
        /// not, it will return a default <see cref="TimeSpan"/> object.</remarks>
        protected override TimeSpan? ConvertTextToValue(string text)
        {
            if (string.IsNullOrEmpty(text))
                return this.Value;
            //dd'd' hh:mm:ss.fff
            // Separate the above fields
            var dhhmmssfff = text.Split(':');
            if( dhhmmssfff.Length !=3 )
                return new TimeSpan(0);

            // Get seconds and milliseconds
            var ssfff = dhhmmssfff[2].Split('.');
            var ss = ConvertToValidDigit(ssfff.First());
            var fff = ssfff.Length == 2 
                ? ConvertToValidDigit(ssfff.Last()) 
                : 0;
            
            //Get minutes
            var mm = ConvertToValidDigit(dhhmmssfff[1]);

            var dhh = GetDaysAndHours(dhhmmssfff);
            var hh = GetHours(dhh);
            var d = GetDays(dhh);

            //Return new timespan
            return new TimeSpan(d, hh, mm, ss, fff);
        }

        private static string[] GetDaysAndHours(string[] dhhmmssfff)
        {
            var dhh = dhhmmssfff[0].Split('d');
            dhh = dhh.Length == 2 ? dhh : dhh.Last().Split(' ');
            return dhh;
        }

        private static int GetDays(string[] dhh)
        {
            var daysStringValue = dhh.First().Split(' ').Last();
            return dhh.Length == 2 
                ? ConvertToValidDigit(daysStringValue) 
                : 0;
        }

        private static int GetHours(IEnumerable<string> dhh)
        {
            var hoursStringValue = dhh.Last().Split(' ').Last();
            return ConvertToValidDigit(hoursStringValue);
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
            if (!this.Value.HasValue)
                return string.Empty;

            return string.Format(@"{0:%d\d\ hh\:mm\:ss\.fff}", this.Value.Value);
        }
    }
}