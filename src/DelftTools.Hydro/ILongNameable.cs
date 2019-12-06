namespace DelftTools.Hydro
{
    /// <summary>
    /// <see cref="ILongNameable"/> defines the <c>LongName</c> property on a
    /// class.
    /// </summary>
    public interface ILongNameable
    {
        /// <summary>
        /// Gets or sets the long name.
        /// </summary>
        string LongName { get; set; }
    }
}