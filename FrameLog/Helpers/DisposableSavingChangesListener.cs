using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace FrameLog.Helpers
{
    /// <summary>
    /// A disposable event listener for the ObjectContext.SavingChanges event
    /// </summary>
    /// <remarks>
    /// The specified event handler will be triggered if the object context raises
    /// the SavingChanges event during the lifetime of this disposble. Event registration
    /// and deregistration is handled automatically.
    /// </remarks>
    public class DisposableSavingChangesListener : IDisposable
    {
        private readonly ObjectContext context;
        private readonly EventHandler handler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">
        /// The database context in which the SavingChanges event will be listened on
        /// </param>
        /// <param name="eventHandler">
        /// The event handler to call when the SavingChanges event is triggered
        /// </param>

        public DisposableSavingChangesListener(DbContext context, EventHandler eventHandler)
            : this(((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext, eventHandler)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">
        /// The object context in which the SavingChanges event will be listened on
        /// </param>
        /// <param name="eventHandler">
        /// The event handler to call when the SavingChanges event is triggered
        /// </param>
        public DisposableSavingChangesListener(ObjectContext context, EventHandler eventHandler)
        {
            // NOTE: We wrap the event handler in our own event handler. The reason for this is so that 
            //       we can perform an object context check prior to calling the real event handler, 
            //       insuring that we don't fire off for the wrong context.
            this.context = context;
            this.handler = (sender, args) =>
            {
                // If our context didn't trigger this event, ignore it.
                // We are only interested in events raised from our context
                if (sender != context)
                    return;

                // Call the child event handler
                if (eventHandler != null)
                    eventHandler.Invoke(sender, args);
            };

            // Register the event handler
            if (context != null)
                context.SavingChanges += handler;
        }

        public void Dispose()
        {
            // Degregister the eventr handler
            if (context != null)
                context.SavingChanges -= handler;
        }
    }
}
