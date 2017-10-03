using System;
using System.Data.Entity;
using FrameLog.Helpers;
using FrameLog.Models;

namespace FrameLog.Contexts
{
    public abstract partial class DbContextAdapter<TChangeSet, TPrincipal> 
        : ObjectContextAdapter<TChangeSet, TPrincipal>
        where TChangeSet : IChangeSet<TPrincipal>
    {
        private readonly DbContext context;

        public DbContextAdapter(DbContext context)
            : base(((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext)
        {
            this.context = context;
        }

        public override int SaveAndAcceptChanges(EventHandler onSavingChanges = null)
        {
            // Wrap the save operation inside a disposable listener for the ObjectContext.SaveChanges event
            // By doing this, the event handler will be invoked after saving but BEFORE accepting the changes.
            using (new DisposableSavingChangesListener(context, onSavingChanges))
            {
                return context.SaveChanges();
            }
        }
    }
}
