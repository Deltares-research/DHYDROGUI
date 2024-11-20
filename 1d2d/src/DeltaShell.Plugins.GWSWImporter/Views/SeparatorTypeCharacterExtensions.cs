using System;

namespace DeltaShell.Plugins.ImportExport.GWSW.Views
{
    public static class SeparatorTypeCharacterExtensions
    {
        public static char GetChar(this SeparatorType separatorType, char otherChar)
        {
            switch (separatorType)
            {
                case SeparatorType.Tab:
                    return '\t';
                case SeparatorType.Semicolon:
                    return ';';
                case SeparatorType.Comma:
                    return ',';
                case SeparatorType.Space:
                    return ' ';
                case SeparatorType.Other:
                    return otherChar;
                default:
                    throw new ArgumentOutOfRangeException(nameof(separatorType));
            }
        }
    }
}