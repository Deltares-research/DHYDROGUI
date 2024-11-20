using System;
using log4net;

namespace DeltaShell.NGHS.Utils
{
    public static class ObjectExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ObjectExtensions));

        /// <summary>
        /// Executes the given <paramref name="action"/> with the property <paramref name="propertyName"/> temporarily set 
        /// to <paramref name="value"/>. The state is restored after the action has exited (also if an error occurs)
        /// </summary>
        /// <typeparam name="TObject">Type of the object that has the property</typeparam>
        /// <typeparam name="TProperty">Type of the property</typeparam>
        /// <param name="objectToSet">Instance of the object to set</param>
        /// <param name="propertyName">Name of the property to set</param>
        /// <param name="value">Temporary value the property has to have</param>
        /// <param name="action">Action to execute</param>
        public static void DoWithPropertySet<TObject, TProperty>(this TObject objectToSet, string propertyName, TProperty value, Action action)
        {
            if (objectToSet == null)
            {
                log.Warn($"Could not set propery {propertyName} because object is not set");
                action?.Invoke();
                return;
            }

            var propertyInfo = objectToSet?.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(propertyName, $"Could not find property {propertyName} on {objectToSet?.GetType()}");
            }

            if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
            {
                throw new ArgumentNullException(propertyName, $"Could not read/write property {propertyName} on {objectToSet.GetType()}");
            }

            var currentValue = (TProperty) propertyInfo.GetValue(objectToSet);
            try
            {
                propertyInfo.SetMethod.Invoke(objectToSet, new object[]{ value });
                action?.Invoke();
            }
            finally
            {
                propertyInfo.SetMethod.Invoke(objectToSet, new object[] { currentValue });
            }

        }
    }
}
