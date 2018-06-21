using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.IO;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    public static class WaveDomainHelper
    {
        public static IList<WaveDomainData> GetAllDomains(WaveDomainData root)
        {
            var list = new List<WaveDomainData> {root};
            AddSubDomains(root, ref list);
            return list;
        }

        public static IList<WaveDomainData> GetAllInnerDomains(WaveDomainData root)
        {
            var domains = GetAllDomains(root);
            domains.Remove(root);
            return domains;
        }

        private static void AddSubDomains(WaveDomainData domain, ref List<WaveDomainData> list)
        {
            foreach (var subDomain in domain.SubDomains)
            {

                list.Add(subDomain);
                AddSubDomains(subDomain, ref list); // recursively!!
            }
        }

        public static bool IsValidDomainName(string name, WaveModel model)
        {
            var allDomains = GetAllDomains(model.OuterDomain);
            var currentNames = allDomains.SelectMany(d => new[]
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
    }
}