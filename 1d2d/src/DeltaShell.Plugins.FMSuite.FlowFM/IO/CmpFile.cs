using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class CmpFile : FMSuiteFileBase
    {
        private readonly ILog log = LogManager.GetLogger(typeof(CmpFile));
        public void Write(string cmpFilePath, IEnumerable<HarmonicComponent> astroComponents)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                OpenOutputFile(cmpFilePath);
                try
                {
                    foreach (var astroComponent in astroComponents)
                    {
                        if (!String.IsNullOrEmpty(astroComponent.Name))
                        {
                            WriteLine(String.Format("{0,-10} {1,-15} {2,-15}",
                                                    astroComponent.Name,
                                                    astroComponent.Amplitude,
                                                    astroComponent.Phase));
                        }
                        else
                        {
                            WriteLine(String.Format("{0,-10} {1,-15} {2,-15}",
                                                    FlowBoundaryCondition.GetPeriodInMinutes(astroComponent.Frequency),
                                                    astroComponent.Amplitude,
                                                    astroComponent.Phase));
                        }
                    }
                }
                finally
                {
                    CloseOutputFile();
                }
            }
        }
        
        public BoundaryConditionDataType GetForcingType(string cmpFilePath)
        {
            OpenInputFile(cmpFilePath);
            try
            {
                var line = GetNextLine();
                if (line == null)
                {
                    throw new Exception(string.Format("Could not establish forcing type from empty cmp file {0}",
                                                      cmpFilePath));
                }
                var lineFields = SplitLine(line).ToList();
                if (lineFields.Count < 3)
                {
                    throw new Exception(string.Format("Invalid point row on line {0} in file {1}", LineNumber,
                                                      cmpFilePath));
                }
                var key = lineFields[0];
                if (HarmonicComponent.DefaultAstroComponentsRadPerHour.ContainsKey(key.ToUpper()))
                {
                    return BoundaryConditionDataType.AstroComponents;
                }
                GetDouble(lineFields[0]); // force parsing of frequency
                return BoundaryConditionDataType.Harmonics;
            }
            finally
            {
                CloseInputFile();
            }
        }

        public IList<HarmonicComponent> Read(string cmpFilePath, BoundaryConditionDataType? dataType = null)
        {
            var astroComponents = new List<HarmonicComponent>();

            OpenInputFile(cmpFilePath);

            try
            {
                var line = GetNextLine();

                while (line != null)
                {
                    var lineFields = SplitLine(line).ToList();
                    if (lineFields.Count < 3)
                    {
                        throw new FormatException(String.Format("Invalid point row on line {0} in file {1}", LineNumber,
                                                                cmpFilePath));
                    }

                    var key = lineFields[0].ToUpper();                    
                    var amplitude = GetDouble(lineFields[1], "amplitude");
                    var phase = GetDouble(lineFields[2], "phase");

                    var defaultAstroComponent = HarmonicComponent.DefaultAstroComponentsRadPerHour.ContainsKey(key);
                    if (!defaultAstroComponent && dataType != null &&
                        dataType.Value == BoundaryConditionDataType.AstroComponents)
                    {
                        log.WarnFormat(Resources.CmpFile_Read_Unknown_key__0__from_file__1___It_will_not_be_imported_, key, cmpFilePath);
                        line = GetNextLine();
                        continue;
                    }

                    var harmonicComponent = defaultAstroComponent
                        ? new HarmonicComponent(key, amplitude, phase)
                        : new HarmonicComponent(
                            FlowBoundaryCondition.GetFrequencyInDegPerHour(GetDouble(key, "period")), amplitude, phase);

                    if (dataType != null)
                    {
                        var astroData = dataType.Value == BoundaryConditionDataType.AstroComponents;
                        var astroFile = harmonicComponent.IsAstro();

                        if (astroData != astroFile)
                        {
                            throw new NotSupportedException(
                                string.Format(
                                    "Cmp file {0} with mixed harmonic and astronomic components (line {1}) is not supported.",
                                    cmpFilePath, LineNumber));
                        }
                    }

                    astroComponents.Add(harmonicComponent);

                    line = GetNextLine();
                }
            }
            finally
            {
                CloseInputFile();
            }
            return astroComponents;
        }
    }
}