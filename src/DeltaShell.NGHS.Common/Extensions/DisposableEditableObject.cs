using DelftTools.Utils.Editing;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Utils;

namespace DeltaShell.NGHS.Common.Extensions
{
    public sealed class DisposableEditableObject : DisposableObjectWrapper<IEditableObject>
    {
        /// <summary>
        /// Sets an editableObject in edit mode
        /// </summary>
        /// <param name="editableObject">Editable object</param>
        /// <param name="actionName">Name of the action</param>
        public DisposableEditableObject(IEditableObject editableObject, string actionName = ""): base(()=> editableObject)
        {
            Ensure.NotNull(editableObject, nameof(editableObject));

            editableObject.BeginEdit(actionName);
            disposeAction = o => o.EndEdit();
        }
    }
}