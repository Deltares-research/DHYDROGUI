namespace DeltaShell.Plugins.ImportExport.Gwsw
{
    public interface IGwswFeatureGenerator<T>
    {
        T Generate(GwswElement gwswElement);
    }
}