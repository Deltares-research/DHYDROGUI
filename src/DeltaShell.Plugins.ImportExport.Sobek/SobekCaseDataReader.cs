using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekCaseDataReader
    {
        public static SobekCaseData ReadCaseData(string caseDataPath)
        {
            var sobekCaseData = new SobekCaseData();

            IList<string> caseData = new List<string>();
            using (var reader = new StreamReader(caseDataPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    caseData.Add(line.ToUpper());
                }
            }
            // Get first path that refers to wind data (wdc = constant, wnd is timeseries) SobekWindReader handle both
            string windDataPath = caseData.FirstOrDefault(t => (t.Contains(".WDC ") || (t.Contains(".WND "))));
            if (windDataPath != null)
            {
                sobekCaseData.WindDataPath = GetRelativePathFromCaseToSobekFixed(windDataPath.Split(' ')[1], caseDataPath);
            }
            // Get path that refers to precipitation data
            string buiDataPath = caseData.FirstOrDefault(t => (t.Contains(".BUI ")));
            if (buiDataPath != null)
            {
                sobekCaseData.BuiDataPath = GetRelativePathFromCaseToSobekFixed(buiDataPath.Split(' ')[1], caseDataPath);
            }
            return sobekCaseData;
        }

        public static string GetRelativePathFromCaseToSobekFixed(string absoluteFilePath, string caseDataPath)
        {
            return Path.GetFullPath(Path.GetDirectoryName(caseDataPath) + @"\..\..\FIXED\" + Path.GetFileName(absoluteFilePath));
        }
    }
}