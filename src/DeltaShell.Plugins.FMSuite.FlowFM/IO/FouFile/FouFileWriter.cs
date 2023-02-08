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
            if (writeFile)
            {
                WaterFlowFMProperty fouFileName = modelDefinition.GetModelProperty(FouFileProperties.MduFouFileProperty);
                fouFileName.Value = FouFileProperties.FouFileName;
            }
            else
            {
                WaterFlowFMProperty fouUpdateStep = modelDefinition.GetModelProperty(FouFileProperties.MduFouUpdateStep);
                fouUpdateStep.Value = Enum.Parse(fouUpdateStep.PropertyDefinition.DataType, "0");
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
                
                WriteHeader(fouFile, new FouFileHeader());

                var mduStartTime = (double) modelDefinition.GetModelProperty(KnownProperties.TStart).Value;
                var mduStopTime = (double) modelDefinition.GetModelProperty(KnownProperties.TStop).Value;
                var rowFactory = new RowFactory(mduStartTime, mduStopTime);

                WriteWlProperty(fouFile, rowFactory, modelDefinition);
                WriteUcProperty(fouFile, rowFactory, modelDefinition);
                WriteFbProperty(fouFile, rowFactory, modelDefinition);
                WriteWdogProperty(fouFile, rowFactory, modelDefinition);
                WriteVogProperty(fouFile, rowFactory, modelDefinition);
            }
        }

        private static void WriteWlProperty(StreamWriter sw, RowFactory rowFactory, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty writeWlAverage = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWlAverage);
            if ((bool) writeWlAverage.Value)
            {
                WriteRow(sw, rowFactory.WaterLevelRow(FouFileProperties.ElpAverage));
            }

            WaterFlowFMProperty writeWlMaximum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWlMaximum);
            if ((bool) writeWlMaximum.Value)
            {
                WriteRow(sw, rowFactory.WaterLevelRow(FouFileProperties.ElpMaximum));
            }

            WaterFlowFMProperty writeWlMinimum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWlMinimum);
            if ((bool) writeWlMinimum.Value)
            {
                WriteRow(sw, rowFactory.WaterLevelRow(FouFileProperties.ElpMinimum));
            }
        }

        private static void WriteUcProperty(StreamWriter sw, RowFactory rowFactory, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty writeUcAverage = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteUcAverage);
            if ((bool) writeUcAverage.Value)
            {
                WriteRow(sw, rowFactory.VelocityMagnitudeRow(FouFileProperties.ElpAverage));
            }

            WaterFlowFMProperty writeUcMaximum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteUcMaximum);
            if ((bool) writeUcMaximum.Value)
            {
                WriteRow(sw, rowFactory.VelocityMagnitudeRow(FouFileProperties.ElpMaximum));
            }

            WaterFlowFMProperty writeUcMinimum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteUcMinimum);
            if ((bool) writeUcMinimum.Value)
            {
                WriteRow(sw, rowFactory.VelocityMagnitudeRow(FouFileProperties.ElpMinimum));
            }
        }

        private static void WriteFbProperty(StreamWriter sw, RowFactory rowFactory, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty writeFbAverage = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteFbAverage);
            if ((bool) writeFbAverage.Value)
            {
                WriteRow(sw, rowFactory.Freeboard(FouFileProperties.ElpAverage));
            }

            WaterFlowFMProperty writeFbMaximum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteFbMaximum);
            if ((bool) writeFbMaximum.Value)
            {
                WriteRow(sw, rowFactory.Freeboard(FouFileProperties.ElpMaximum));
            }

            WaterFlowFMProperty writeFbMinimum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteFbMinimum);
            if ((bool) writeFbMinimum.Value)
            {
                WriteRow(sw, rowFactory.Freeboard(FouFileProperties.ElpMinimum));
            }
        }

        private static void WriteWdogProperty(StreamWriter sw, RowFactory rowFactory, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty writeWdogAverage = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWdogAverage);
            if ((bool) writeWdogAverage.Value)
            {
                WriteRow(sw, rowFactory.WaterDepthOnGround(FouFileProperties.ElpAverage));
            }

            WaterFlowFMProperty writeWdogMaximum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWdogMaximum);
            if ((bool) writeWdogMaximum.Value)
            {
                WriteRow(sw, rowFactory.WaterDepthOnGround(FouFileProperties.ElpMaximum));
            }

            WaterFlowFMProperty writeWdogMinimum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteWdogMinimum);
            if ((bool) writeWdogMinimum.Value)
            {
                WriteRow(sw, rowFactory.WaterDepthOnGround(FouFileProperties.ElpMinimum));
            }
        }

        private static void WriteVogProperty(StreamWriter sw, RowFactory rowFactory, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty writeVogAverage = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteVogAverage);
            if ((bool) writeVogAverage.Value)
            {
                WriteRow(sw, rowFactory.VolumeOnGround(FouFileProperties.ElpAverage));
            }

            WaterFlowFMProperty writeVogMaximum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteVogMaximum);
            if ((bool) writeVogMaximum.Value)
            {
                WriteRow(sw, rowFactory.VolumeOnGround(FouFileProperties.ElpMaximum));
            }

            WaterFlowFMProperty writeVogMinimum = modelDefinition.GetModelProperty(FouFileProperties.GuiOnlyWriteVogMinimum);
            if ((bool) writeVogMinimum.Value)
            {
                WriteRow(sw, rowFactory.VolumeOnGround(FouFileProperties.ElpMinimum));
            }
        }
        
        private static void WriteRow(StreamWriter sw, FouFileRow row)
        {
            sw.Write($"{row.Var, -FouFileProperties.ColumnWidth}");
            sw.Write($"{row.Tsrts, -FouFileProperties.ColumnWidth}");
            sw.Write($"{row.Sstop, -FouFileProperties.ColumnWidth}");
            sw.Write($"{row.Numcyc, -FouFileProperties.ColumnWidth}");
            sw.Write($"{row.Knfac, -FouFileProperties.ColumnWidth}");
            sw.Write($"{row.V0plu, -FouFileProperties.ColumnWidth}");
            sw.Write($"{(row.Layno != null ? row.Layno.ToString() : string.Empty), -FouFileProperties.ColumnWidth}");
            sw.Write(row.Elp);
            sw.WriteLine();
        }

        private static void WriteHeader(StreamWriter sw, FouFileHeader row)
        {
            foreach (string rowHeader in row.Headers)
            {
                sw.Write($"{rowHeader, -FouFileProperties.ColumnWidth}");
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