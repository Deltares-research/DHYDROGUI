using System;
using BasicModelInterface;
using ProtoBufRemote;

namespace DeltaShell.Dimr
{
    public class LoggerToProtoConverter : ITypeToProtoConverter
    {
        public object ToProtoObject(object original)
        {
            return string.Empty;
        }

        public object FromProtoObject(object protoObject)
        {
            return new Logger((level, message) => {});
        }

        public Type GetProtoType()
        {
            return typeof(string);
        }

        public Type GetSourceType()
        {
            return typeof(Logger);
        }
    }
}