using System;

namespace DeltaShell.NGHS.Utils
{
    /// <summary>
    /// Disposable object that 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DisposableObjectWrapper<T> : IDisposable
    {
        protected Action<T> disposeAction;

        public DisposableObjectWrapper(Func<T> createObjectFunc, Action<T> disposeAction = null)
        {
            this.disposeAction = disposeAction;
            WrapperObject = createObjectFunc();
        }

        public T WrapperObject { get; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                disposeAction?.Invoke(WrapperObject);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}