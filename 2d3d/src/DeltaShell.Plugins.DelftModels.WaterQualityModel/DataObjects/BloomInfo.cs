using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects
{
    public class BloomInfo
    {
        private readonly List<string> algHeaders;
        private readonly List<string> korts;
        private readonly IDictionary<string, string> descriptions;

        private readonly List<string> allParameters;

        public BloomInfo(List<string> algHeaders, List<string> korts, List<string> descriptions)
        {
            this.algHeaders = algHeaders;
            this.korts = korts;
            allParameters = ExpandParameterNames();

            this.descriptions = korts.Zip(descriptions, (k, d) => new[]
            {
                k,
                d
            }).ToDictionary(t => t[0], t => t[1]);
        }

        /// <summary>
        /// Returns an expansion of all korts and headers. They make up the parameter
        /// names as present in the library.
        /// </summary>
        public IEnumerable<string> AllParameters => allParameters;

        public List<string> Korts => korts;

        public List<string> Headers => algHeaders;

        public IEnumerable<string> Descriptions => descriptions.Values;

        public string MakeParameter(string header, string kort)
        {
            string subString = header.Substring(0, header.Length - 3);
            return (subString + kort).ToLowerInvariant();
        }

        public string GetKortDescription(string kort)
        {
            string result;
            return descriptions.TryGetValue(kort, out result) ? result : null;
        }

        public IEnumerable<string> GetKortsPresentInFunctions(IEnumerable<IFunction> functions)
        {
            return korts.Where(
                kort => functions.Any(f => f.Name.EndsWith(kort, StringComparison.InvariantCultureIgnoreCase)));
        }

        public IEnumerable<string> GetHeadersPresentInFunctions(IEnumerable<IFunction> functions)
        {
            foreach (string header in algHeaders)
            {
                string subString = header.Substring(0, header.Length - 3);
                if (functions.Any(f => f.Name.StartsWith(subString, StringComparison.InvariantCultureIgnoreCase)))
                {
                    yield return header;
                }
            }
        }

        private List<string> ExpandParameterNames()
        {
            return algHeaders.SelectMany(alga => korts, MakeParameter).ToList();
        }
    }
}