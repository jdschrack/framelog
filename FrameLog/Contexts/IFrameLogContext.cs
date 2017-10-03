using FrameLog.Models;
using System;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;

namespace FrameLog.Contexts
{
    public interface IFrameLogContext
    {
        Type UnderlyingContextType { get; }
        MetadataWorkspace Workspace { get; }
    }

    public partial interface IFrameLogContext<TChangeSet, TPrincipal> : IHistoryContext<TChangeSet, TPrincipal>, IFrameLogContext
        where TChangeSet : IChangeSet<TPrincipal>
    {
        ObjectStateManager ObjectStateManager { get; }

        /// <summary>
        /// Detect all pending changes to tracked objects
        /// </summary>
        void DetectChanges();

        /// <summary>
        /// Save changes inline with the specified save options.
        /// The available save options are detect/save, save/accept, or detect/save/accept.
        /// </summary>
        /// <remarks>
        /// It IS NOT expected that any custom user logic will be executed during this save.
        /// This call is used internally by FrameLog to save the actual log objects. Because
        /// of this, invoking custom user logic is not intended or desired.
        /// </remarks>
        /// <param name="saveOptions"></param>
        /// <returns></returns>
        int SaveChanges(SaveOptions saveOptions);
        
        /// <summary>
        /// Perform a save and accept only, no change detection should be performed here.
        /// </summary>
        /// <remarks>
        /// It IS expected that any custom user logic will be executed during this save.
        /// This call is used by FrameLog to save the original object changes made by the user.
        /// Because of this, invoking custom user logic is intented.
        /// </remarks>
        /// <param name="onSavingChanges">
        /// A callback that should be invoked AFTER saving the changes but BEFORE accepting them.
        /// The event sequeuce should be as follows.
        /// 
        ///     1. Save the changes
        ///     2. Invoke 'onSavingChanges'
        ///     3. Accept the changes
        /// 
        /// </param>
        /// <returns></returns>
        int SaveAndAcceptChanges(EventHandler onSavingChanges = null);
        
        object GetObjectByKey(EntityKey key);
        void AddChangeSet(TChangeSet changeSet);
    }
}
