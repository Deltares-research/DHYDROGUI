using System.Collections.Generic;

namespace DeltaShell.NGHS.IO.Store1D
{
    public interface IOutput1DFileReader<U> where U : ITimeDependentVariableMetaDataBase, new()
    {
        OutputFile1DMetaData<U> ReadMetaData(string path, bool doValidation = true);
        double[,] GetAllVariableData(string path, string variableName, OutputFile1DMetaData<U> metaData);
        IList<double> GetSelectionOfVariableData(string path, string variableName, int[] origin, int[] shape);
    }
}