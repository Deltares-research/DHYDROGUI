using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    // String handling
    public class StringBufferHandling
    {
        // make filled buffers for writing
        public static string MakeStringBuffer(ref string[] strings, int padding)
        {

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < strings.Length; i++)
            {
                var idsString = strings[i] ?? string.Empty;
                idsString = idsString.PadRight(padding, ' ');
                stringBuilder.Append(idsString);
            }

            return stringBuilder.ToString();
        }

        //make empty string buffers for reading
        public static string MakeStringBuffer(int nstrings, int padding)
        {
            var str = new string('_', nstrings * padding);
            return str;
        }

        public static IList<string> ParseString(IntPtr c_str, int numElements, int chunkSize)
        {
            var byteArray = new byte[numElements * chunkSize];
            Marshal.Copy(c_str, byteArray, 0, numElements * chunkSize);
            var str = Encoding.ASCII.GetString(byteArray);
            return Split(str, chunkSize);
        }

        public static IList<string> Split(string str, int chunkSize)
        {

            var en = Enumerable.Range(0, str.Length / chunkSize).Select(i => str.Substring(i * chunkSize, chunkSize));
            var l = new List<string>();
            foreach (var e in en)
            {
                l.Add(e.Trim());
            }

            return l;
        }
    }
}