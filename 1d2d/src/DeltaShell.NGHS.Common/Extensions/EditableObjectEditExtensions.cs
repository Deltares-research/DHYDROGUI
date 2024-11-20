using DelftTools.Utils.Editing;

namespace DeltaShell.NGHS.Common.Extensions
{
    public static class EditableObjectEditExtensions
    {
        /// <summary>
        /// Creates a wrapper for the <see cref="IEditableObject"/> that starts in edit mode
        /// and ends edit mode when disposing
        /// </summary>
        /// <example>
        /// Use this together with the using statement
        /// <code>
        /// using(editableObject, "Doing action") // Start edit mode
        /// {
        /// 
        /// } // End edit mode
        /// </code> 
        /// </example>
        /// <param name="editableObject">The editable object</param>
        /// <param name="actionName">Name of the action</param>
        /// <returns>A disposable editable object</returns>
        public static DisposableEditableObject InEditMode(this IEditableObject editableObject, string actionName = "")
        {
            return new DisposableEditableObject(editableObject, actionName);
        }
    }
}