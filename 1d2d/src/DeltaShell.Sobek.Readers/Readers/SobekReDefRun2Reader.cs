using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekReDefRun2Reader : SobekReader<SobekCaseSettings>
    {
        public SobekCaseSettings SobekCaseSettingsInstance { get; set; }
        private SobekCaseSettings settingsToAdapt;

        public override IEnumerable<SobekCaseSettings> Parse(string text)
        {
            return RegularExpression.GetMatches(@"(FLNM (?'text'.*?)flnm)", text)
                .Cast<Match>()
                .Select(structureMatch => ParseSobekCaseSettings(structureMatch.Value))
                .Where(definition => definition != null);
        }

        // This file contains numerical parameters for the flow module.
        // FLNM g_ 9.81 th 0.55 ps 0.5 rh 1000 ur 0.5 mi 50 sw 0.01 sd 0.1 cm 1 er 0 us 1
        // in 0.001 pc 1000 xn 50 sm 0.01 dt 1 flnm
        // 
        // Where:
        //  g = gravity acceleration (9.81 m/s2) 
        //  th = theta (default 0.55)
        //  ps = psi (default 0.5)
        //  rh = rho (density of water; default 1000)
        //  ur = under relaxation (default 0.5)
        //  mi = max. number iterations (default 50)
        //  sw = stop criteria water level (default 0.01)
        //  sd = stop criteria discharge (default 0.1)
        //  sr = relative discharge stop criteria
        //  cm = calculation mode
        //    0 = steady
        //    1 = unsteady (default = unsteady)
        //  gm = continue after convergance
        //    1 = no
        //    0 = yes
        //  er = extra resistance (default 0)
        //  us = under relaxation structure (default 1)
        //  in = increment numerical differences structures (default 0.001)
        //  pc = pseudo-Courant number (default 1000)
        //  xn = max. number of iteration NAM (Nodal Administration Matrix) (default 50)
        //  sm = stop criteria NAM
        //  dt = transition height for summer dikes (default 1)
        //  xr = type extra resistance
        //////    0 = eta  NB online help has these values switched
        //////    1 = ksi
        //    0 = ksi
        //    1 = eta
        // The above is a SobekRe record. Avoid reusing numerical parameters
        private SobekCaseSettings ParseSobekCaseSettings(string record)
        {
            settingsToAdapt = new SobekCaseSettings();
            
            var pattern = RegularExpression.GetScientific("g_") + "|" +
                             RegularExpression.GetScientific("xr");

            foreach (Match match in RegularExpression.GetMatches(pattern, record))
            {
                settingsToAdapt.GravityAcceleration = RegularExpression.ParseDouble(match, "g_", SobekCaseSettingsInstance.GravityAcceleration);
                settingsToAdapt.UseKsiForExtraResistance =
                    RegularExpression.ParseInt(match, "xr", SobekCaseSettingsInstance.UseKsiForExtraResistance ? 0 : 1) == 0
                        ? true
                        : false;
                ApplyAdaptedSettingsToExistingSettings(settingsToAdapt);
            }

            return settingsToAdapt;
        }

        public void ApplyAdaptedSettingsToExistingSettings(SobekCaseSettings adaptedSettings)
        {
            SobekCaseSettingsInstance.GravityAcceleration = adaptedSettings.GravityAcceleration;
            SobekCaseSettingsInstance.UseKsiForExtraResistance = adaptedSettings.UseKsiForExtraResistance;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "flnm";
        }
    }
}
