namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart
{
    /// <summary>
    /// A restart file used in the RTC model.
    /// </summary>
    public class RealTimeControlRestartFile
    {
        public RealTimeControlRestartFile() : this(string.Empty, string.Empty) {}

        /// <summary>
        /// Creates a new instance of <see cref="RealTimeControlRestartFile"/>.
        /// </summary>
        /// <param name="name">The name of the restart file.</param>
        /// <param name="content">The content of the restart file.</param>
        public RealTimeControlRestartFile(string name, string content)
        {
            Name = name;
            Content = content;
        }

        public string Name { get; }

        public bool IsEmpty => string.IsNullOrEmpty(Content);

        /// <summary>
        /// Gets the content of the restart file
        /// </summary>
        public string Content { get; }

        public RealTimeControlRestartFile Clone()
        {
            return new RealTimeControlRestartFile(Name, Content);
        }
    }
}