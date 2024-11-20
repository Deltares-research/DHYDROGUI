using System;
using DeltaShell.NGHS.IO.FileReaders;

namespace DeltaShell.NGHS.IO.Helpers
{
    public class PropertyNotFoundInFileException : FileReadingException
    {
        public PropertyNotFoundInFileException(string message)
            : base(message)
        {
        }

        public PropertyNotFoundInFileException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}