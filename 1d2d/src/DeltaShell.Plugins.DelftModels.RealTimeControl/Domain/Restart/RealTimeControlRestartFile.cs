using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart
{
    /// <summary>
    /// A restart file used in the RTC model.
    /// </summary>
    [Entity]
    public class RealTimeControlRestartFile : Unique<long>
    {
        private string name;

        /// <summary>
        /// Creates a new instance of <see cref="RealTimeControlRestartFile"/>.
        /// </summary>
        public RealTimeControlRestartFile() : this(string.Empty, null) {}

        /// <summary>
        /// Creates a new instance of <see cref="RealTimeControlRestartFile"/>.
        /// </summary>
        /// <param name="name">The name of the restart file.</param>
        /// <param name="content">The content of the restart file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c>.</exception>
        public RealTimeControlRestartFile(string name, string content)
        {
            Ensure.NotNull(name, nameof(name));

            Name = name;
            Content = content;
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when set <paramref name="value"/> is <c>null</c>.</exception>
        public string Name
        {
            get => name;
            set
            {
                Ensure.NotNull(value, nameof(value));

                name = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        public bool IsEmpty => Content == null;

        /// <summary>
        /// Gets the content of the restart file
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A new copied instance of this instance.</returns>
        public RealTimeControlRestartFile Clone()
        {
            return new RealTimeControlRestartFile(Name, Content);
        }
    }
}