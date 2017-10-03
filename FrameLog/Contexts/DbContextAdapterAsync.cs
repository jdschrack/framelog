using System;
using System.Threading;
using System.Threading.Tasks;
using FrameLog.Helpers;
using FrameLog.Models;

namespace FrameLog.Contexts
{
    public abstract partial class DbContextAdapter<TChangeSet, TPrincipal>
        where TChangeSet : IChangeSet<TPrincipal>
    {
        public async override Task<int> SaveAndAcceptChangesAsync(EventHandler onSavingChanges = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Wrap the save operation inside a disposable listener for the ObjectContext.SaveChanges event
            // By doing this, the event handler will be invoked after saving but BEFORE accepting the changes.
            using (new DisposableSavingChangesListener(context, onSavingChanges))
            {
                return await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
