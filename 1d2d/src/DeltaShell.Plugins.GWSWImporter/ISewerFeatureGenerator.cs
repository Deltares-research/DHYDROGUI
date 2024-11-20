namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public interface IGwswFeatureGenerator<T>
    {
        T Generate(GwswElement gwswElement);
    }
}