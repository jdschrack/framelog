using System;
using System.Data.Entity.Core.Objects;
using FrameLog.Models;
using System.Threading;
using System.Threading.Tasks;

namespace FrameLog.Contexts
{
    public partial interface IFrameLogContext<TChangeSet, TPrincipal>
        where TChangeSet : IChangeSet<TPrincipal>
    {
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
        Task<int> SaveChangesAsync(SaveOptions saveOptions);

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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> SaveAndAcceptChangesAsync(EventHandler onSavingChanges = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
