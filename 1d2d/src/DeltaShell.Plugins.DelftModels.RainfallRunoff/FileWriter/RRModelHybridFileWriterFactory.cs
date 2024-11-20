namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter
{
    public static class RRModelHybridFileWriterFactory
    {
        public static IRRModelHybridFileWriter GetWriter()
        {
            return new RRModelHybridFileWriter();
        }
    }
}