namespace DeltaShell.Sobek.Readers
{
    public static class StringUtils
    {
        public static string RemoveQuotes(string sourceString)
        {
            if (sourceString.StartsWith("'") && sourceString.EndsWith("'"))
            {
                return sourceString.Substring(1, sourceString.Length - 2);
            }
            return sourceString;
        }
    }
}