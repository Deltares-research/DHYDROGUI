namespace DeltaShell.NGHS.Common
{
    /// <summary>
    /// Interface for methods specific for coupled models
    /// </summary>
    public interface ICoupledModel
    {
        void CleanUpModelAfterModelCoupling();
    }
}