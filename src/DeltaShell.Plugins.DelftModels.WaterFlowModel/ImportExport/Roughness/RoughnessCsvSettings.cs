using System.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    /// <summary>
    /// Just a record with the defintions for the CSV; no need to make things extra verbose with setters.
    /// </summary>
    public class RoughnessCSvSettings : List<CsvColumn>
    {
        public RoughnessCSvSettings()
        {
            Add(new CsvColumn(BranchNameIndex, BranchNameHeaderText, typeof (string)));
            Add(new CsvColumn(ChainageIndex, ChainageHeaderText, typeof (double)));
            Add(new CsvColumn(RoughnessTypeIndex, RoughnessTypeHeaderText, typeof (string))); // RoughnessType
            Add(new CsvColumn(SectionTypeIndex, SectionTypeHeaderText, typeof (string)));
            Add(new CsvColumn(RoughnessFunctionIndex, RoughnessFunctionHeaderText, typeof (string)));// RoughnessFunction
            Add(new CsvColumn(InterpolationTypeIndex, InterpolationTypeHeaderText, typeof (string)));// InterpolationType
            Add(new CsvColumn(NegativeIsPositiveIndex, NegativeIsPositiveHeaderText, typeof (string)));// Same/Different, bool
            Add(new CsvColumn(PositiveConstantIndex, PositiveConstantHeaderText, typeof (double)));
            Add(new CsvColumn(PositiveQIndex, PositiveQHeaderText, typeof (double)));
            Add(new CsvColumn(PositiveQRoughnessIndex, PositiveQRoughnessHeaderText, typeof (double)));
            Add(new CsvColumn(PositiveHIndex, PositiveHHeaderText, typeof (double)));
            Add(new CsvColumn(PositiveHRoughnessIndex, PositiveHRoughnessHeaderText, typeof (double)));
            Add(new CsvColumn(NegativeConstantIndex, NegativeConstantHeaderText, typeof (double)));
            Add(new CsvColumn(NegativeQIndex, NegativeQHeaderText, typeof (double)));
            Add(new CsvColumn(NegativeQRoughnessIndex, NegativeQRoughnessHeaderText, typeof (double)));
            Add(new CsvColumn(NegativeHIndex, NegativeHHeaderText, typeof (double)));
            Add(new CsvColumn(NegativeHRoughnessIndex, NegativeHRoughnessHeaderText, typeof (double)));
        }

        public const string BranchNameHeaderText = "Name";
        public const string ChainageHeaderText = "Chainage";
        public const string RoughnessTypeHeaderText = "RoughnessType";
        public const string SectionTypeHeaderText = "SectionType";
        public const string RoughnessFunctionHeaderText = "Dependance";
        public const string InterpolationTypeHeaderText = "Interpolation";
        public const string NegativeIsPositiveHeaderText = "Pos/neg";
        public const string PositiveConstantHeaderText = "R_pos_constant";
        public const string PositiveQHeaderText = "Q_pos";
        public const string PositiveQRoughnessHeaderText = "R_pos_f(Q)";
        public const string PositiveHHeaderText = "H_pos";
        public const string PositiveHRoughnessHeaderText = "R_pos__f(h)";
        public const string NegativeConstantHeaderText = "R_neg_constant";
        public const string NegativeQHeaderText = "Q_neg";
        public const string NegativeQRoughnessHeaderText = "R_neg_f(Q)";
        public const string NegativeHHeaderText = "H_neg";
        public const string NegativeHRoughnessHeaderText = "R_neg_f(h)";

        public int BranchNameIndex = 0;
        public int ChainageIndex = 1;
        public int RoughnessTypeIndex = 2;
        public int SectionTypeIndex = 3;
        public int RoughnessFunctionIndex = 4;
        public int InterpolationTypeIndex = 5;
        public int NegativeIsPositiveIndex = 6;
        public int PositiveConstantIndex = 7;
        public int PositiveQIndex = 8;
        public int PositiveQRoughnessIndex = 9;
        public int PositiveHIndex = 10;
        public int PositiveHRoughnessIndex = 11;
        public int NegativeConstantIndex = 12;
        public int NegativeQIndex = 13;
        public int NegativeQRoughnessIndex = 14;
        public int NegativeHIndex = 15;
        public int NegativeHRoughnessIndex = 16;
    }
}