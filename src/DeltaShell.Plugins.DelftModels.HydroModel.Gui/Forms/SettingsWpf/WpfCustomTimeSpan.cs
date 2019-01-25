using System;
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
        /// Converts the text to value.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        protected override TimeSpan? ConvertTextToValue(string text)
        {
            if (string.IsNullOrEmpty(text))
                return this.Value;
            //dd'd' hh:mm:ss.fff
            // Separate the above fields
            var dhhmmssfff = text.Split(':');
            if( dhhmmssfff.Length !=3 )
                return new TimeSpan(0);

            // Get seconds and miliseconds
            var ssfff = dhhmmssfff[2].Split('.');
            var ss = ReplaceToValidDigit(ssfff.First());
            var fff = ssfff.Length == 2 
                ? ReplaceToValidDigit(ssfff.Last()) 
                : 0;
            
            //Get minutes
            var mm = ReplaceToValidDigit(dhhmmssfff[1]);

            //Get hours and days
            var dhh = dhhmmssfff[0].Split('d');
            var hh = ReplaceToValidDigit(dhh.Last());
            var d = dhh.Length == 2 ? ReplaceToValidDigit(dhh.First()) : 0;

            //Return new timespan
            return new TimeSpan(d, hh, mm, ss, fff);
        }

        private int ReplaceToValidDigit(string textInput)
        {
            var digitsOnly = new Regex(@"[^\d]");
            return Convert.ToInt32(digitsOnly.Replace(textInput, ""));
        }

        /// <summary>
        /// Converts the value to text.
        /// </summary>
        /// <returns></returns>
        protected override string ConvertValueToText()
        {
            if (!this.Value.HasValue)
                return string.Empty;

            return string.Format(@"{0:%d\d\ hh\:mm\:ss\.fff}", this.Value.Value);
        }
    }
}