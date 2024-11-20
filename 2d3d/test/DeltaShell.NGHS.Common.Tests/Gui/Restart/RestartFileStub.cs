using DeltaShell.NGHS.Common.Restart;

namespace DeltaShell.NGHS.Common.Tests.Gui.Restart
{
    /// <summary>
    /// This class offers a stub implementation of IRestartFile
    /// </summary>
    public class RestartFileStub : IRestartFile
    {
        public RestartFileStub()
        {
            Name = null;
        }

        public RestartFileStub(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public bool IsEmpty => string.IsNullOrEmpty(Name);
    }
}