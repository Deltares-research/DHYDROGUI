using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DeltaShell.Plugins.FMSuite.Common.Utils
{
    public static class EnumHelper
    {
        public static bool TryParseEnumValueFromDescription<T>(string description, out T enumValue)
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException();
            FieldInfo[] fields = type.GetFields();
            var field = fields
                .SelectMany(f => 
                    f.GetCustomAttributes(
                        typeof(DescriptionAttribute), false), 
                        (f, a) => 
                            new { Field = f, Att = a })
                                .SingleOrDefault(a => 
                                    ((DescriptionAttribute)a.Att).Description == description);
            enumValue = field == null ? default(T) : (T)field.Field.GetRawConstantValue();
            return field != null;
        }
    }
}
