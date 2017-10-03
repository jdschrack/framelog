using FrameLog.Contexts;
using FrameLog.Exceptions;
using FrameLog.Filter;
using FrameLog.Logging;
using FrameLog.Models;
using FrameLog.Transactions;
using System;
using System.Data.Entity.Core.Objects;
using System.Threading;
using System.Transactions;
using FrameLog.Translation;
using FrameLog.Translation.Serializers;

namespace FrameLog
{
    public partial class FrameLogModule<TChangeSet, TPrincipal>
        where TChangeSet : IChangeSet<TPrincipal>
    {
        public bool Enabled { get; set; }
        private IChangeSetFactory<TChangeSet, TPrincipal> factory;
        private IFrameLogContext<TChangeSet, TPrincipal> context;        
        private ILoggingFilter filter;
        private ISerializationManager serializer;

        public FrameLogModule(IChangeSetFactory<TChangeSet, TPrincipal> factory,
            IFrameLogContext<TChangeSet, TPrincipal> context,
            ILoggingFilterProvider filter = null,
            ISerializationManager serializer = null)
        {
            this.factory = factory;
            this.context = context;
            this.filter = (filter ?? Filters.Default).Get(context);
            this.serializer = (serializer ?? new ValueTranslationManager(context));
            Enabled = true;
        }

        /// <summary>
        /// Save the changes and log them as controlled by the logging filter. 
        /// A TransactionScope will be used to wrap the save, which will use an ambient 
        /// transaction if available, or create a new one otherwise.
        ///  
        /// If you are using an explicit transaction, and not using the TransactionScope
        /// API, then do not use this overload. Use SaveChangesWithinExplicitTransaction.
        /// </summary>
        public ISaveResult<TChangeSet> SaveChanges(TPrincipal principal)
        {
            return SaveChanges(principal, new TransactionOptions());
        }        
        /// <summary>
        /// Save the changes and log them as controlled by the logging filter. 
        /// A TransactionScope will be used to wrap the save, which will use an ambient 
        /// transaction available, or create a new one otherwise. The given options
        /// will be used in constructing the TransactionScope.
        ///  
        /// If you are using an explicit transaction, and not using the TransactionScope
        /// API, then do not use this overload. Use SaveChangesWithinExplicitTransaction.
        /// </summary>
        public ISaveResult<TChangeSet> SaveChanges(TPrincipal principal, TransactionOptions transactionOptions)
        {
            return saveChanges(principal, new TransactionScopeProvider(transactionOptions));
        }
        /// <summary>
        /// Save the changes and log them as controlled by the logging filter. 
        /// Warning: Only use this overload when you are wrapping the call to FrameLog in your 
        /// own transaction. This prevents FrameLog from automatically creating its own transaction.
        /// 
        /// If you are using the TransactionScope API, you can use the SaveChanges overload,
        /// as FrameLog will automatically detect the ambient transaction.
        /// </summary>
        public ISaveResult<TChangeSet> SaveChangesWithinExplicitTransaction(TPrincipal principal)
        {
            // If there is already an explicit transaction in use, we don't need to do anything
            // with transactions in FrameLog, so just use the NullTransactionProvider
            return saveChanges(principal, new NullTransactionProvider());
        }

        protected ISaveResult<TChangeSet> saveChanges(TPrincipal principal, ITransactionProvider transactionProvider)
        {
            if (!Enabled)
                return new SaveResult<TChangeSet, TPrincipal>(context.SaveAndAcceptChanges());

            var result = new SaveResult<TChangeSet,TPrincipal>();
            // We want to split saving and logging into two steps, so that when we
            // generate the log objects the database has already assigned IDs to new
            // objects. Then we can log about them meaningfully. So we wrap it in a
            // transaction so that even though there are two saves, the change is still
            // atomic.
            transactionProvider.InTransaction(() =>
            {
                var logger = new ChangeLogger<TChangeSet, TPrincipal>(context, factory, filter, serializer);
                var oven = (IOven<TChangeSet, TPrincipal>) null;

                // First we detect all the changes, but we do not save or accept the changes 
                // (i.e. we keep our record of them).
                context.DetectChanges();

                // Then we save and accept the changes, which invokes the standard EntityFramework
                // DbContext.SaveChanges(), including any custom user logic the end-user has defined. 
                // Eventually, DbContext.InternalContext.ObjectContext.SaveChanges() will be invoked
                // and then the delegate below is called back to prepare the log objects/changes.
                result.AffectedObjectCount = context.SaveAndAcceptChanges((sender, args) =>
                {
                    // This is invoked just moments before EntityFramework accepts the original changes.
                    // Now is our best oppertunity to create the log objects, which will not yet be attached 
                    // to the context. They are unattached so that the context change tracker won't noticed 
                    // them when accepting the original changes.
                    oven = logger.Log(context.ObjectStateManager);                
                });

                // If the oven is not set here, then DbContext.SaveChanges() did not call our delegate back
                // when accepting the original changes. Without the oven, we cannot bake the logged changes.
                if (oven == null)
                    throw new ChangesNotDetectedException();

                // Finally, we attach the previously prepared log objects to the context (and save/accept them)
                if (oven.HasChangeSet)
                {
                    // First do any deferred log value calculations.
                    // (see PropertyChange.Bake for more information)
                    // Then detect all the log changes that were previously deferred
                    result.ChangeSet = oven.Bake(DateTime.Now, principal);
                    context.AddChangeSet(result.ChangeSet);
                    context.DetectChanges();

                    // Then we save and accept the changes that result from creating the log objects
                    // NOTE: We do not use SaveAndAcceptChanges() here because we are not interested in going
                    //       through DbContext.SaveChanges() and invoking end-users custom logic.
                    context.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);
                }
            });
            return result;
        }
    }
}
