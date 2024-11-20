namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public interface INwrwComponentFileWriterBase
    {
        bool Write(string path);
    }
}