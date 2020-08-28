using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart
{
    /// <summary>
    /// A restart file used in the RTC model.
    /// </summary>
    [Entity]
    public class RealTimeControlRestartFile : Unique<long>
    {
        /// <summary>
        /// Creates a new instance of <see cref="RealTimeControlRestartFile"/>.
        /// </summary>
        public RealTimeControlRestartFile() : this(string.Empty, string.Empty) {}

        /// <summary>
        /// Creates a new instance of <see cref="RealTimeControlRestartFile"/>.
        /// </summary>
        /// <param name="name">The name of the restart file.</param>
        /// <param name="content">The content of the restart file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any parameter is <c>null</c>.</exception>
        public RealTimeControlRestartFile(string name, string content)
        {
            Ensure.NotNull(name, nameof(name));
            Ensure.NotNull(content, nameof(content));

            Name = name;
            Content = content;
        }

        public string Name { get; set; }

        public bool IsEmpty => string.IsNullOrEmpty(Content);

        /// <summary>
        /// Gets the content of the restart file
        /// </summary>
        public string Content { get; set; }

        public RealTimeControlRestartFile Clone()
        {
            return new RealTimeControlRestartFile(Name, Content);
        }
    }
}