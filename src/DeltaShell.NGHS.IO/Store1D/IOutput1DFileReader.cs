using System.Collections.Generic;

namespace DeltaShell.NGHS.IO.Store1D
{
    public interface IOutput1DFileReader<T> where T : TimeDependentVariableMetaDataBase, new()
    {
        OutputFile1DMetaData<LocationMetaData, T> ReadMetaData(string path, bool doValidation = true);
        double[,] GetAllVariableData(string path, string variableName, OutputFile1DMetaData<LocationMetaData, T> metaData);
        IList<double> GetSelectionOfVariableData(string path, string variableName, int[] origin, int[] shape);
    }
}