using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    public static class WaveDomainHelper
    {
        public static IList<IWaveDomainData> GetAllDomains(IWaveDomainData root)
        {
            var list = new List<IWaveDomainData> {root};
            AddSubDomains(root, ref list);
            return list;
        }

        public static bool IsValidDomainName(string name, WaveModel model)
        {
            IList<IWaveDomainData> allDomains = GetAllDomains(model.OuterDomain);
            List<string> currentNames = allDomains.SelectMany(d => new[]
            {
                Path.GetFileNameWithoutExtension(d.GridFileName),
                Path.GetFileNameWithoutExtension(d.BedLevelFileName)
            }).ToList();

            return !string.IsNullOrEmpty(name) && !currentNames.Contains(name) && FileUtils.IsValidFileName(name);
        }

        public static bool IsDryPoint(double x, double y)
        {
            return double.IsNaN(x) && double.IsNaN(y);
        }

        private static void AddSubDomains(IWaveDomainData domain, ref List<IWaveDomainData> list)
        {
            foreach (IWaveDomainData subDomain in domain.SubDomains)
            {
                list.Add(subDomain);
                AddSubDomains(subDomain, ref list); // recursively!!
            }
        }
    }
}