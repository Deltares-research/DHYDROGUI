namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter
{
    public enum NodeType
    {
        BoundaryOrLateral,
        Unpaved,
        Paved,
        Greenhouse,
        Openwater,
        Wwtp,
        Sacramento,
        Hbv
    }
    
    public enum SewerType
    {
        Mixed = 0,
        Separated,
        MixedSeparated
    }

    public enum DwfComputationOption
    {
        NumberOfInhabitantsTimesConstantDWF = 1,
        NumberOfInhabitantsTimesVariableDWF,
        ConstantDWF,
        VariableDWF
    }

    public enum LinkType
    {
        Boundary=0,
        WasteWaterTreatmentPlant=2
    }

    public enum DrainageComputationOption
    {
        DeZeeuwHellinga = 1,
        KrayenhoffVdLeur,
        Ernst
    }

    public enum SeepageComputationOption
    {
        Constant = 1,
        VariableWithH0 = 2,
        VariableWithH0FromModflow = 3,
        TimeTable = 4,
        TimeTableAndSaltConcentration = 5
    }
}