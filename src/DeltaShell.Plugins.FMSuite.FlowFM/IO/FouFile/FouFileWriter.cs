using System;
using System.IO;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    public static class FouFileWriter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FouFileWriter));
        public static bool UseFouFile(WaterFlowFMModelDefinition modelDefinition)
        {
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));

            WaterFlowFMProperty shouldWriteFouFile = modelDefinition.GetModelProperty(FouFileProperties.WriteFouFile);
            var writeFile = (bool) shouldWriteFouFile.Value;
            if (!writeFile)
            {
                WaterFlowFMProperty fouUpdateStep = modelDefinition.GetModelProperty(FouFileProperties.MduFouUpdateStep);
                fouUpdateStep.Value = Enum.Parse(fouUpdateStep.PropertyDefinition.DataType, "0");
            }
            else
            {
                WaterFlowFMProperty fouFileName = modelDefinition.GetModelProperty(FouFileProperties.MduFouFileProperty);
                fouFileName.Value = FouFileProperties.FouFileName;
            }
            return writeFile;
        }

        public static void Process(string targetDir, WaterFlowFMModelDefinition modelDefinition)
        {
            Ensure.NotNullOrEmpty(targetDir, nameof(targetDir));
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));

            WriteFouFileData(targetDir, modelDefinition);
        }

        private static void WriteFouFileData(string targetDir, WaterFlowFMModelDefinition modelDefinition)
        {
            using (StreamWriter fouFile = CreateFile(targetDir))
            {
                if (fouFile == null)
                {
                    log.Error($"Could not create a FouFile.");
                    return;
                }
                
                WriteRow(fouFile, new FouFileHeader());

                var mduStartTime = (double) modelDefinition.GetModelProperty(KnownProperties.TStart).Value;
                var mduStopTime = (double) modelDefinition.GetModelProperty(KnownProperties.TStop).Value;
                var rowFactory = new RowFactory(mduStartTime, mduStopTime);

                WaterFlowFMProperty writeWlAverage = modelDefinition.GetModelProperty( FouFileProperties.GuiOnlyWriteWlAverage);
                if ((bool) writeWlAverage.Value)
                {
                    WriteRow(fouFile, rowFactory.WaterLevelRow(FouFileProperties.ElpAverage));
                }

                WaterFlowFMProperty writeWlMaximum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWlMaximum);
                if ((bool) writeWlMaximum.Value)
                {
                    WriteRow(fouFile, rowFactory.WaterLevelRow(FouFileProperties.ElpMaximum));
                }

                WaterFlowFMProperty writeWlMinimum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWlMinimum);
                if ((bool) writeWlMinimum.Value)
                {
                    WriteRow(fouFile, rowFactory.WaterLevelRow(FouFileProperties.ElpMinimum));
                }

                WaterFlowFMProperty writeUcAverage = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteUcAverage);
                if ((bool) writeUcAverage.Value)
                {
                    WriteRow(fouFile, rowFactory.VelocityMagnitudeRow(FouFileProperties.ElpAverage));
                }

                WaterFlowFMProperty writeUcMaximum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteUcMaximum);
                if ((bool) writeUcMaximum.Value)
                {
                    WriteRow(fouFile, rowFactory.VelocityMagnitudeRow(FouFileProperties.ElpMaximum));
                }

                WaterFlowFMProperty writeUcMinimum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteUcMinimum);
                if ((bool) writeUcMinimum.Value)
                {
                    WriteRow(fouFile, rowFactory.VelocityMagnitudeRow(FouFileProperties.ElpMinimum));
                }
            }
        }

        private static void WriteRow(TextWriter sw, FouFileRow row)
        {
            sw.Write(row.Var);
            sw.Write(FouFileProperties.FouFileDelimiter);

            sw.Write(row.Tsrts);
            sw.Write(FouFileProperties.FouFileDelimiter);

            sw.Write(row.Sstop);
            sw.Write(FouFileProperties.FouFileDelimiter);

            sw.Write(row.Numcyc);
            sw.Write(FouFileProperties.FouFileDelimiter);

            sw.Write(row.Knfac);
            sw.Write(FouFileProperties.FouFileDelimiter);

            sw.Write(row.V0plu);
            sw.Write(FouFileProperties.FouFileDelimiter);

            if (row.Layno == null)
            {
                sw.Write(" ");
            }
            else
            {
                sw.Write(row.Layno);
            }
            sw.Write(FouFileProperties.FouFileDelimiter);

            sw.Write(row.Elp);
            
            sw.WriteLine();
        }

        private static void WriteRow(StreamWriter sw, FouFileHeader row)
        {
            foreach (string rowHeader in row.Headers)
            {
                sw.Write(rowHeader);
                sw.Write(FouFileProperties.FouFileDelimiter);
            }

            sw.WriteLine();
        }

        private static StreamWriter CreateFile(string targetMduFilePath)
        {
            string nodeFilePath = Path.Combine(targetMduFilePath, FouFileProperties.FouFileName);
            FileUtils.DeleteIfExists(nodeFilePath);

            return new StreamWriter(nodeFilePath);
        }
    }
}