using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    public static class FouFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FouFileReader));

        public static void ReadFouFile(string targetDir, WaterFlowFMModelDefinition modelDefinition)
        {
            Ensure.NotNullOrEmpty(targetDir, nameof(targetDir));
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));

            string fouFilePath = IsFouFileUsed(targetDir, modelDefinition);
            if (fouFilePath == null)
            {
                return;
            }

            List<FouFileRow> fouFileEntries = ReadFouFile(fouFilePath);

            UpdateWlProperty(fouFileEntries, modelDefinition);
            UpdateUcProperty(fouFileEntries, modelDefinition);
            UpdateFbProperty(fouFileEntries, modelDefinition);
            UpdateWdogProperty(fouFileEntries, modelDefinition);
            UpdateVogProperty(fouFileEntries, modelDefinition);
        }

        private static void UpdateWlProperty(IReadOnlyCollection<FouFileRow> rows, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty writeWlAverage = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWlAverage);
            IEnumerable<FouFileRow> row = rows.Where(t => t.Var.Equals(FouFileProperties.VarWaterLevel, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpAverage, StringComparison.InvariantCultureIgnoreCase));
            writeWlAverage.Value = row.Any();

            WaterFlowFMProperty writeWlMaximum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWlMaximum);
            row = rows.Where(t => t.Var.Equals(FouFileProperties.VarWaterLevel, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpMaximum, StringComparison.InvariantCultureIgnoreCase));
            writeWlMaximum.Value = row.Any();

            WaterFlowFMProperty writeWlMinimum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWlMinimum);
            row = rows.Where(t => t.Var.Equals(FouFileProperties.VarWaterLevel, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpMinimum, StringComparison.InvariantCultureIgnoreCase));
            writeWlMinimum.Value = row.Any();
        }

        private static void UpdateUcProperty(IReadOnlyCollection<FouFileRow> rows, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty writeUcAverage = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteUcAverage);
            IEnumerable<FouFileRow> row = rows.Where(t => t.Var.Equals(FouFileProperties.VarVelocityMagnitude, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpAverage, StringComparison.InvariantCultureIgnoreCase));
            writeUcAverage.Value = row.Any();

            WaterFlowFMProperty writeUcMaximum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteUcMaximum);
            row = rows.Where(t => t.Var.Equals(FouFileProperties.VarVelocityMagnitude, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpMaximum, StringComparison.InvariantCultureIgnoreCase));
            writeUcMaximum.Value = row.Any();

            WaterFlowFMProperty writeUcMinimum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteUcMinimum);
            row = rows.Where(t => t.Var.Equals(FouFileProperties.VarVelocityMagnitude, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpMinimum, StringComparison.InvariantCultureIgnoreCase));
            writeUcMinimum.Value = row.Any();
        }

        private static void UpdateFbProperty(IReadOnlyCollection<FouFileRow> rows, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty writeFbAverage = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteFbAverage);
            IEnumerable<FouFileRow> row = rows.Where(t => t.Var.Equals(FouFileProperties.VarFreeboard, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpAverage, StringComparison.InvariantCultureIgnoreCase));
            writeFbAverage.Value = row.Any();

            WaterFlowFMProperty writeFbMaximum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteFbMaximum);
            row = rows.Where(t => t.Var.Equals(FouFileProperties.VarFreeboard, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpMaximum, StringComparison.InvariantCultureIgnoreCase));
            writeFbMaximum.Value = row.Any();

            WaterFlowFMProperty writeFbMinimum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteFbMinimum);
            row = rows.Where(t => t.Var.Equals(FouFileProperties.VarFreeboard, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpMinimum, StringComparison.InvariantCultureIgnoreCase));
            writeFbMinimum.Value = row.Any();
        }

        private static void UpdateWdogProperty(IReadOnlyCollection<FouFileRow> rows, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty writeWdogAverage = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWdogAverage);
            IEnumerable<FouFileRow> row = rows.Where(t => t.Var.Equals(FouFileProperties.VarWaterDepthOnGround, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpAverage, StringComparison.InvariantCultureIgnoreCase));
            writeWdogAverage.Value = row.Any();

            WaterFlowFMProperty writeWdogMaximum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWdogMaximum);
            row = rows.Where(t => t.Var.Equals(FouFileProperties.VarWaterDepthOnGround, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpMaximum, StringComparison.InvariantCultureIgnoreCase));
            writeWdogMaximum.Value = row.Any();

            WaterFlowFMProperty writeWdogMinimum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWdogMinimum);
            row = rows.Where(t => t.Var.Equals(FouFileProperties.VarWaterDepthOnGround, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpMinimum, StringComparison.InvariantCultureIgnoreCase));
            writeWdogMinimum.Value = row.Any();
        }

        private static void UpdateVogProperty(IReadOnlyCollection<FouFileRow> rows, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty writeVogAverage = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteVogAverage);
            IEnumerable<FouFileRow> row = rows.Where(t => t.Var.Equals(FouFileProperties.VarWaterVolumeOnGround, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpAverage, StringComparison.InvariantCultureIgnoreCase));
            writeVogAverage.Value = row.Any();

            WaterFlowFMProperty writeVogMaximum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteVogMaximum);
            row = rows.Where(t => t.Var.Equals(FouFileProperties.VarWaterVolumeOnGround, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpMaximum, StringComparison.InvariantCultureIgnoreCase));
            writeVogMaximum.Value = row.Any();

            WaterFlowFMProperty writeVogMinimum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteVogMinimum);
            row = rows.Where(t => t.Var.Equals(FouFileProperties.VarWaterVolumeOnGround, StringComparison.InvariantCultureIgnoreCase) && t.Elp.Equals(FouFileProperties.ElpMinimum, StringComparison.InvariantCultureIgnoreCase));
            writeVogMinimum.Value = row.Any();
        }        
        
        private static string IsFouFileUsed(string targetDir, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty fouFileProperty = modelDefinition.GetModelProperty(FouFileProperties.MduFouFileProperty);
            var fouFileName = (string) fouFileProperty.Value;
            if (string.IsNullOrEmpty(fouFileName))
            {
                return null;
            }
            
            string fouFilePath = Path.Combine(targetDir, fouFileName);
            if (!File.Exists(fouFilePath))
            {
                return null;
            }

            EnableFouFileUsedSettings(modelDefinition);
            return fouFilePath;
        }

        private static void EnableFouFileUsedSettings(WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty fouFileSettingsEnabled = modelDefinition.GetModelProperty(FouFileProperties.WriteFouFile);
            fouFileSettingsEnabled.Value = true;
        }

        private static List<FouFileRow> ReadFouFile(string path)
        {
            var rows = new List<FouFileRow>();
            using (StreamReader sr = OpenFile(path))
            {
                // ignore the first line, this is the header.
                sr.ReadLine();

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    IEnumerable<string> tokens = Tokenize(line);
                    FouFileRow entry = FouFileEntry(tokens.ToList());
                    if (entry != null)
                    {
                        rows.Add(entry);
                    }
                }
            }

            return rows;
        }

        private static IEnumerable<string> Tokenize(string make)
        {
            if (string.IsNullOrEmpty(make))
            {
                return Enumerable.Empty<string>();
            }

            return make.SplitOnEmptySpace();
        }

        private static FouFileRow FouFileEntry(List<string> tokens)
        {
            FouFileRow wl = MakeWaterLevel(tokens);
            if (wl != null)
            {
                return wl;
            }

            FouFileRow uc = MakeVelocityMagnitude(tokens);
            if (uc != null)
            {
                return uc;
            }

            FouFileRow fb = MakeFreeboard(tokens);
            if (fb != null)
            {
                return fb;
            }

            FouFileRow wdog = MakeWaterDepthOnGround(tokens);
            if (wdog != null)
            {
                return wdog;
            }

            FouFileRow vog = MakeWaterVolumeOnGround(tokens);
            if (vog != null)
            {
                return vog;
            }

            return null;
        }

        private static FouFileRow MakeWaterLevel(IReadOnlyList<string> tokens)
        {
            if (tokens.Count() < 6 || tokens.Count() > 7)
            {
                // invalid number of tokens
                return null;
            }

            string firstToken = tokens[0];
            if (!string.Equals(firstToken, FouFileProperties.VarWaterLevel, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            FouFileRow fouFileRow = null;
            try
            {
                fouFileRow = new FouFileRow();
                fouFileRow.Var = tokens[0];
                fouFileRow.Tsrts = Convert.ToDouble(tokens[1]);
                fouFileRow.Sstop = Convert.ToDouble(tokens[2]);
                fouFileRow.Numcyc = Convert.ToInt32(tokens[3]);
                fouFileRow.Knfac = Convert.ToInt32(tokens[4]);
                fouFileRow.V0plu = Convert.ToInt32(tokens[5]);
                fouFileRow.Layno = null;
                if (tokens.Count() == 7)
                {
                    fouFileRow.Elp = tokens[6];
                }
                else
                {
                    fouFileRow.Elp = "";
                }
            }
            catch (FormatException formatException)
            {
                log.Debug(formatException);
                log.Error($"The FouFile entry ({string.Join(" ", tokens)}) was not in correct format.");
                return null;
            }

            return fouFileRow;
        }

        private static FouFileRow MakeVelocityMagnitude(IReadOnlyList<string> tokens)
        {
            if (tokens.Count() < 7 || tokens.Count() > 8)
            {
                // invalid number of tokens
                return null;
            }

            string firstToken = tokens[0];
            if (!string.Equals(firstToken, FouFileProperties.VarVelocityMagnitude, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            FouFileRow fouFileRow = null;
            try
            {
                fouFileRow = new FouFileRow();
                fouFileRow.Var = tokens[0];
                fouFileRow.Tsrts = Convert.ToDouble(tokens[1]);
                fouFileRow.Sstop = Convert.ToDouble(tokens[2]);
                fouFileRow.Numcyc = Convert.ToInt32(tokens[3]);
                fouFileRow.Knfac = Convert.ToInt32(tokens[4]);
                fouFileRow.V0plu = Convert.ToInt32(tokens[5]);
                fouFileRow.Layno = tokens[6].GetValueOrNull<int>();
                if (tokens.Count() == 8)
                {
                    fouFileRow.Elp = tokens[7];
                }
                else
                {
                    fouFileRow.Elp = "";
                }
            }
            catch (FormatException formatException)
            {
                log.Debug(formatException);
                log.Error($"The FouFile entry ({string.Join(" ", tokens)}) was not in correct format.");
                return null;
            }

            return fouFileRow;
        }
        
        private static FouFileRow MakeFreeboard(IReadOnlyList<string> tokens)
        {
            if (tokens.Count() < 6 || tokens.Count() > 7)
            {
                // invalid number of tokens
                return null;
            }

            string firstToken = tokens[0];
            if (!string.Equals(firstToken, FouFileProperties.VarFreeboard, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            FouFileRow fouFileRow = null;
            try
            {
                fouFileRow = new FouFileRow();
                fouFileRow.Var = tokens[0];
                fouFileRow.Tsrts = Convert.ToDouble(tokens[1]);
                fouFileRow.Sstop = Convert.ToDouble(tokens[2]);
                fouFileRow.Numcyc = Convert.ToInt32(tokens[3]);
                fouFileRow.Knfac = Convert.ToInt32(tokens[4]);
                fouFileRow.V0plu = Convert.ToInt32(tokens[5]);
                fouFileRow.Layno = null;
                if (tokens.Count() == 7)
                {
                    fouFileRow.Elp = tokens[6];
                }
                else
                {
                    fouFileRow.Elp = "";
                }
            }
            catch (FormatException formatException)
            {
                log.Debug(formatException);
                log.Error($"The FouFile entry ({string.Join(" ", tokens)}) was not in correct format.");
                return null;
            }

            return fouFileRow;
        }
        
        private static FouFileRow MakeWaterDepthOnGround(IReadOnlyList<string> tokens)
        {
            if (tokens.Count() < 6 || tokens.Count() > 7)
            {
                // invalid number of tokens
                return null;
            }

            string firstToken = tokens[0];
            if (!string.Equals(firstToken, FouFileProperties.VarWaterDepthOnGround, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            FouFileRow fouFileRow = null;
            try
            {
                fouFileRow = new FouFileRow();
                fouFileRow.Var = tokens[0];
                fouFileRow.Tsrts = Convert.ToDouble(tokens[1]);
                fouFileRow.Sstop = Convert.ToDouble(tokens[2]);
                fouFileRow.Numcyc = Convert.ToInt32(tokens[3]);
                fouFileRow.Knfac = Convert.ToInt32(tokens[4]);
                fouFileRow.V0plu = Convert.ToInt32(tokens[5]);
                fouFileRow.Layno = null;
                if (tokens.Count() == 7)
                {
                    fouFileRow.Elp = tokens[6];
                }
                else
                {
                    fouFileRow.Elp = "";
                }
            }
            catch (FormatException formatException)
            {
                log.Debug(formatException);
                log.Error($"The FouFile entry ({string.Join(" ", tokens)}) was not in correct format.");
                return null;
            }

            return fouFileRow;
        }
        
        private static FouFileRow MakeWaterVolumeOnGround(IReadOnlyList<string> tokens)
        {
            if (tokens.Count() < 6 || tokens.Count() > 7)
            {
                // invalid number of tokens
                return null;
            }

            string firstToken = tokens[0];
            if (!string.Equals(firstToken, FouFileProperties.VarWaterVolumeOnGround, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            FouFileRow fouFileRow = null;
            try
            {
                fouFileRow = new FouFileRow();
                fouFileRow.Var = tokens[0];
                fouFileRow.Tsrts = Convert.ToDouble(tokens[1]);
                fouFileRow.Sstop = Convert.ToDouble(tokens[2]);
                fouFileRow.Numcyc = Convert.ToInt32(tokens[3]);
                fouFileRow.Knfac = Convert.ToInt32(tokens[4]);
                fouFileRow.V0plu = Convert.ToInt32(tokens[5]);
                fouFileRow.Layno = null;
                if (tokens.Count() == 7)
                {
                    fouFileRow.Elp = tokens[6];
                }
                else
                {
                    fouFileRow.Elp = "";
                }
            }
            catch (FormatException formatException)
            {
                log.Debug(formatException);
                log.Error($"The FouFile entry ({string.Join(" ", tokens)}) was not in correct format.");
                return null;
            }

            return fouFileRow;
        }
        
        private static T? GetValueOrNull<T>(this string valueAsString)
            where T : struct 
        {
            if (string.IsNullOrEmpty(valueAsString))
                return null;
            return (T) Convert.ChangeType(valueAsString, typeof(T));
        }

        private static StreamReader OpenFile(string targetMduFilePath)
        {
            string fouFilePath = IoHelper.GetFilePathToLocationInSameDirectory(targetMduFilePath, FouFileProperties.FouFileName);
            if (!File.Exists(fouFilePath))
            {
                log.Error("Failed to open FouFile, File disappeared during processing");
                return null;
            }

            var sr = new StreamReader(fouFilePath);

            return sr;
        }
    }
}