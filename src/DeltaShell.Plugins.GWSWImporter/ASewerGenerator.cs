using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public abstract class ASewerGenerator
    {
        protected readonly ILogHandler logHandler;
        protected ASewerGenerator(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }
    }
}