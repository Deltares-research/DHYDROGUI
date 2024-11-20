namespace DeltaShell.Dimr
{
    public interface IDimrApiFactory
    {
        IDimrApi CreateNew(bool runRemote = true);
    }
}