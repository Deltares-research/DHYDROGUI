namespace DeltaShell.NGHS.IO.Helpers
{
    public class DelftIniProperty : IDelftIniProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Comment { get; set; }

        /// <summary>
        /// The line where this property was read in the file.
        /// </summary>
        public int LineNumber { get; set; }

        public DelftIniProperty()
        {
            
        }

        public DelftIniProperty(string name, string value, string comment)
        {
            Name = name;
            Value = value;
            Comment = comment;
        }

        public override string ToString()
        {
            return $"Line {LineNumber}: {Name}={Value}";
        }
    }
}