using System;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.Common.Extensions
{
    public sealed class DisposableEditableObject : IDisposable
    {
        private readonly IEditableObject editableObject;

        /// <summary>
        /// Sets an editableObject in edit mode
        /// </summary>
        /// <param name="editableObject">Editable object</param>
        /// <param name="actionName">Name of the action</param>
        public DisposableEditableObject(IEditableObject editableObject, string actionName = "")
        {
            this.editableObject = editableObject;

            Ensure.NotNull(editableObject, nameof(editableObject));
            editableObject.BeginEdit(actionName);
        }

        /// <inheritdoc cref="IDisposable"/>
        /// <summary>
        /// Puts the <see cref="IEditableObject"/> out of edit mode
        /// </summary>
        public void Dispose()
        {
            editableObject.EndEdit();
        }
    }
}