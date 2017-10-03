using System;
using FrameLog.Helpers;
using FrameLog.Models;
using System.Data.Entity.Core.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace FrameLog.Contexts
{
    public abstract partial class ObjectContextAdapter<TChangeSet, TPrincipal> 
        where TChangeSet : IChangeSet<TPrincipal>
    {
        public async virtual Task<int> SaveChangesAsync(SaveOptions saveOptions)
        {
            return await context.SaveChangesAsync(saveOptions);
        }

        public async virtual Task<int> SaveAndAcceptChangesAsync(EventHandler onSavingChanges = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Wrap the save operation inside a disposable listener for the ObjectContext.SaveChanges event
            // By doing this, the event handler will be invoked after saving but BEFORE accepting the changes.
            using (new DisposableSavingChangesListener(context, onSavingChanges))
            {
                return await context.SaveChangesAsync(SaveOptions.AcceptAllChangesAfterSave, cancellationToken);
            }
        }
    }
}
