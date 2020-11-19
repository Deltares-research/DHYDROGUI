namespace DeltaShell.NGHS.Common
{
    /// <summary>
    /// Interface for models that contain linked data items.
    /// These links are assumed to be to another model.
    /// </summary>
    public interface IControllingModel : ICoupledModel
    {
        void CleanUpModelAfterModelCoupling();
    }
}