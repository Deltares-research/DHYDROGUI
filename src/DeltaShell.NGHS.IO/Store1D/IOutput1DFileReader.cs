using System.Collections.Generic;

namespace DeltaShell.NGHS.IO.Store1D
{
    public interface IOutput1DFileReader<T, U> where T : ILocationMetaData, new()  where U : ITimeDependentVariableMetaDataBase, new()
    {
        OutputFile1DMetaData<T, U> ReadMetaData(string path, bool doValidation = true);
        double[,] GetAllVariableData(string path, string variableName, OutputFile1DMetaData<T, U> metaData);
        IList<double> GetSelectionOfVariableData(string path, string variableName, int[] origin, int[] shape);
    }
}